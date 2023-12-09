using CommunityToolkit.Mvvm.ComponentModel;

namespace Contact_Helper.Bases
{
    public partial class BaseViewModel : ObservableObject
    {
        [ObservableProperty]
        public bool isBusy;

        public bool IsNotBusy => !IsBusy;

        internal event Func<string, Task> DoDisplayAlert;

        internal event Func<BaseViewModel, bool, Task> DoNavigate;

        public Task NavigateAsync(BaseViewModel vm, bool showModal = false)
        { return DoNavigate?.Invoke(vm, showModal) ?? Task.CompletedTask; }

        public void OnAppearing()
        {
            //throw new NotImplementedException();
        }

        public void OnDisappearing()
        {
            // throw new NotImplementedException();
        }
    }
}
