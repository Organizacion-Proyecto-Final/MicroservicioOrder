using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.UseCases.Facturation.Commands;

namespace Application.Interfaces
{
    public interface ICreateInvoiceFromOrdersCommandHandler
    {
        Task Handle(CreateInvoiceFromOrdersCommand command, CancellationToken cancellationToken = default);

    }
}
