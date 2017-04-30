using Android.Database;
using Android.Database.Sqlite;
using NasaApp.Models;
using System.Collections.Generic;
using Android.Content;

namespace NasaApp.Database
{
    public class SolarNetworks : Table<SolarNetwork>
    {
        internal const string Table = "SolarNetwork";
        internal const string ID = nameof(SolarNetwork.ID);
        internal const string Name = nameof(SolarNetwork.Name);
        internal const string Latitude = nameof(SolarNetwork.Latitude);
        internal const string Longtitude = nameof(SolarNetwork.Longtitude);
        private static readonly IDictionary<string, StorageClass> Fields = new Dictionary<string, StorageClass>
        {
            { ID, StorageClass.ID },
            { Name, StorageClass.Text },
            { Latitude, StorageClass.Real },
            { Longtitude, StorageClass.Real },
        };

        public SolarNetworks(SQLiteOpenHelper dbHelper)
            : base(dbHelper, Table, Fields)
        {
        }

        protected override long SelectID(SolarNetwork item) => item.ID;

        protected override SolarNetwork Extract(ICursor cursor) => new SolarNetwork()
        {
            ID = cursor.GetInt(cursor.GetColumnIndex(ID)),
            Name = cursor.GetString(cursor.GetColumnIndex(Name)),
            Latitude = cursor.GetDouble(cursor.GetColumnIndex(Latitude)),
            Longtitude = cursor.GetDouble(cursor.GetColumnIndex(Longtitude))
        };

        protected override ContentValues Infuse(SolarNetwork item)
        {
            ContentValues insertValues = new ContentValues();
            insertValues.Put(Name, item.Name);
            insertValues.Put(Longtitude, item.Longtitude);
            insertValues.Put(Latitude, item.Latitude);
            return insertValues;
        }
    }
}