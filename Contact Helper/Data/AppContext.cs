using Microsoft.EntityFrameworkCore;

namespace Contact_Helper
{
    public class AppContext : DbContext
    {
        public AppContext()
        {
            SQLitePCL.Batteries_V2.Init();
            Database.EnsureCreated();
            Database.Migrate();

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //string dbPath = Path.Combine(FileSystem.AppDataDirectory, "medbaseapplica.db3");
            optionsBuilder.UseSqlite(Constants.DbString);
        }

        public DbSet<AContact> AContacts { get; set; }
        public DbSet<ContactExt> ContactExts { get; set; }
        public DbSet<AksContact> Contacts { get; set; }

    }
}
