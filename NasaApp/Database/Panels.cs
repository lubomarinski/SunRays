using Android.Database;
using Android.Database.Sqlite;
using NasaApp.Models;
using System.Collections.Generic;
using System;
using Android.Content;

namespace NasaApp.Database
{
    public class Panels : Table<Panel>
    {
        internal const string Table = "Panel";
        internal const string ID = nameof(Panel.ID);
        internal const string NetworkID = nameof(Panel.NetworkID);
        internal const string ModuleType = nameof(Panel.ModuleType);
        internal const string ArrayType = nameof(Panel.ArrayType);
        internal const string TiltAngle = nameof(Panel.TiltAngle);
        internal const string SystemLosses = nameof(Panel.SystemLosses);
        internal const string PowerRating = nameof(Panel.PowerRating);
        internal const string Count = nameof(Panel.Count);
        private static readonly IDictionary<string, StorageClass> Fields = new Dictionary<string, StorageClass>
        {
            { ID, StorageClass.ID },
            { NetworkID, StorageClass.Integer },
            { ModuleType, StorageClass.Integer },
            { ArrayType, StorageClass.Integer },
            { TiltAngle, StorageClass.Real },
            { SystemLosses, StorageClass.Real },
            { PowerRating, StorageClass.Real },
            { Count, StorageClass.Integer }
        };


        public Panels(SQLiteOpenHelper dbHelper)
            : base(dbHelper, Table, Fields)
        {
        }

        protected override long SelectID(Panel item) => item.ID;

        protected override Panel Extract(ICursor cursor) => new Panel()
        {
            ID = cursor.GetInt(cursor.GetColumnIndex(ID)),
            NetworkID = cursor.GetInt(cursor.GetColumnIndex(NetworkID)),
            ModuleType = (PanelModuleType)cursor.GetInt(cursor.GetColumnIndex(ModuleType)),
            ArrayType = (PanelArrayType)cursor.GetInt(cursor.GetColumnIndex(ArrayType)),
            TiltAngle = cursor.IsNull(cursor.GetColumnIndex(TiltAngle))
                                ? null : new double?(cursor.GetDouble(cursor.GetColumnIndex(TiltAngle))),
            SystemLosses = cursor.GetDouble(cursor.GetColumnIndex(SystemLosses)),
            PowerRating = cursor.GetDouble(cursor.GetColumnIndex(PowerRating)),
            Count = cursor.GetInt(cursor.GetColumnIndex(Count))
        };

        protected override ContentValues Infuse(Panel item)
        {
            ContentValues insertValues = new ContentValues();
            insertValues.Put(NetworkID, item.NetworkID);
            insertValues.Put(ModuleType, (int)item.ModuleType);
            insertValues.Put(ArrayType, (int)item.ArrayType);
            if (item.TiltAngle.HasValue)
            {
                insertValues.Put(TiltAngle, item.TiltAngle.Value);
            }
            insertValues.Put(SystemLosses, item.SystemLosses);
            insertValues.Put(PowerRating, item.PowerRating);
            insertValues.Put(Count, item.Count);
            return insertValues;
        }
    }
}