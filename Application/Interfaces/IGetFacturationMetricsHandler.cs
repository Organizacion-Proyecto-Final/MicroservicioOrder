using Application.DTOs;

namespace Application.Interfaces;

public interface IGetFacturationMetricsHandler
{
    Task<FacturationMetricsDto> Handle(CancellationToken cancellationToken = default);
}
