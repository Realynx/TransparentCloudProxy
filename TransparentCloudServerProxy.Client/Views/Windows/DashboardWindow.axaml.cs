using Avalonia.Controls;
using Avalonia.Input;

namespace TransparentCloudServerProxy.Client;

public partial class DashboardWindow : Window {
    public DashboardWindow() {
        InitializeComponent();
    }

    private void DragWindow(object? sender, PointerPressedEventArgs e) {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) {
            BeginMoveDrag(e);
        }
    }
}