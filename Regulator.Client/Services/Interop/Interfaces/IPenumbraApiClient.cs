using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Regulator.Client.Models.Penumbra;

namespace Regulator.Client.Services.Interop.Interfaces;

public interface IPenumbraApiClient : IDisposable
{
    Task<Dictionary<string, HashSet<string>>> GetCustomResourcePathsAsync(bool isDownload);
    Task<Guid> CreateTemporaryCollection(string syncCode);
    Task AddTemporaryMod(string syncCode, FileReplacement fileReplacement);
    Task AddTemporaryMods(string syncCode, IEnumerable<FileReplacement> fileReplacements);
    void AssignTemporaryCollection(string syncCode);
}