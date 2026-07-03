using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.Application.UseCases;
using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using Application.UseCases.Facturation.Commands;
using Application.UseCases.Facturation.Queries;


using OrderService.Domain.Exceptions;
using OrderService.Presentation.Authorization;
using System.Security.Claims;
using System.Reflection.Metadata;

namespace OrderService.Controllers
{
    [ApiController]
    [Route("api/v1/orders")]
   // [Authorize]
    public class FacturationAndMetricalController : ControllerBase
    {
        private readonly IGetFacturasHandler _getFacturasHandler;
        private readonly IGetFacturationMetricsHandler _getFacturationMetricsHandler;
        private readonly IConfirmPaymentHandler _confirmPaymentHandler;
        private readonly IConfirmTablePaymentHandler _confirmTablePaymentHandler;
        private readonly ICreateInvoiceFromOrdersCommandHandler _createInvoiceHandler;

        public FacturationAndMetricalController(
            IGetFacturasHandler getFacturasHandler,
            IGetFacturationMetricsHandler getFacturationMetricsHandler,
            IConfirmPaymentHandler confirmPaymentHandler,
            IConfirmTablePaymentHandler confirmTablePaymentHandler,
            ICreateInvoiceFromOrdersCommandHandler createInvoiceFromOrdersCommandHandler)
        {
            _getFacturasHandler = getFacturasHandler;
            _getFacturationMetricsHandler = getFacturationMetricsHandler;
            _confirmPaymentHandler = confirmPaymentHandler;
            _confirmTablePaymentHandler = confirmTablePaymentHandler;
            _createInvoiceHandler = createInvoiceFromOrdersCommandHandler;
            
        }


        [HttpGet("facturas")]
        public async Task<IActionResult> GetFacturas([FromQuery] GetFacturasQuery query)
        {
            var result = await _getFacturasHandler.Handle(query);
            return Ok(result);
        }

        [HttpGet("facturas/metrics")]
        public async Task<IActionResult> GetFacturationMetrics(
            CancellationToken cancellationToken)
        {
            var result = await _getFacturationMetricsHandler.Handle(cancellationToken);
            return Ok(result);
        }

        [HttpPut("facturas/{id}/pay")]
        public async Task<IActionResult> ConfirmPayment(int id)
        {
            var command = new ConfirmPaymentCommand { FacturaId = id };

            var result = await _confirmPaymentHandler.Handle(command);

            if (!result)
                return NotFound();

            return NoContent();
        }

        [HttpPut("facturas/table/{tableName}/pay")]
        public async Task<IActionResult> ConfirmTablePayment(
            string tableName,
            CancellationToken cancellationToken)
        {
            var result = await _confirmTablePaymentHandler.Handle(
                tableName,
                cancellationToken);

            return result ? NoContent() : NotFound();
        }

        [HttpPost("facturas/from-orders")]
        public async Task<IActionResult> CreateFromOrders(
         [FromBody] CreateInvoiceFromOrdersCommand command,CancellationToken cancellationToken)
        {
            await _createInvoiceHandler.Handle(command, cancellationToken);
            return Ok();
        }
    }
}
