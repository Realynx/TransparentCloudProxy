using Avalonia.Controls;
using Avalonia.Input;

using SukiUI.Controls;

namespace TransparentCloudServerProxy.Client.Views.Windows;

public partial class StartupWindowView : SukiWindow {
    public StartupWindowView() {
        InitializeComponent();
    }

    private void DragWindow(object? sender, PointerPressedEventArgs e) {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) {
            BeginMoveDrag(e);
        }
    }
}