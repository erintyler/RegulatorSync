using System;
using System.Threading.Tasks;
using Glamourer.Api.Enums;

namespace Regulator.Client.Services.Interop.Interfaces;

public interface IGlamourerApiClient : IDisposable
{
    Task<GlamourerApiEc> ResetCustomizationsAsync(string syncCode);
    Task<GlamourerApiEc> ApplyCustomizationsAsync(string syncCode, string customizations);
    Task<string> RequestCustomizationsAsync();
}