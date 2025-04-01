using pos.Data;
using pos.Models;
using SQLite;

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
    }

    public async Task<int> AddCategory(MenuCategory category)
    {
        return await _database.InsertAsync(category);
    }
    public async Task<int> AddProduct(ProductItem product)
    {
        return await _database.InsertAsync(product);
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

    public async ValueTask DisposeAsync()
    {
        if (_database != null)
            await _database.CloseAsync();
    }
}