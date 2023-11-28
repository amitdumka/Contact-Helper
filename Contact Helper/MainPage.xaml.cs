using System.Text.Json;

namespace Contact_Helper
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
           // treeView.ItemsSource = new SearchData();
        }

        private void OnCounterClicked(object sender, EventArgs e)
        {
            count++;

            if (count == 1)
                CounterBtn.Text = $"Clicked {count} time";
            else
                CounterBtn.Text = $"Clicked {count} times";

            SemanticScreenReader.Announce(CounterBtn.Text);
        }

        private async void searchButton_Clicked(object sender, EventArgs e)
        {
            var data = await TCallerAPI.SearchNumber(PhoneNumberEntry.Text, (bool)ByName.IsChecked, (bool)ByEmail.IsChecked, (bool)ByRaw.IsChecked) ;

            if (data != null)
            {
                if (data.Status != "error" && data.SearchResult!=null && data.SearchResult.Data!=null && data.SearchResult.Data.Data!=null && data.SearchResult.Data.Data.Count>0)
                {
                    var xData=data.SearchResult.Data.Data[0];
                    treeView.Text =await JsonSerializer.SerializeAsync(xData);
                    ///treeView.RefreshView();
                    //treeView.ResetTreeViewItems(xData);
                    //treeView.RefreshView();
                    Notify.NotifyVLong(xData.Name);

                }
            }
        }
    }
}