using FluentNHibernate.Mapping;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace USBBackup.Entities
{
    public abstract class DatabaseModel : INotifyPropertyChanged, IDataErrorInfo
    {
        public virtual Guid Id { get; set; }
        public virtual string Error
        {
            get
            {
                var errors = GetType().GetProperties().Select(x => Validate(x.Name)).Where(x => x != null).ToList();
                if (!errors.Any())
                    return null;
                return string.Join(Environment.NewLine, errors);
            }
        }

        public virtual string this[string columnName] => Validate(columnName);

        public virtual event PropertyChangedEventHandler PropertyChanged;

        public virtual void SetId()
        {
            if (Id == Guid.Empty)
                Id = Guid.NewGuid();
        }

        public virtual void OnPropertyChanged([CallerMemberName]string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        protected virtual string Validate(string columnName)
        {
            return null;
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