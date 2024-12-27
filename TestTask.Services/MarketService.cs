using Microsoft.EntityFrameworkCore;
using TestTask.Data;
using TestTask.Data.Entities;

namespace TestTask.Services;

public class MarketService
{
    private readonly TestDbContext _testDbContext;

    public MarketService(TestDbContext testDbContext)
    {
        _testDbContext = testDbContext;
    }

    public async Task BuyAsync(int userId, int itemId)
    {
        var user = await _testDbContext.Users.FirstOrDefaultAsync(n => n.Id == userId);

        if (user == null)
        {
            throw new Exception("User not found");
        }

        var item = await _testDbContext.Items.FirstOrDefaultAsync(n => n.Id == itemId);

        if (item == null)
        {
            throw new Exception("Item not found");
        }

        if (user.Balance < item.Cost)
        {
            return;
        }

        user.Balance -= item.Cost;
        user.Version = Guid.NewGuid();
        await _testDbContext.UserItems.AddAsync(new UserItem
        {
            UserId = userId,
            ItemId = itemId
        });

        try
        {
            await _testDbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            var entry = ex.Entries.First(a => a.Entity is User);
            var databaseValues = await entry.GetDatabaseValuesAsync();
            var balance = (decimal?)databaseValues?[nameof(User.Balance)];

            if (databaseValues?[nameof(User.Balance)] is null || balance < item.Cost)
            {
                return;
            }

            databaseValues[nameof(User.Balance)] = balance - item.Cost;
            entry.OriginalValues.SetValues(databaseValues);
        }
    }

    public async Task<List<ReportDto>> ReportAsync()
    {
        var entity =  _testDbContext.UserItems.GroupBy(t => new
        {
            t.UserId,
            t.ItemId,
            t.PurchaseDate.Day,
            t.PurchaseDate.Year
        }).Select(g => new
        {
            g.Key.ItemId,
            g.Key.Year,
            Count = g.Count()
        });
        
        var result = await (from e in entity
            join i in _testDbContext.Items on e.ItemId equals i.Id
            select new ReportDto
            {
                Year = e.Year,
                ItemName = i.Name,
                Count = e.Count
            })
            .AsNoTracking()
            .ToListAsync();
        
        return result.GroupBy(t => t.Year)
            .SelectMany(g =>
                g.OrderByDescending(t => t.Count)
                    .DistinctBy(x => x.ItemName)
                    .Take(3))
            .Select(t => new ReportDto
            {
                Year = t.Year,
                ItemName = t.ItemName,
                Count = t.Count
            })
            .ToList();
    }
    
    public class ReportDto : IEquatable<ReportDto>
    {
        public int Year { get; set; }
        
        public string ItemName { get; set; } = null!;
        
        public int Count { get; set; }

        public bool Equals(ReportDto? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Year == other.Year && ItemName == other.ItemName && Count == other.Count;
        }

        public override bool Equals(object? obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ReportDto)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Year, ItemName, Count);
        }
    }
}