using Regulator.Services.Sync.Shared.Dtos;

namespace Regulator.Services.Sync.RequestHandlers.Interfaces;

public interface IRequestHandlerFactory
{
    IRequestHandler<T> GetHandler<T>() where T : BaseSyncDto;
}