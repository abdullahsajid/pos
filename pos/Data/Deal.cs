using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pos.Data
{
    public class Deal
    {
        [PrimaryKey,AutoIncrement]
        public long Id { get; set; }
        public string DealName { get; set; }

        public DateTime OrderDate { get; set; }

        public decimal DealAmount { get; set; }

        [Ignore]
        public List<DealItem> DealItems { get; set; } = new List<DealItem>();
    }
}