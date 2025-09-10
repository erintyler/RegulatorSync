using Regulator.Services.Sync.RequestHandlers.Interfaces;
using Regulator.Services.Sync.Shared.Dtos;

namespace Regulator.Services.Sync.RequestHandlers;

public class RequestHandlerFactory(IServiceProvider serviceProvider) : IRequestHandlerFactory
{
    public IRequestHandler<T> GetHandler<T>() where T : BaseSyncDto
    {
        var handler = serviceProvider.GetService<IRequestHandler<T>>();
        return handler ?? throw new InvalidOperationException($"No handler registered for type {typeof(T).FullName}");
    }
}