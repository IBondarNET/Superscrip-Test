using TestTask.API.Controllers;
using TestTask.Data.Entities;
using TestTask.Services;

namespace TestTask.API.Tests;

public class ReportTests: BaseTest
{
    private const int ItemCost = 0;
    private const int InitialUserBalance = 100;
    private const string ItemA = "Item A";
    private const string ItemB = "Item B";
    private const string ItemC = "Item C";
    private const string ItemD = "Item D";
    private const string ItemE = "Item E";
    
    protected override async Task SetupBase()
    {
        var userList = new List<User>()
        {
            new()
            {
                Id = 1,
                Balance = InitialUserBalance,
                Email = "test@test.com"
            },
            new()
            {
                Id = 2,
                Balance = InitialUserBalance,
                Email = "test2@test.com"
            }
        };
        
        var items = new List<Item>
        {
            new Item { Id = 1, Name = ItemA, Cost = ItemCost },
            new Item { Id = 2, Name = ItemB, Cost = ItemCost },
            new Item { Id = 3, Name = ItemC, Cost = ItemCost },
            new Item { Id = 4, Name = ItemD, Cost = ItemCost },
            new Item { Id = 5, Name = ItemE, Cost = ItemCost }
        };

        var userItems2022 = new List<UserItem>
        {
            new UserItem { UserId = 1, ItemId = 1, PurchaseDate = new DateTime(2022, 1, 1) },
            new UserItem { UserId = 1, ItemId = 1, PurchaseDate = new DateTime(2022, 1, 1) },
            new UserItem { UserId = 1, ItemId = 1, PurchaseDate = new DateTime(2022, 1, 1) },
            new UserItem { UserId = 1, ItemId = 1, PurchaseDate = new DateTime(2022, 1, 1) },
            
            new UserItem { UserId = 1, ItemId = 2, PurchaseDate = new DateTime(2022, 1, 1) },
            new UserItem { UserId = 1, ItemId = 2, PurchaseDate = new DateTime(2022, 1, 1) },
            
            new UserItem { UserId = 2, ItemId = 1, PurchaseDate = new DateTime(2022, 1, 2) },
            new UserItem { UserId = 2, ItemId = 1, PurchaseDate = new DateTime(2022, 1, 2) },
            new UserItem { UserId = 2, ItemId = 1, PurchaseDate = new DateTime(2022, 1, 2) },
            new UserItem { UserId = 2, ItemId = 1, PurchaseDate = new DateTime(2022, 1, 2) },
            new UserItem { UserId = 2, ItemId = 1, PurchaseDate = new DateTime(2022, 1, 2) },
            
            new UserItem { UserId = 2, ItemId = 3, PurchaseDate = new DateTime(2022, 1, 2) },
            new UserItem { UserId = 2, ItemId = 3, PurchaseDate = new DateTime(2022, 1, 2) },
            new UserItem { UserId = 2, ItemId = 3, PurchaseDate = new DateTime(2022, 1, 2) },

        };
        
        var userItems2023 = new List<UserItem>
        {
            new UserItem { UserId = 1, ItemId = 1, PurchaseDate = new DateTime(2023, 1, 1) },
            new UserItem { UserId = 1, ItemId = 1, PurchaseDate = new DateTime(2023, 1, 1) },
            new UserItem { UserId = 1, ItemId = 1, PurchaseDate = new DateTime(2023, 1, 1) },
            new UserItem { UserId = 1, ItemId = 1, PurchaseDate = new DateTime(2023, 1, 1) },
            
            new UserItem { UserId = 1, ItemId = 2, PurchaseDate = new DateTime(2023, 1, 1) },
            new UserItem { UserId = 1, ItemId = 2, PurchaseDate = new DateTime(2023, 1, 1) },
            
            new UserItem { UserId = 1, ItemId = 4, PurchaseDate = new DateTime(2023, 1, 1) },
            new UserItem { UserId = 1, ItemId = 4, PurchaseDate = new DateTime(2023, 1, 1) },
            new UserItem { UserId = 1, ItemId = 4, PurchaseDate = new DateTime(2023, 1, 1) },
            
            new UserItem { UserId = 2, ItemId = 1, PurchaseDate = new DateTime(2023, 1, 2) },
            new UserItem { UserId = 2, ItemId = 1, PurchaseDate = new DateTime(2023, 1, 2) },
            new UserItem { UserId = 2, ItemId = 1, PurchaseDate = new DateTime(2023, 1, 2) },
            
            new UserItem { UserId = 2, ItemId = 3, PurchaseDate = new DateTime(2023, 1, 2) },
            new UserItem { UserId = 2, ItemId = 3, PurchaseDate = new DateTime(2023, 1, 2) },
            new UserItem { UserId = 2, ItemId = 3, PurchaseDate = new DateTime(2023, 1, 2) },
            
            new UserItem { UserId = 2, ItemId = 4, PurchaseDate = new DateTime(2023, 1, 2) },
            new UserItem { UserId = 2, ItemId = 4, PurchaseDate = new DateTime(2023, 1, 2) },
        };
        
        await Context.DbContext.Users.AddRangeAsync(userList);
        await Context.DbContext.Items.AddRangeAsync(items);
        await Context.DbContext.UserItems.AddRangeAsync(userItems2022);
        await Context.DbContext.UserItems.AddRangeAsync(userItems2023);

        await Context.DbContext.SaveChangesAsync();
    }

    [Test]
    public async Task GerReportAsync_ShouldNotNull()
    {
        // Act
        var result = await Rait<MarketController>().Call(controller => controller.GerReportAsync());
        
        // Assert
        Assert.IsNotNull(result);
        var items = result.Value;
        Assert.IsNotNull(items);
    }
    
    [Test]
    public async Task GerReportAsync_ShouldGetExpectedCount()
    {
        // Act
        var result = await Rait<MarketController>().Call(controller => controller.GerReportAsync());
        
        // Assert
        var items = result!.Value;
        var yearCount = items!.GroupBy(i => i.Year).Count();
        Assert.That(yearCount, Is.EqualTo(2));
        Assert.That(items!.Count, Is.EqualTo(6));
    }
    
    [Test]
    public async Task GerReportAsync_ShouldEqualExpectedList()
    {
        // Act
        var result = await Rait<MarketController>().Call(controller => controller.GerReportAsync());
        
        // Assert
        var expectedItems = new List<MarketService.ReportDto>()
        {
            new ()
            {
                Year = 2022,
                ItemName = ItemA,
                Count = 5
            },
            new ()
            {
                Year = 2022,
                ItemName = ItemC,
                Count = 3
            },
            new ()
            {
                Year = 2022,
                ItemName = ItemB,
                Count = 2
            },
            new ()
            {
                Year = 2023,
                ItemName = ItemA,
                Count = 4
            },
            new ()
            {
                Year = 2023,
                ItemName = ItemC,
                Count = 3
            },
            new ()
            {
                Year = 2023,
                ItemName = ItemD,
                Count = 3
            },
        };
        
        var items = result!.Value;
        var orderedActualItems = items!.OrderBy(x => x.ItemName);
        var orderedExpectedItems = expectedItems.OrderBy(x => x.ItemName);
        Assert.That(orderedActualItems.SequenceEqual(orderedExpectedItems), Is.True);
    }
}