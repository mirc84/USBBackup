﻿using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using USBBackup.Core;

namespace USBBackup.Entities
{
    public abstract class DatabaseModel : INotifyPropertyChanged, IDataErrorInfo
    {
        #region Properties

        public virtual Guid Id { get; set; }

        #endregion
        
        #region IDataError Implementation

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

        protected virtual string Validate(string columnName)
        {
            return null;
        }

        #endregion

        #region INotifyPropertyChanged Implementation

        public virtual event PropertyChangedEventHandler PropertyChanged;

        public virtual void OnPropertyChanged([CallerMemberName]string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #endregion
    }
}