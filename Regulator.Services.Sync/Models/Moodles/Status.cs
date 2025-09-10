using Regulator.Services.Sync.Enums.Moodles;

namespace Regulator.Services.Sync.Models.Moodles;

public class Status
{
    public required Guid Id { get; set; }
    public required int IconId { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required StatusType Type { get; set; }
    public required string Applier { get; set; }
}