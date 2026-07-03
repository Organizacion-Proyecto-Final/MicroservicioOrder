using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Enums;

namespace Application.UseCases.Facturation.Queries
{
    public class GetFacturasQuery
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public PaymentFilter Filter { get; set; } = PaymentFilter.All;
    }
}
