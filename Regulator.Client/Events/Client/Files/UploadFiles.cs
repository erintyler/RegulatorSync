using System.Collections.Generic;

namespace Regulator.Client.Events.Client.Files;

public record UploadFiles(Dictionary<nint, HashSet<string>> FilePathsByPointer) : BaseEvent;