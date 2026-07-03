using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases.Facturation.Commands
{
    public sealed class CreateInvoiceFromOrdersCommand
    {
        public List<OrderToInvoiceDto> Orders { get; set; } = [];
    }

    public class OrderToInvoiceDto
    {
        public int TableNumber { get; set; }
        public List<OrderItemDto> Items { get; set; } = [];
    }

    public class OrderItemDto
    {
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
