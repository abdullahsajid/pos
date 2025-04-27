using SQLite;
using System.Diagnostics;

namespace pos.Data;

public class DB_Services : IAsyncDisposable
{
	private readonly SQLiteAsyncConnection _database;
    public DB_Services()
	{
		var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "pos.db");
        Console.WriteLine($"DB Path: {dbPath}");
        _database = new SQLiteAsyncConnection(dbPath,SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.SharedCache);
    }

    public async Task initDatabase()
    {
        await _database.CreateTableAsync<ProductItem>();
        await _database.CreateTableAsync<MenuCategory>();
        await _database.CreateTableAsync<Order>();
        await _database.CreateTableAsync<OrderItem>();
        await _database.CreateTableAsync<Deal>();
        await _database.CreateTableAsync<DealItem>();
    }

    public async Task<int> AddCategory(MenuCategory category)
    {
        return await _database.InsertAsync(category);
    }
    public async Task<int> AddProduct(ProductItem product)
    {
        return await _database.InsertAsync(product);
    }

    public async Task<int> AddOrder(Order order)
    {
        return await _database.InsertAsync(order);
    }

    public async Task<int> AddOrderItem(OrderItem orderItem)
    {
        return await _database.InsertAsync(orderItem);
    }

    public async Task<string> GetNextOrderNumber()
    {
        var orderCounter = await _database.Table<Order>().CountAsync();

        return orderCounter.ToString();
    }

    public async Task<int> AddDeal(Deal deal)
    {
        return await _database.InsertAsync(deal);
    }

    public async Task<int> AddDealItem(DealItem dealItem)
    {
        return await _database.InsertAsync(dealItem);
    }

    public async Task<List<Deal>> GetDeal()
    {
        return await _database.Table<Deal>().ToListAsync();
    }

    public async Task<List<DealItem>> GetDealItem(long dealId)
    {
        return await _database.Table<DealItem>().Where(x => x.DealId == dealId).ToListAsync();
    }

    public async Task<List<Order>> GetOrder()
    {
        return await _database.Table<Order>().ToListAsync();
    }

    public async Task<List<OrderItem>> GetOrderItems(long orderId)
    {
        return await _database.Table<OrderItem>().Where(x => x.OrderId == orderId).ToListAsync();
    }

    public async Task<List<MenuCategory>> GetCategory()
    {
        return await _database.Table<MenuCategory>().ToListAsync();
    }

    public async Task<int> UpdateCategory(MenuCategory category)
    {
        var categories = await _database.Table<MenuCategory>().Where(x => x.Id == category.Id).FirstOrDefaultAsync();
        if(categories == null)
        {
            return 0;
        }
        categories.Name = category.Name;
        try
        {
            return await _database.UpdateAsync(categories);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error updating category: {ex.Message}");
            return 0;
        }
    }

    public async Task<List<ProductItem>> SeachProuctsAsync(string searchValue)
    {
        if(searchValue != null)
        {
            return await _database.Table<ProductItem>().Where(p => p.Name.ToLower().Contains(searchValue.ToLower())).ToListAsync();
        }
        else
        {
            return await _database.Table<ProductItem>().ToListAsync();
        }
    }
    public async Task<int> DeleteCategory(MenuCategory category)
    {
        var categories = await _database.Table<MenuCategory>().Where(x => x.Id == category.Id).FirstOrDefaultAsync();
        if (categories == null)
        {
            return 0;
        }
        var deleted = await _database.DeleteAsync(categories);
        if (deleted == 0)
        {
            return 0;
        }
        return deleted;
    }

    public async Task<List<ProductItem>> GetProducts()
    {
        return await _database.Table<ProductItem>().ToListAsync();
    }

    public async Task<List<ProductItem>> GetProductsByCategory(int categoryId)
    {
        return await _database.Table<ProductItem>().Where(x => x.CategoryId == categoryId).ToListAsync();
    }

    public async Task<List<Deal>> GetDealByCategory(int categoryId)
    {
        return await _database.Table<Deal>().Where(x => x.CategoryId == categoryId).ToListAsync();
    }

    public async Task<List<ProductItem>> GetProductAndCategoryById(int menuId)
    {
        var query = @"SELECT p.*, c.Name AS CategoryName
                      FROM ProductItem p
                      INNER JOIN MenuCategory c ON p.CategoryId = c.Id
                        WHERE p.Id = ?";
        return await _database.QueryAsync<ProductItem>(query,menuId);
    }

    public async Task<int> UpdateProductById(ProductItem product)
    {
        var products = await _database.Table<ProductItem>().Where(x => x.Id == product.Id).FirstOrDefaultAsync();
        if (products == null)
        {
            return 0;
        }
        products.Name = product.Name;
        products.Price = product.Price;
        return await _database.UpdateAsync(products);
    }

    public async Task<int> UpdateDealById(Deal deal)
    {
        var deals = await _database.Table<Deal>().Where(x => x.Id == deal.Id).FirstOrDefaultAsync();
        if (deals == null)
        {
            return 0;
        }
        deals.DealName = deal.DealName;
        deals.DealAmount = deal.DealAmount;
        return await _database.UpdateAsync(deals);
    }

    public async Task<int> DeleteProductById(ProductItem item)
    {
        var products = await _database.Table<ProductItem>().Where(x => x.Id == item.Id).FirstOrDefaultAsync();
        if (products == null)
        {
            return 0;
        }
        var deleted = await _database.DeleteAsync(products);
        if (deleted == 0)
        {
            return 0;
        }
        return deleted;
    }

    public async Task<int> DeleteDealById(Deal deal)
    {
        var deals = await _database.Table<Deal>().Where(x => x.Id == deal.Id).FirstOrDefaultAsync();
        if (deals == null)
        {
            return 0;
        }
        var dealItems = await _database.Table<DealItem>().Where(x => x.DealId == deal.Id).ToListAsync();

        if(dealItems != null)
        {
            foreach (var item in dealItems)
            {
                await _database.DeleteAsync(item);
            }
        }
        
        return await _database.DeleteAsync(deals);
    }

    public async ValueTask DisposeAsync()
    {
        if (_database != null)
            await _database.CloseAsync();
    }
}