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
        await _database.CreateTableAsync<ProductModel>();
        await _database.CreateTableAsync<CategoryModel>();
    }

    public async Task<int> AddCategory(CategoryModel category)
    {
        return await _database.InsertAsync(category);
    }
    public async Task<int> AddProduct(ProductModel product)
    {
        return await _database.InsertAsync(product);
    }

    public async Task<List<CategoryModel>> GetCategory()
    {
        return await _database.Table<CategoryModel>().ToListAsync();
    }

    public async Task<List<ProductModel>> GetProducts()
    {
        return await _database.Table<ProductModel>().ToListAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_database != null)
            await _database.CloseAsync();
    }
}