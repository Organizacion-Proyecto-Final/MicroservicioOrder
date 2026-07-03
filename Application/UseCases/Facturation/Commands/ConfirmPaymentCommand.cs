using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases.Facturation.Commands
{
    public class ConfirmPaymentCommand
    {
        public int FacturaId { get; set; }
    }
}
