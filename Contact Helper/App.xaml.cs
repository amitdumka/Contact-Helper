namespace Contact_Helper
{
    public partial class App : Application
    {
        public App()
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1NHaF5cXmVCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdgWXZdcHZVRGFeUUJyW0M=");
            InitializeComponent();

            MainPage = new LoginPage();
        }
    }
}