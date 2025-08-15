using Avalonia.Controls;

using TransparentCloudServerProxy.App.Controllers;

namespace TransparentCloudServerProxy.App;

public partial class DashboardView : UserControl {
    private readonly DashboardController _dashboardController;

    public DashboardView(DashboardController dashboardController) {
        InitializeComponent();
        _dashboardController = dashboardController;
    }
}