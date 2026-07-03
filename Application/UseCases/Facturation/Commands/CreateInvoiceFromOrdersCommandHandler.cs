using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OrderService.Application.Interfaces;
using OrderService.Domain.Constants;
using OrderService.Domain.Exceptions;
using Application.Interfaces;
using Domain.Entities;

namespace Application.UseCases.Facturation.Commands
{
    public sealed class CreateInvoiceFromOrdersCommandHandler : ICreateInvoiceFromOrdersCommandHandler
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IFacturationRepository _facturationRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CreateInvoiceFromOrdersCommandHandler> _logger;

        public CreateInvoiceFromOrdersCommandHandler(
            IOrderRepository orderRepository,
            IFacturationRepository facturationRepository,
            IUnitOfWork unitOfWork,
            ILogger<CreateInvoiceFromOrdersCommandHandler> logger)
        {
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _facturationRepository = facturationRepository;
        }

        public async Task Handle(
            CreateInvoiceFromOrdersCommand request,
            CancellationToken cancellationToken = default)
        {
            if (request.Orders == null || request.Orders.Count == 0)
                throw new ValidationException("Debe enviar al menos una orden.");

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                // 🧾 Aplanar todos los items de todas las órdenes
                var details = request.Orders
                    .SelectMany(o => o.Items)
                    .Select(i => new FacturaDetail
                    {
                        Quantity = i.Quantity,
                        ProductName = i.ProductName,
                        Price = i.Price
                    })
                    .ToList();

                // 💰 total
                var total = details.Sum(d => d.Price * d.Quantity);

                // 🧾 factura
                var factura = new Factura
                {
                    TableName = request.Orders.First().TableNumber.ToString(),
                    Date = DateTime.UtcNow,
                    IsPaid = false,
                    Total = total,
                    Details = details
                };

                Console.WriteLine(_facturationRepository == null
                    ? "_facturationRepository es NULL"
                    : "_facturationRepository OK");

                await _facturationRepository.AddAsync(factura, cancellationToken);

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                _logger.LogError(ex, "Error creando factura desde órdenes.");
                throw;
            }
        }
    }
}
