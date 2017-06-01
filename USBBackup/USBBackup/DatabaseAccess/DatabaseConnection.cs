using System.Collections.Generic;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using System.IO;
using NHibernate.Linq;
using System.Linq;
using USBBackup.Entities;

namespace USBBackup.DatabaseAccess
{
    public class DatabaseConnection
    {
        #region Fields

        private readonly string _dbPath;
        private readonly ISessionFactory _sessionFactory;

        #endregion

        #region Constructor

        public DatabaseConnection(string dbPath)
        {
            _dbPath = dbPath;
            _sessionFactory = CreateSessionFactory();
        }

        #endregion

        #region Public Methods

        public IList<T> GetAll<T>()
        {
            using (var session = _sessionFactory.OpenSession())
            {
                var list = session.Query<T>().ToList();

                foreach (var item in list)
                {
                }

                return list;
            }
        }

        public void SaveDevices(IList<Drive> usbDevices)
        {
            foreach (var device in usbDevices)
            {
                Save(device);
            }
        }

        public void Save(Drive drive)
        {
            if (!drive.Backups.Any())
            {
                DeleteIfPersited(drive);
                return;
            }
            foreach (var backup in drive.Backups)
            {
                backup.IsInverse = (backup.SourcePath.StartsWith(drive.DriveLetter));
            }
            using (var session = _sessionFactory.OpenSession())
            {
                foreach (var backup in drive.Backups)
                {
                    session.SaveOrUpdate(backup);
                }
                session.SaveOrUpdate(drive);

                session.Flush();
            }
            foreach (var backup in drive.Backups)
            {
                backup.SetDataSaved();
            }
        }

        #endregion

        #region Non Public Methods

        private void DeleteIfPersited(Drive drive)
        {
            using (var session = _sessionFactory.OpenSession())
            {
                foreach (var backup in drive.Backups)
                {
                    session.Delete(backup);
                }
                session.Delete(drive);
                session.Flush();
            }
        }

        private ISessionFactory CreateSessionFactory()
        {
            var config = Fluently.Configure()
                .Database(SQLiteConfiguration.Standard.UsingFile(_dbPath))
                .Mappings(m => m.FluentMappings.AddFromAssemblyOf<DatabaseConnection>());

            if (!File.Exists(_dbPath))
                config = config.ExposeConfiguration(BuildSchema);
            else
                config = config.ExposeConfiguration(UpdateSchema);

            return config.BuildSessionFactory();
        }

        private void UpdateSchema(Configuration config)
        {
            var schemaUpdate = new SchemaUpdate(config);
            schemaUpdate.Execute(false, true);
        }

        private void BuildSchema(Configuration config)
        {
            var schemaExport = new SchemaExport(config);
            schemaExport.Create(false, true);
        } 

        #endregion
    }
}
