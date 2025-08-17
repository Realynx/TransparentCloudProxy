using Avalonia.Controls;

using TransparentCloudServerProxy.Client.ViewModels.Pages;

namespace TransparentCloudServerProxy.Client.Views.Pages;

public partial class UserControlPanelView : UserControl {
    public UserControlPanelView() {
        InitializeComponent();
    }

    private void ApplChanges() {
        if (DataContext is UserControlPanelViewModel vm) {
            vm.ApplyChangesVisible = true;
        }
    }

    private void DataGrid_CellEditEnded(object? sender, Avalonia.Controls.DataGridCellEditEndedEventArgs e) {
        ApplChanges();
    }

    private void DataGrid_BeginningEdit(object? sender, Avalonia.Controls.DataGridBeginningEditEventArgs e) {
        ApplChanges();
    }

    private void ComboBox_DropDownClosed(object? sender, System.EventArgs e) {
        ApplChanges();
    }

    private void DataGrid_RowEditEnding(object? sender, Avalonia.Controls.DataGridRowEditEndingEventArgs e) {
        ApplChanges();
    }

    private void DataGrid_RowEditEnded(object? sender, Avalonia.Controls.DataGridRowEditEndedEventArgs e) {
        ApplChanges();
    }

    private void DataGrid_CellEditEnding(object? sender, Avalonia.Controls.DataGridCellEditEndingEventArgs e) {
        ApplChanges();
    }

    private void NumericUpDown_ValueChanged(object? sender, Avalonia.Controls.NumericUpDownValueChangedEventArgs e) {
        ApplChanges();
    }

    private void NumericUpDown_ValueChanged_1(object? sender, Avalonia.Controls.NumericUpDownValueChangedEventArgs e) {
        ApplChanges();
    }
}