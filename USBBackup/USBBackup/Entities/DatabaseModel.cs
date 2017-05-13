using System;
using System.ComponentModel.DataAnnotations;
using USBBackup.Core;

namespace USBBackup.Entities
{
    public abstract class DatabaseModel : NotificationObject
    {
        [Key]
        public Guid Id { get; set; }

        public void SetId()
        {
            if (Id == Guid.Empty)
                Id = Guid.NewGuid();
        }
    }
}