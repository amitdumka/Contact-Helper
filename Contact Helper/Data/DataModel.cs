namespace Contact_Helper
{
    public abstract class DataModel<T>
    {
        private AppContext db;

        public async Task<bool> DeleteAllAsync(List<T> entities)
        {
            db.RemoveRange(entities);
            return (await db.SaveChangesAsync()) > 0;
        }

        public async Task<bool> DeleteAsync(T entity)
        {

            db.Remove(entity);
            return (await db.SaveChangesAsync()) > 0;
        }

        //public T GetById(int id) { return db.Set<T>.Find(id); }
        //public T GetById(string id) { return db.Set<T>().Find(id); }

        public async Task<T> SaveOrUpdateAsync(T entity, bool isNew = true)
        {
            if (entity == null) return default(T);
            if (isNew) _ = db.AddAsync(entity);
            else db.Update(entity);
            if ((await db.SaveChangesAsync()) > 0)
            {
                return entity;
            }
            else return default;
        }

        public List<T> Entities { get; set; }

        public T Entity { get; set; }

    }
}
