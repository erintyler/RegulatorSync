using Regulator.Services.Sync.Shared.Dtos.Server.Glamourer;

namespace Regulator.Services.Sync.Shared.Dtos.Client.Glamourer;

/// <summary>
/// Requests the client to send customization data to the server for the requesting client.
/// Sent to the client in response to <seealso cref="RequestCustomizationsDto"/>
/// </summary>
public class CustomizationRequestDto : BaseSyncDto
{
}