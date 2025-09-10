using Regulator.Services.Sync.Shared.Dtos.Server.Glamourer;

namespace Regulator.Services.Sync.Shared.Dtos.Client.Glamourer;

/// <summary>
/// Receives customizations for a paired client.
/// Sent to the client in response to <seealso cref="RequestCustomizationsDto"/>
/// </summary>
public class ReceiveCustomizationsDto : BaseSyncDto
{
    public required string GlamourerData { get; set; }
}