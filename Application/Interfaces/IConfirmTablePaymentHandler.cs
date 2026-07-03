namespace Application.Interfaces;

public interface IConfirmTablePaymentHandler
{
    Task<bool> Handle(
        string tableName,
        CancellationToken cancellationToken = default);
}
