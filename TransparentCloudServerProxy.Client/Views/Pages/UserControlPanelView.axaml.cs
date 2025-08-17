using Avalonia.Controls;

using TransparentCloudServerProxy.Client.ViewModels.Pages;

namespace TransparentCloudServerProxy.Client.Views.Pages;

public partial class UserControlPanelView : UserControl {
    public UserControlPanelView() {
        InitializeComponent();
    }

    private void DataGrid_CellEditEnded(object? sender, Avalonia.Controls.DataGridCellEditEndedEventArgs e) {
        if (DataContext is UserControlPanelViewModel vm) {
            vm.ApplyChangesVisible = true;
        }
    }
}