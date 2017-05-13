﻿namespace USBBackup.DatabaseAccess
{
    public class DatabaseConnection
    {
        public DatabaseConnection()
        {
            Database = new Database();
        }

        public Database Database { get; }

        public void Save()
        {
            Database.SaveChanges();
        }
    }
}