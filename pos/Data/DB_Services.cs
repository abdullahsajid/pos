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

    public async Task<List<ProductItem>> GetProducts()
    {
        return await _database.Table<ProductItem>().ToListAsync();
    }

    public async Task<List<ProductItem>> GetProductsByCategory(int categoryId)
    {
        return await _database.Table<ProductItem>().Where(x => x.CategoryId == categoryId).ToListAsync();
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
        Debug.WriteLine("UpdateProductById called"+product.Id);
        var products = await _database.Table<ProductItem>().Where(x => x.Id == product.Id).FirstOrDefaultAsync();
        if (products == null)
        {
            return 0;
        }
        products.Name = product.Name;
        products.Price = product.Price;
        return await _database.UpdateAsync(products);
    }

    public async ValueTask DisposeAsync()
    {
        if (_database != null)
            await _database.CloseAsync();
    }
}