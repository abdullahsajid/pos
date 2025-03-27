using SQLite;

namespace pos.Data
{
    public class ProductItem
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Price { get; set; }
        public int Stock { get; set; }
        public string Barcode { get; set; }

        public int CategoryId { get; set; }
    }
}
