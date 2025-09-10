using Regulator.Services.Sync.Shared.Dtos.Client.Glamourer;

namespace Regulator.Services.Sync.Shared.Dtos.Server.Glamourer;

/// <summary>
/// Send customizations to the server for the requesting client.
/// Sent in response to <seealso cref="CustomizationRequestDto"/>
/// </summary>
public class NotifyCustomizationsUpdatedDto : BaseSyncDto
{
    public required string GlamourerData { get; set; }
}