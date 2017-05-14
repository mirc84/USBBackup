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
    internal class DatabaseConnection
    {
        private readonly string _dbPath;
        private readonly ISessionFactory _sessionFactory;

        public DatabaseConnection(string dbPath)
        {
            _dbPath = dbPath;
            var cfg = new Configuration()
            {
                
            };
            _sessionFactory = CreateSessionFactory();
        }

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

        public void SaveDevice(USBDevice device)
        {
            if (!device.Drives.SelectMany(x => x.Backups).Any())
            {
                DeleteIfPersited(device);
                return;
            }
            using (var session = _sessionFactory.OpenSession())
            {
                foreach (var drive in device.Drives)
                {
                    foreach (var backup in drive.Backups)
                    {
                        session.SaveOrUpdate(backup);
                    }
                    session.SaveOrUpdate(drive);
                }
                session.SaveOrUpdate(device);
                session.Flush();
            }
        }

        private void DeleteIfPersited(USBDevice device)
        {
            using (var session = _sessionFactory.OpenSession())
            {
                foreach (var drive in device.Drives)
                {
                    foreach (var backup in drive.Backups)
                    {
                        session.Delete(backup);
                    }
                    session.Delete(drive);
                }
                session.Delete(device);
            }
        }
//
//        public void Save<T>(T model)
//        where T: DatabaseModel
//        {
//
//            if (model.Id == Guid.Empty)
//                model.Id = Guid.NewGuid();
//            using (var session = _sessionFactory.OpenSession())
//            {
//                session.SaveOrUpdate(model);
//            }
//        }

//        public void Save<T>(IEnumerable<T> models)
//        {
//            using (var session = _sessionFactory.OpenSession())
//            {
//                foreach(var model in models)
//                    session.SaveOrUpdate(model);
//            }
//        }

        private ISessionFactory CreateSessionFactory()
        {
            return Fluently.Configure()
                .Database(SQLiteConfiguration.Standard.UsingFile(_dbPath))
                .Mappings(m => m.FluentMappings.AddFromAssemblyOf<DatabaseConnection>())
                //.ExposeConfiguration(BuildSchema)
                .BuildSessionFactory();
        }

        private void BuildSchema(Configuration config)
        {
            // delete the existing db on each run
            //if (File.Exists(_dbPath))
            //    File.Delete(_dbPath);

            // this NHibernate tool takes a configuration (with mapping info in)
            // and exports a database schema from it
            new SchemaExport(config).Create(false, true);
        }

        public void SaveDevices(IList<USBDevice> usbDevices)
        {
            foreach (var device in usbDevices)
            {
                SaveDevice(device);
            }
        }
    }
}
