using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Factura
    {
        public int Id { get; set; }

        public string TableName { get; set; }

        public DateTime Date { get; set; }

        public bool IsPaid { get; set; }

        public decimal Total { get; set; }

        public List<FacturaDetail> Details { get; set; } = new();
    }
}
