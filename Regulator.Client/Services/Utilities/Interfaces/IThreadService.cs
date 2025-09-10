using System;
using System.Threading.Tasks;

namespace Regulator.Client.Services.Utilities.Interfaces;

public interface IThreadService
{
    ValueTask RunOnFrameworkThreadAsync(Action action);
    ValueTask<T> RunOnFrameworkThreadAsync<T>(Func<T> func);
}