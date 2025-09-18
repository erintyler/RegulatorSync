using System.Collections.Generic;

namespace Regulator.Client.Events.Client.Files;

public record DownloadFiles(HashSet<string> Hashes) : BaseEvent;