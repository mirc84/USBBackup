using FluentNHibernate.Mapping;
using System;

namespace USBBackup.Entities
{
    public abstract class DatabaseModel
    {
        public virtual Guid Id { get; set; }

        public virtual void SetId()
        {
            if (Id == Guid.Empty)
                Id = Guid.NewGuid();
        }
    }

    public abstract class DatabaseModelMap<T> : ClassMap<T>
        where T: DatabaseModel
    {
        public DatabaseModelMap()
        {
            Id(x => x.Id).Unique();
        }
    }
}