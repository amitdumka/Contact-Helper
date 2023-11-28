using CommunityToolkit.Maui.Alerts;
using Syncfusion.Maui.DataForm;
using System.ComponentModel.DataAnnotations;

namespace Contact_Helper
{
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();
        }
        public LoginPage(AppShell ss)
        {
            InitializeComponent();

        }
    }
    public class LoginFormModel
    {
        [Display(Prompt = "Enter User name", Name = "User Name")]
        [Phone]
        public string UserName { get; set; } = "AmitKumar";

        [Display(Name = "OTP")]
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Enter the password")]
        [MaxLength(6)]
        public string Password { get; set; } = "123456";
    }
    public class SignInFormViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SignInFormViewModel" /> class.
        /// </summary>
        public SignInFormViewModel()
        {
            this.LoginFormModel = new LoginFormModel();
        }

        /// <summary>
        /// Gets or sets the login form model.
        /// </summary>
        public LoginFormModel LoginFormModel { get; set; }
    }
    public class LoginFormBehavior : Behavior<ContentPage>
    {

        /// <summary>
        /// Holds the data form object.
        /// </summary>
        private SfDataForm dataForm;

        /// <summary>
        /// Holds the login button instance.
        /// </summary>
        private Button loginButton, otpButton;

        protected override void OnAttachedTo(ContentPage bindable)
        {
            base.OnAttachedTo(bindable);
            this.dataForm = bindable.FindByName<SfDataForm>("loginForm");
            this.dataForm.GenerateDataFormItem += this.OnGenerateDataFormItem;

            this.loginButton = bindable.FindByName<Button>("loginButton");
            this.otpButton = bindable.FindByName<Button>("otpButton");

            if (this.loginButton != null)
            {
                this.loginButton.Clicked += OnLoginButtonCliked;
            }
            if (this.otpButton != null)
            {
                this.otpButton.Clicked += OnOTPButtonCliked;
            }
        }

        /// <summary>
        /// Invokes on each data form item generation.
        /// </summary>
        /// <param name="sender">The data form.</param>
        /// <param name="e">The event arguments.</param>
        private void OnGenerateDataFormItem(object sender, GenerateDataFormItemEventArgs e)
        {
            if (e.DataFormItem != null && e.DataFormItem.FieldName == nameof(LoginFormModel.UserName) && e.DataFormItem is DataFormTextEditorItem textItem)
            {
                textItem.Keyboard = Keyboard.Text;
            }
        }

        /// <summary>
        /// Invokes on login button click.
        /// </summary>
        /// <param name="sender">The login button.</param>
        /// <param name="e">The event arguments.</param>
        private async void OnLoginButtonCliked(object sender, EventArgs e)
        {
            if (this.dataForm != null && App.Current?.MainPage != null)
            {
                if (this.dataForm.Validate())
                {
                    var usr = dataForm.DataObject as LoginFormModel;
                    var result = await TCallerAPI.DoLoginCheckAsync(usr.UserName);

                    if (result)
                    {
                        Notify.NotifyVShort(" Already LoggedIn");
                        //TODO: Move to Search Page
                        App.Current.MainPage = new MainPage(); 
                    }

                    else
                    {
                        Notify.NotifyVShort("Need to Login!");
                        var result2 = await TCallerAPI.DoLogin(usr.UserName);
                        if (!result2)
                        {
                            Notify.NotifyVLong("Error Occured , Try again!");
                        }
                        else
                        {
                            Notify.NotifyVLong("Entry OTP to Login");
                            var bb = sender as Button;
                            bb.IsEnabled = false;
                        }
                    }


                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("", "Please enter the required details", "OK");
                }
            }
        }


        private async void OnOTPButtonCliked(object sender, EventArgs e)
        {
            if (this.dataForm != null && App.Current?.MainPage != null)
            {
                if (this.dataForm.Validate())
                {
                    var usr = dataForm.DataObject as LoginFormModel;
                    var result = await TCallerAPI.VerifyOTP(usr.Password);
                    
                    if (result)
                    {
                        App.Current.MainPage = new MainPage();
                    }
                    else Notify.NotifyVShort("Error, Occured! or OTP is incorrect!");

                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("", "Please enter the required details", "OK");
                }
            }
        }

        protected override void OnDetachingFrom(ContentPage bindable)
        {
            base.OnDetachingFrom(bindable);
            if (this.loginButton != null)
            {
                this.loginButton.Clicked -= OnLoginButtonCliked;
            }

            if (this.dataForm != null)
            {
                this.dataForm.GenerateDataFormItem -= this.OnGenerateDataFormItem;
            }
        }
    }


    public static class Notify
    {
        public static void NotifyLong(string msg)
        {
            Toast.Make(msg, CommunityToolkit.Maui.Core.ToastDuration.Long).Show();
        }

        public static void NotifyShort(string msg)
        {
            Toast.Make(msg, CommunityToolkit.Maui.Core.ToastDuration.Short).Show();
        }

        public static void NotifyVLong(string msg)
        {
            if (!string.IsNullOrEmpty(msg))
            {
                Toast.Make(msg, CommunityToolkit.Maui.Core.ToastDuration.Long).Show();
                ASpeak.Speak(msg);
            }
        }

        public static void NotifyVShort(string msg)
        {
            Toast.Make(msg, CommunityToolkit.Maui.Core.ToastDuration.Short).Show();
            ASpeak.Speak(msg);
        }
    }

    public class ASpeak
    {
        public static async void Speak(string text) => await TextToSpeech.Default.SpeakAsync(text);

        private CancellationTokenSource cts;

        public async Task SpeakNowDefaultSettingsAsync()
        {
            cts = new CancellationTokenSource();
            await TextToSpeech.Default.SpeakAsync("Hello World", cancelToken: cts.Token);

            // This method will block until utterance finishes.
        }

        // Cancel speech if a cancellation token exists & hasn't been already requested.
        public void CancelSpeech()
        {
            if (cts?.IsCancellationRequested ?? true)
                return;

            cts.Cancel();
        }

        private bool isBusy = false;

        public void SpeakMultiple(List<string> texts)
        {
            isBusy = true;
            Task[] tList = new Task[texts.Count];

            int i = 0;
            foreach (var text in texts)
                tList[i++] = (TextToSpeech.Default.SpeakAsync(text));

            Task.WhenAll(tList).ContinueWith((t) => { isBusy = false; },
                TaskScheduler.FromCurrentSynchronizationContext());
        }
    }

}
