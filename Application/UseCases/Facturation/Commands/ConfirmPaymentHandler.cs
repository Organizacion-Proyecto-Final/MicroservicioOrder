using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces;
using OrderService.Domain.Entities;

namespace Application.UseCases.Facturation.Commands
{
    public class ConfirmPaymentHandler : IConfirmPaymentHandler
    {
        private readonly IFacturationRepository _facturationRepository;

        public ConfirmPaymentHandler(IFacturationRepository facturationRepository)
        {
            _facturationRepository = facturationRepository;
        }

        public async Task<bool> Handle(ConfirmPaymentCommand command)
        {
            var result = await _facturationRepository.MarkAsPaidAsync(command.FacturaId);

            if (!result)
                throw new Exception("Factura no encontrada");

            return true;
        }
    }
}
