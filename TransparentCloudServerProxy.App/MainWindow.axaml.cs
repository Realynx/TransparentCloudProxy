using Avalonia.Controls;
using Avalonia.Input;

namespace TransparentCloudServerProxy.App {
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
        }

        private void DragWindow(object? sender, PointerPressedEventArgs e) {
            // Only start drag if left mouse button is pressed
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) {
                BeginMoveDrag(e);
            }
        }
    }
}