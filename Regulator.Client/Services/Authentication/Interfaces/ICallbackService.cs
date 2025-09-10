using System;

namespace Regulator.Client.Services.Authentication.Interfaces;

public interface ICallbackService : IDisposable
{
    void StartCallbackListener();
    void StopCallbackListener();
}