using Syncfusion.XlsIO;
using System.Data;
using System.Drawing;
using System.Reflection;
using System.Text.Json;
using IApplication = Syncfusion.XlsIO.IApplication;

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

        }

        private async void searchButton_Clicked(object sender, EventArgs e)
        {
            var data = await TCallerAPI.SearchNumber(PhoneNumberEntry.Text, (bool)ByName.IsChecked, (bool)ByEmail.IsChecked, (bool)ByRaw.IsChecked);

            if (data != null)
            {
                if (data.Status != "error" && data.SearchResult != null && data.SearchResult.Data != null && data.SearchResult.Data.Data != null && data.SearchResult.Data.Data.Count > 0)
                {
                    var xData = data.SearchResult.Data.Data[0];
                    treeView.Text = JsonSerializer.Serialize(xData);
                    Notify.NotifyVLong(xData.Name);

                }
            }
        }

        public void ReadExcelFile(string filename)
        {

            //Creates a new instance for ExcelEngine
            using ExcelEngine excelEngine = new ExcelEngine();
            //Initialize IApplication
            Syncfusion.XlsIO.IApplication application = excelEngine.Excel;
            FileStream fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            //Loads or open an existing workbook through Open method of IWorkbooks
            IWorkbook workbook = application.Workbooks.Open(fileStream);
            IWorksheet worksheet = workbook.Worksheets[0];
            //Read data from the worksheet and Export to the DataTable
            DataTable customersTable = worksheet.ExportDataTable(worksheet.UsedRange, ExcelExportDataTableOptions.ColumnNames);



        }

        private void ExcelButton_Clicked(object sender, EventArgs e)
        {

        }
        public async Task<FileResult> PickAndShow(PickOptions? options)
        {
            try
            {
                var result = await FilePicker.Default.PickAsync(options);
                if (result != null)
                {
                    //if (result.FileName.EndsWith("jpg", StringComparison.OrdinalIgnoreCase) ||
                    //    result.FileName.EndsWith("png", StringComparison.OrdinalIgnoreCase))
                    //{
                    //    using var stream = await result.OpenReadAsync();
                    //    var image = ImageSource.FromStream(() => stream);
                    //}
                }

                return result;
            }
            catch (Exception ex)
            {
                // The user canceled or something went wrong
                return null;
            }

            return null;
        }

        private async void FileSelect_Clicked(object sender, EventArgs e)
        {
            PickOptions options = new()
            {
                PickerTitle = "Please select a excel file",

            };
            var result = await FilePicker.Default.PickAsync(options);
            if (result != null)
            {
                if (result.FileName.EndsWith("xls", StringComparison.OrdinalIgnoreCase) || result.FileName.EndsWith("xlsx", StringComparison.OrdinalIgnoreCase))
                {
                    fileEntry.Text = result.FullPath;
                }
                else { Notify.NotifyVShort($"{result.FileName} is not valid, select only excel File"); }
            }
        }

        private void ExcelToJson(string excelFileName, string jsonFileName, bool isSchema)
        {
            using (ExcelEngine excelEngine = new ExcelEngine())
            {
                IApplication application = excelEngine.Excel;
                application.DefaultVersion = ExcelVersion.Xlsx;
                FileStream fileStream = new FileStream(excelFileName, FileMode.Open, FileAccess.Read);
                IWorkbook workbook = application.Workbooks.Open(fileStream);
                IWorksheet worksheet = workbook.Worksheets[0];

                //Saves the workbook to a JSON filestream, as schema by default
                FileStream stream = new FileStream(jsonFileName, FileMode.Create, FileAccess.ReadWrite);
                if (isSchema)
                {
                    workbook.SaveAsJson(stream);
                }
                else
                {
                    workbook.SaveAsJson(stream, isSchema);
                }
                stream.Dispose();

            }
        }
    }
}