using System;
using System.Collections.Generic;
using Android.Database;
using Android.Database.Sqlite;
using Android.Content;
using System.Linq;
using System.Threading.Tasks;

namespace NasaApp.Database
{
    public abstract class Table<T>
    {
        private static string StorageClassToSql(StorageClass storageClass)
        {
            switch (storageClass)
            {
                case StorageClass.ID: return "INTEGER PRIMARY KEY";
                case StorageClass.Integer: return "INTEGER";
                case StorageClass.Real: return "REAL";
                case StorageClass.Text: return "TEXT";
                case StorageClass.Blob: return "BLOB";
            }
            throw new InvalidOperationException();
        }

        protected string TableName { get; private set; }
        private readonly SQLiteOpenHelper dbHelper;
        private readonly IDictionary<string, StorageClass> fields;
        private readonly string createQuery;
        private readonly string dropQuery;

        protected Table(SQLiteOpenHelper dbHelper, string tableName, IDictionary<string, StorageClass> fields)
        {
            TableName = tableName;
            this.dbHelper = dbHelper;
            this.fields = fields;
            this.createQuery = GenerateCreateQuery(fields);
            this.dropQuery = GenerateDropQuery();
        }

        private string GenerateCreateQuery(IDictionary<string, StorageClass> fields)
        {
            string fieldDefs = string.Join(",", fields.Select(kvp =>
            {
                string columnName = kvp.Key;
                StorageClass storageClass = kvp.Value;
                return $"\"{columnName}\" {StorageClassToSql(storageClass)}";
            }));

            return $"CREATE TABLE {TableName} ({fieldDefs})";
        }

        private string GenerateDropQuery()
        {
            return $"DROP TABLE IF EXISTS \"{TableName}\"";
        }

        private Task<SQLiteDatabase> GetReadableDBAsync()
        {
            return Task.Run(delegate
            {
                return dbHelper.ReadableDatabase;
            });
        }

        private SQLiteDatabase WritableDB { get => dbHelper.WritableDatabase; }
        private SQLiteDatabase ReadableDB { get => dbHelper.ReadableDatabase; }

        public void Create(SQLiteDatabase db)
        {
            db.ExecSQL(createQuery);
        }

        public void Drop(SQLiteDatabase db)
        {
            db.ExecSQL(dropQuery);
        }

        public virtual Task<T> SelectByIdAsync(long id)
        {
            return GetReadableDBAsync().ContinueWith(dbTask =>
            {
                string query = $"SELECT * FROM {TableName} WHERE ID={id} LIMIT 1";
                ICursor cursor = dbTask.Result.RawQuery(query, null);
                if (cursor.Count == 0)
                {
                    throw new KeyNotFoundException();
                }
                return Select(cursor, Extract).First();
            });
        }

        public virtual Task<IList<T>> SelectAllAsync()
        {
            return GetReadableDBAsync().ContinueWith(dbTask =>
            {
                string query = $"SELECT * FROM {TableName}";
                ICursor cursor = dbTask.Result.RawQuery(query, null);
                return Select(cursor, Extract).ToList() as IList<T>;
            });
        }

        public virtual Task<long> InsertAsync(T item)
        {
            return Task.Run(delegate
            {
                return WritableDB.InsertOrThrow(TableName, null, Infuse(item));
            });
        }

        public virtual Task UpdateAsync(T item)
        {
            return Task.Run(delegate
            {
                return WritableDB.Update(TableName, Infuse(item), $"ID={SelectID(item)}", null);
            });
        }

        public virtual Task DeleteAsync(T item)
        {
            return Task.Run(delegate
            {
                SQLiteDatabase writableDB = WritableDB;
                long id = SelectID(item);
                string testQuery = $"SELECT 1 FROM {TableName} WHERE ID={id}";
                if (writableDB.RawQuery(testQuery, null).MoveToNext())
                {
                    writableDB.Delete(TableName, $"ID={id}", null);
                }
                else
                {
                    throw new KeyNotFoundException("The item to be deleted was not found in the database");
                }
            });
        }

        protected abstract long SelectID(T item);
        protected abstract T Extract(ICursor cursor);
        protected abstract ContentValues Infuse(T item);

        private static IEnumerable<T> Select(ICursor cursor, Func<ICursor, T> selector)
        {
            while (cursor.MoveToNext())
            {
                yield return selector(cursor);
            }
        }
    }
}