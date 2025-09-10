using System;
using System.Threading.Tasks;

namespace Regulator.Client.Services.Utilities.Interfaces;

public interface IDebounceService
{
    void Debounce(string key, Action action, int milliseconds = 300);
    void DebounceAsync(string key, Func<Task> action, int milliseconds = 300);
}