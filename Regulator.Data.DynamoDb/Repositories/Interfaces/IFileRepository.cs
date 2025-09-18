using Regulator.Data.DynamoDb.Enums;
using File = Regulator.Data.DynamoDb.Models.File;

namespace Regulator.Data.DynamoDb.Repositories.Interfaces;

public interface IFileRepository : IRangeKeyRepository<UploadStatus, string, File>
{
}