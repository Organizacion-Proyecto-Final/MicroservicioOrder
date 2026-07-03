using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Enums
{
    public enum PaymentFilter
    {
        All = 0,
        Pending = 1,
        Paid = 2,
        Cancel = 3
    }
}
