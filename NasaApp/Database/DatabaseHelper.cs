using System;
using Android.Database.Sqlite;
using Android.Content;
using System.Threading.Tasks;

namespace NasaApp.Database
{
    public class DatabaseHelper : SQLiteOpenHelper, IDisposable
    {
        private const string Filename = "datastore.db";
        private const int Version = 5;

        public SolarNetworks SolarNetworks { get; private set; }
        public Panels Panels { get; private set; }
        public Consumers Consumers { get; private set; }

        public DatabaseHelper(Context context)
            : base(context, Filename, null, Version)
        {
            SolarNetworks = new SolarNetworks(this);
            Panels = new Panels(this);
            Consumers = new Consumers(this);
        }

        public override void OnCreate(SQLiteDatabase db)
        {
            CreateDatabase(db);
        }

        public override void OnUpgrade(SQLiteDatabase db, int oldVersion, int newVersion)
        {
            DropDatabase(db);
            CreateDatabase(db);
        }

        private void CreateDatabase(SQLiteDatabase db)
        {
            SolarNetworks.Create(db);
            Panels.Create(db);
            Consumers.Create(db);
        }

        private void DropDatabase(SQLiteDatabase db)
        {
            SolarNetworks.Drop(db);
            Panels.Drop(db);
            Consumers.Drop(db);
        }

        void IDisposable.Dispose()
        {
            Close();
        }
    }
}