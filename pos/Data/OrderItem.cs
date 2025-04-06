using SQLite;

namespace pos.Data
{
    public class OrderItem
    {
        [PrimaryKey,AutoIncrement]
        public long Id { get; set; }
        public long OrderId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime UpdatedDate { get; set; }
        public decimal UnitPrice { get; set; }

        public int Quantity { get; set; }

        public decimal SubTotal { get; set; }
    }
}
