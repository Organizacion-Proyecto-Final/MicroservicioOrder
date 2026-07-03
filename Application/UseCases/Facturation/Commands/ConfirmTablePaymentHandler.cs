using Application.Interfaces;

namespace Application.UseCases.Facturation.Commands;

public sealed class ConfirmTablePaymentHandler : IConfirmTablePaymentHandler
{
    private readonly IFacturationRepository _facturationRepository;

    public ConfirmTablePaymentHandler(IFacturationRepository facturationRepository)
    {
        _facturationRepository = facturationRepository;
    }

    public Task<bool> Handle(
        string tableName,
        CancellationToken cancellationToken = default)
    {
        return _facturationRepository.MarkLatestAsPaidByTableAsync(
            tableName,
            cancellationToken);
    }
}
