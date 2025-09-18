namespace Regulator.Services.Files.Shared.Dtos.Responses;

public record GetPresignedDownloadUrlResponseDto(string Url, string OriginalFileExtension);