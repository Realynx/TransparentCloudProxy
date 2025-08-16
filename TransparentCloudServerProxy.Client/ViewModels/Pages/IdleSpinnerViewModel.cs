namespace TransparentCloudServerProxy.Client.ViewModels.Pages {
    public class IdleSpinnerViewModel : ViewModel {
        public string Message { get; set; }

        public IdleSpinnerViewModel() {
            Message = "Loading";
        }
        public IdleSpinnerViewModel(string message) {
            Message = message;
        }
    }
}
