using Avalonia.Controls;
using Avalonia.Input;

namespace TransparentCloudServerProxy.Client.Views.Windows;

public partial class StartupWindowView : Window {
    public StartupWindowView() {
        InitializeComponent();
    }

    private void DragWindow(object? sender, PointerPressedEventArgs e) {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) {
            BeginMoveDrag(e);
        }
    }
}