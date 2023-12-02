using SQLite;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using MixERP.Net.VCards.Types;
using MixERP.Net.VCards.Models;

namespace Contact_Helper
{
    public static class Constants
    {
        public const string DatabaseFilename = "ContantCleanerWithContact.db3";

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
    public class AContact
    {
        [SQLite.PrimaryKey]
        public int Id { get; set; }
        public string IDS { get; set; }
        public string? MiddleName { get; set; }
        public string? NamePrefix { get; set; }
        public string? FamilyName { get; set; }
        public string? GivenName { get; set; }
        public string? NameSuffix { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? TrueCallerName { get; set; }
        public string? Status { get; set; }
    }


    public class ContactExt
    {
        public int Id { get; set; }

        public DateTime? BirthDay { get; set; }

        public string Addresse { get; set; }

        public string DeliveryAddress { get; set; }

        public string Telephone { get; set; }

        public string Email { get; set; }

        public string Mailer { get; set; }

        public string Title { get; set; }

        public string Role { get; set; }

        public TimeZoneInfo TimeZone { get; set; }

        public Photo Logo { get; set; }

        public Photo Photo { get; set; }

        public string Note { get; set; }

        public DateTime? LastRevision { get; set; }

        public Uri Url { get; set; }

        public string UniqueIdentifier { get; set; }


        public string Organization { get; set; }

        public string OrganizationalUnit { get; set; }

        public double Longitude { get; set; }

        public double Latitude { get; set; }

        public string FormattedName { get; set; }

        public string LastName { get; set; }

        public string FirstName { get; set; }

        public string MiddleName { get; set; }

        public string Prefix { get; set; }

        public string Suffix { get; set; }

        public string NickName { get; set; }

        public string[] Categories { get; set; }

        public string SortString { get; set; }

        public string Sound { get; set; }

        public string Key { get; set; }

        public ClassificationType Classification { get; set; }

        public Uri Source { get; set; }

        public Kind Kind { get; set; }

        public DateTime? Anniversary { get; set; }

        public string Relations { get; set; }
        public string TrueCallerName {  get; set; } 
        public bool Status {  get; set; }   

    }

    public class AppContext : DbContext
    {
        public AppContext()
        {
            SQLitePCL.Batteries_V2.Init();
            Database.Migrate();
            Database.EnsureCreated();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //string dbPath = Path.Combine(FileSystem.AppDataDirectory, "medbaseapplica.db3");
            optionsBuilder.UseSqlite(Constants.DatabasePath);
        }
        public DbSet<ContactExt> Contacts { get; set; }
        public DbSet<AContact> AContacts { get; set; }

    }


    public class DBClass
    {
        SQLiteAsyncConnection Database;




        public DBClass()
        {

        }



        async Task Init()
        {
            if (Database is not null)
                return;

            Database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
            var result = await Database.CreateTableAsync<ContactModel>();
            var result2 = await Database.CreateTableAsync<AContact>();
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


        public async Task<List<AContact>> GetContactsAsync()
        {
            await Init();
            return await Database.Table<AContact>().ToListAsync();
        }


        public async Task<AContact> GetContactAsync(string phonenumber)
        {
            await Init();
            return await Database.Table<AContact>().Where(i => i.Phone == phonenumber).FirstOrDefaultAsync();
        }

        public async Task<int> SaveContactAsync(AContact item)
        {
            await Init();
            // if (item.Id != 0)
            //   return await Database.UpdateAsync(item);
            // else
            return await Database.InsertAsync(item);
        }
        public async Task<int> UpdateContactAsync(AContact item)
        {
            await Init();
            if (item.Id != 0)
                return await Database.UpdateAsync(item);
            else return -1;
            // else
            //return await Database.InsertAsync(item);
        }

        public async Task<int> DeleteContactAsync(AContact item)
        {
            await Init();
            return await Database.DeleteAsync(item);
        }
    }


    public abstract class DataModel<T>
    {
        private AppContext db;
        public List<T> Entities { get; set; }

        public T Entity { get; set; }

        //public T GetById(int id) { return db.Set<T>.Find(id); }
        //public T GetById(string id) { return db.Set<T>().Find(id); }

        public async Task<T> SaveOrUpdateAsync(T entity, bool isNew = true)
        {
            if (entity == null) return default(T);
            if (isNew) db.AddAsync(entity);
            else db.Update(entity);
            if ((await db.SaveChangesAsync()) > 0)
            {
                return entity;
            }
            else return default(T);
        }

        public async Task<bool> DeleteAsync(T entity)
        {

            db.Remove(entity);
            return (await db.SaveChangesAsync()) > 0;
        }
        public async Task<bool> DeleteAllAsync(List<T> entities)
        {
            db.RemoveRange(entities);
            return (await db.SaveChangesAsync()) > 0;
        }

    }
}
