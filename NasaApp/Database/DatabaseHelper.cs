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

        public override async void OnCreate(SQLiteDatabase db)
        {
            await CreateDatabaseAsync(db);
        }

        public override async void OnUpgrade(SQLiteDatabase db, int oldVersion, int newVersion)
        {
            await DropDatabaseAsync(db);
            await CreateDatabaseAsync(db);
        }

        private async Task CreateDatabaseAsync(SQLiteDatabase db)
        {
            await SolarNetworks.CreateAsync();
            await Panels.CreateAsync();
            await Consumers.CreateAsync();
        }

        private async Task DropDatabaseAsync(SQLiteDatabase db)
        {
            await SolarNetworks.DropAsync();
            await Panels.DropAsync();
            await Consumers.DropAsync();
        }

        void IDisposable.Dispose()
        {
            Close();
        }
    }
}