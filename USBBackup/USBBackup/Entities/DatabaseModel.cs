using FluentNHibernate.Mapping;
using System;
using USBBackup.Core;

namespace USBBackup.Entities
{
    public abstract class DatabaseModel : NotificationObject
    {
        public Guid Id { get; set; }

        public void SetId()
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