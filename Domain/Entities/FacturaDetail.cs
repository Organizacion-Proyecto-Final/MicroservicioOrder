using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class FacturaDetail
    {
        public int Id { get; set; }

        public int Quantity { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public decimal Price { get; set; }

        // FK
        public int FacturaId { get; set; }

        // navegación
        public Factura Factura { get; set; } = null!;
    }
}
