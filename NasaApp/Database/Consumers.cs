using Android.Database;
using Android.Database.Sqlite;
using NasaApp.Models;
using System.Collections.Generic;
using System;
using Android.Content;

namespace NasaApp.Database
{
    public class Consumers : Table<Consumer>
    {
        public const string Table = "Consumer";
        public const string ID = nameof(Consumer.ID);
        public const string NetworkID = nameof(Consumer.NetworkID);
        public const string Name = nameof(Consumer.Name);
        public const string Icon = nameof(Consumer.Icon);
        public const string EnergyConsumation = nameof(Consumer.EnergyConsumation);
        public const string UsageHours = nameof(Consumer.UsageHours);
        public const string Count = nameof(Consumer.Count);
        private static readonly IDictionary<string, StorageClass> Fields = new Dictionary<string, StorageClass>
        {
            { ID, StorageClass.ID },
            { NetworkID, StorageClass.Integer },
            { Name, StorageClass.Text },
            { Icon, StorageClass.Text },
            { EnergyConsumation, StorageClass.Real },
            { UsageHours, StorageClass.Real },
            { Count, StorageClass.Integer },
        };

        public Consumers(SQLiteOpenHelper dbHelper) 
            : base(dbHelper, Table, Fields)
        {
        }

        protected override long SelectID(Consumer item) => item.ID;

        protected override Consumer Extract(ICursor cursor) => new Consumer()
        {
            ID = cursor.GetInt(cursor.GetColumnIndex(ID)),
            NetworkID = cursor.GetInt(cursor.GetColumnIndex(NetworkID)),
            Name = cursor.GetString(cursor.GetColumnIndex(Name)),
            Icon = cursor.GetString(cursor.GetColumnIndex(Icon)),
            EnergyConsumation = cursor.GetDouble(cursor.GetColumnIndex(EnergyConsumation)),
            UsageHours = cursor.GetDouble(cursor.GetColumnIndex(UsageHours)),
            Count = cursor.GetInt(cursor.GetColumnIndex(Count))
        };

        protected override ContentValues Infuse(Consumer item)
        {
            ContentValues insertValues = new ContentValues();
            insertValues.Put(NetworkID, item.NetworkID);
            insertValues.Put(Name, item.Name);
            insertValues.Put(Icon, item.Icon);
            insertValues.Put(EnergyConsumation, item.EnergyConsumation);
            insertValues.Put(UsageHours, item.UsageHours);
            insertValues.Put(Count, item.Count);
            return insertValues;
        }
    }
}