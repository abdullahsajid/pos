using SQLite;

namespace pos.Data
{
    public class MenuCategory
    {
        [PrimaryKey,AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }

    }
}
