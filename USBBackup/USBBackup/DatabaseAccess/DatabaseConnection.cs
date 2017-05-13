using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Cfg;
using System.IO;

namespace USBBackup.DatabaseAccess
{
    internal class DatabaseConnection
    {
        public DatabaseConnection(string dbPath)
        {
            var cfg = new Configuration();

        }

        public DatabaseContext DatabaseContext { get; }

        public void Save()
        {
        }

        private static ISessionFactory CreateSessionFactory()
        {
            return Fluently.Configure()
                .Database(SQLiteConfiguration.Standard.UsingFile("firstProject.db"))
                .Mappings(m => m.FluentMappings.AddFromAssemblyOf<DatabaseConnection>())
                .ExposeConfiguration(BuildSchema)
                .BuildSessionFactory();
        }

        private static void BuildSchema(Configuration config)
        {
            // delete the existing db on each run
            if (File.Exists("firstProject.db"))
                File.Delete("firstProject.db");

            // this NHibernate tool takes a configuration (with mapping info in)
            // and exports a database schema from it
            new SchemaExport(config)
              .Create(false, true);
        }
    }
}
