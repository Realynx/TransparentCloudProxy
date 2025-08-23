using Avalonia.Input;

using SukiUI.Controls;

namespace TransparentCloudServerProxy.Client.Views.Windows;

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