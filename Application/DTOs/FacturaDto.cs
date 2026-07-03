using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class FacturaDto
    {
        public int Id { get; set; }
        public string TableName { get; set; }

        public DateTime Date { get; set; }

        public bool IsPaid { get; set; }

        public decimal Total { get; set; }

        public List<FacturaDetailDto> Details { get; set; } = new();
    }

    public class FacturaDetailDto
    {
        public int Quantity { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public decimal Price { get; set; }
    }
}
