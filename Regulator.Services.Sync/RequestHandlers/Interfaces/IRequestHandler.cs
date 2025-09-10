using Regulator.Services.Sync.Shared.Dtos;

namespace Regulator.Services.Sync.RequestHandlers.Interfaces;

public interface IRequestHandler<in T> where T : BaseSyncDto
{
    Task HandleAsync(T dto, CancellationToken cancellationToken = default);
}