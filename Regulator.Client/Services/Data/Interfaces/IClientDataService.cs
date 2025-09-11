using Regulator.Client.Models;

namespace Regulator.Client.Services.Data.Interfaces;

public interface IClientDataService
{
    ClientData? GetClientData();
    void SaveClientData(ClientData clientData);
}