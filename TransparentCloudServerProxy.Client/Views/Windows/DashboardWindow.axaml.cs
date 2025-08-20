using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

using SukiUI.Controls;
using SukiUI.Theme.Shadcn;

namespace TransparentCloudServerProxy.Client;

public partial class DashboardWindow : SukiWindow {
    public DashboardWindow() {
        InitializeComponent();

    }

    private void DragWindow(object? sender, PointerPressedEventArgs e) {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) {
            BeginMoveDrag(e);
        }
    }
}