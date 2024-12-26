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
}