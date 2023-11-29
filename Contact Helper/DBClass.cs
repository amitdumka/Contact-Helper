using SQLite;

namespace Contact_Helper
{
    public static class Constants
    {
        public const string DatabaseFilename = "ContantCleaner.db3";

        public const SQLite.SQLiteOpenFlags Flags =
            // open the database in read/write mode
            SQLite.SQLiteOpenFlags.ReadWrite |
            // create the database if it doesn't exist
            SQLite.SQLiteOpenFlags.Create |
            // enable multi-threaded database access
            SQLite.SQLiteOpenFlags.SharedCache;

        public static string DatabasePath =>
            Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename);
    }

    public class ContactModel
    {
        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Mobile { get; set; }
        public string? Email { get; set; }
        public string? CompanyName { get; set; }
        public string? Notes { get; set; }

        public string? TrueCallerName { get; set; }
        public string? Remarks { get; set; }
    }

    public class DBClass
    {
        SQLiteAsyncConnection Database;

        public DBClass()
        {
            // Init();
        }

        async Task Init()
        {
            if (Database is not null)
                return;

            Database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
            var result = await Database.CreateTableAsync<ContactModel>();
        }

        public async Task<List<ContactModel>> GetItemsAsync()
        {
            await Init();
            return await Database.Table<ContactModel>().ToListAsync();
        }


        public async Task<ContactModel> GetItemAsync(string phonenumber)
        {
            await Init();
            return await Database.Table<ContactModel>().Where(i => i.Mobile == phonenumber).FirstOrDefaultAsync();
        }

        public async Task<int> SaveItemAsync(ContactModel item)
        {
            await Init();
            if (item.Id != 0)
                return await Database.UpdateAsync(item);
            else
                return await Database.InsertAsync(item);
        }

        public async Task<int> DeleteItemAsync(ContactModel item)
        {
            await Init();
            return await Database.DeleteAsync(item);
        }
    }
}
