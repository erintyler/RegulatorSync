namespace Regulator.Services.Files.Shared.Dtos.Requests;

public record GetPresignedUploadUrlRequestDto(string FileExtension, long Size);