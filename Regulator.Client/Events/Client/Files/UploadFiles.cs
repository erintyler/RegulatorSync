using System.Collections.Generic;
using Regulator.Client.Models.Penumbra;

namespace Regulator.Client.Events.Client.Files;

public record UploadFiles(Dictionary<nint, HashSet<FileReplacement>> FilePathsByPointer) : BaseEvent;