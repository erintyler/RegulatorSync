using Regulator.Data.Redis.Models;

namespace Regulator.Data.Redis.Services.Interfaces;

public interface IRepository<in TId, TModel> where TId : notnull where TModel : ICacheModel<TId>
{
    string GetKey(TId id);
    Task<TModel?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);
    Task SaveAsync(TModel model, CancellationToken cancellationToken = default);
    Task DeleteAsync(TId id, CancellationToken cancellationToken = default);
}