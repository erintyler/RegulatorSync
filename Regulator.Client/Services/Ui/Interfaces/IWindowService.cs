using Dalamud.Interface.Windowing;

namespace Regulator.Client.Services.Ui.Interfaces;

public interface IWindowService
{
    void ToggleWindow<T>() where T : Window;
    void ShowWindow<T>() where T : Window;
    void HideWindow<T>() where T : Window;
}