using FluentNHibernate.Mapping;

namespace USBBackup.Entities
{
    public abstract class DatabaseModelMap<T> : ClassMap<T>
        where T: DatabaseModel
    {
        public DatabaseModelMap()
        {
            Id(x => x.Id).Unique();
        }
    }
}