using SQLite;

namespace pos.Data
{
    public class Order
    {
        [PrimaryKey,AutoIncrement]
        public long Id { get; set; }

        public string OrderNumber { get; set; }
        public DateTime OrderDate { get; set; }

        public decimal TotalAmount { get; set; }

        public decimal PaymentAmount { get; set; }

        public decimal ChangeAmount { get; set; }

        [Ignore]
        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}