using Regulator.Client.Models;
using Regulator.Client.Services.Data.Interfaces;

namespace Regulator.Client.Services.Data;

public class ClientDataService : IClientDataService
{
    private ClientData? _clientData;
    
    public ClientData? GetClientData()
    {
        return _clientData;
    }

    public void SaveClientData(ClientData clientData)
    {
        _clientData = clientData;
    }
}