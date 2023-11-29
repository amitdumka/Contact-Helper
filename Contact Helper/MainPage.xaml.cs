using Syncfusion.XlsIO;
using System.Data;
using System.Text.Json;
using IApplication = Syncfusion.XlsIO.IApplication;

namespace Contact_Helper
{
    public partial class MainPage : ContentPage
    {
        int count = 0;
        DataTable Contacts = null;
        DBClass Db;
        public MainPage()
        {
            InitializeComponent();
            this.Db = new DBClass(); ;
        }
        //public MainPage(DBClass db)
        //{
        //    InitializeComponent();
        //    this.Db = db;
        //    // treeView.ItemsSource = new SearchData();
        //}

        private void OnCounterClicked(object sender, EventArgs e)
        {

        }

        private async void searchButton_Clicked(object sender, EventArgs e)
        {

            if (ByName.IsChecked == true)
            {
                var name = await TCallerAPI.SearchNumberByName(PhoneNumberEntry.Text, (bool)ByRaw.IsChecked);
                if (name != null && name.Status == "Ok")
                {
                    treeView.Text = name.Name;
                    Notify.NotifyVLong(name.Name);


                }
            }

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
            DataTable table = worksheet.ExportDataTable(worksheet.UsedRange, ExcelExportDataTableOptions.ColumnNames);
            Contacts = table;
            for (int i = 0; i < table.Rows.Count; i++)
            {
                DataRow row = table.Rows[i];
                if (row["Mobile"] == null || string.IsNullOrEmpty(row["Mobile"].ToString()))
                {
                    Contacts.Rows.Remove(row);
                }
            }

            dataGrid.BindingContext = this;
            dataGrid.ItemsSource = Contacts;



        }

        private void ExcelButton_Clicked(object sender, EventArgs e)
        {
            ReadExcelFile(fileEntry.Text.Trim());
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

        private async void SearchExcel_Clicked(object sender, EventArgs e)
        {

            if (Contacts != null && Contacts.Rows.Count > 0)
            {
                for (int i = 0; i < Contacts.Rows.Count; i++)
                {
                    DataRow row = Contacts.Rows[i];
                    var ph = row["Mobile"].ToString();
                    if (ph.StartsWith("+"))
                    {


                    }
                    else if (ph.Length > 10 && !ph.StartsWith("+"))
                    {
                        ph = "+" + ph;
                    }
                    else if (ph.Length == 10)
                    {
                        ph = "+91" + ph;
                    }
                    else if (ph.Length < 10)
                    {
                        row["Remarks"] = "#Error#Check Mobile  number, it's length is less then 10 digit. ";
                        ph = null;
                    }
                    if (ph != null)
                    {
                        var name = await TCallerAPI.SearchNumberByName(ph, true);
                        if (name != null && name.Status == "Ok")
                        {
                            row["TrueCallerName"] = name.Name;
                            if (name.Name == "Unknown Name")
                                row["Remarks"] = "Failed";
                            else
                                row["Remarks"] = "OK";
                        }
                        else
                        {
                            row["Remarks"] = $"#Error#{name.Status}#{name.ErrMsg}";
                        }
                        row.AcceptChanges();
                        await Db.SaveItemAsync(new ContactModel
                        {
                            CompanyName = row["CompanyName"].ToString(),
                            Email = row["Email"].ToString(),
                            FirstName = row["First Name"].ToString(),
                            LastName = row["Last Name"].ToString(),
                            TrueCallerName = row["TrueCallerName"].ToString(),
                            Mobile = row["Mobile"].ToString(),
                            Notes = row["Notes"].ToString(),
                            Remarks = row["Remarks"].ToString()
                        });
                        await Task.Delay(2000);
                    }
                    row.AcceptChanges();


                }
            }
        }
    }
}