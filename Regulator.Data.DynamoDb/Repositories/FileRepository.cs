using Amazon.DynamoDBv2.DataModel;
using Regulator.Data.DynamoDb.Enums;
using Regulator.Data.DynamoDb.Repositories.Interfaces;
using File = Regulator.Data.DynamoDb.Models.File;

namespace Regulator.Data.DynamoDb.Repositories;

public class FileRepository(IDynamoDBContext context) : IFileRepository
{
    public async Task<File?> GetAsync(UploadStatus hashKey, string rangeKey, CancellationToken cancellationToken = default)
    {
        return await context.LoadAsync<File>(hashKey, rangeKey, cancellationToken);
    }

    public async Task<IReadOnlyList<File>> GetAllAsync(UploadStatus hashKey, CancellationToken cancellationToken = default)
    {
        var query = context.QueryAsync<File>(hashKey);
        
        return await query.GetRemainingAsync(cancellationToken);
    }

    public async Task UpsertAsync(File entity, CancellationToken cancellationToken = default)
    {
        await context.SaveAsync(entity, cancellationToken);
    }

    public async Task DeleteAsync(UploadStatus hashKey, string rangeKey, CancellationToken cancellationToken = default)
    {
        await context.DeleteAsync<File>(hashKey, rangeKey, cancellationToken);
    }
}