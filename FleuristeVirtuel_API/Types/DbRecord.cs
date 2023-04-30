using FleuristeVirtuel_API.Utils;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Common;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FleuristeVirtuel_API.Types
{
    /// <summary>
    /// This represents a table in the database.
    /// </summary>
    /// <typeparam name="T">is the type itself.</typeparam>
    public abstract class DbRecord
    {
        private bool isUnique = false;
        private bool isRemote = false;

        private readonly static Dictionary<Type, WeakList<DbRecord>> existingInstances = new();

        private static void EnsureInstancesTypeExists(Type type)
        {
            if (!existingInstances.ContainsKey(type))
                existingInstances.Add(type, new());
        }

        private void ThrowIfInvalidTableName(string tableName, string action = "")
        {
            if (action.Length == 0)
                action = "for";
            else action = "to " + action;

            if (!NameSecurity.CheckMySqlName(tableName))
                throw new FormatException($"Invalid table {tableName} {action} type {GetType().Name}!");
        }

        public void InsertInto(string tableName, DbConnection connection, bool insertPrimaryKey = false)
        {
            ThrowIfInvalidTableName(tableName, "insert");

            if (isRemote)
                return;

            Dictionary<string, object> parts = new();
            PropertyInfo? lastPrimaryKeyInfo = null;

            Type currentType = GetType();
            foreach (var pi in currentType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                bool isPrimaryKey = false;
                string? columnName = null;
                foreach (var attr in pi.CustomAttributes)
                {
                    if (attr.AttributeType == typeof(DbColumnAttribute))
                    {
                        columnName = (attr.ConstructorArguments[0].Value as string ?? "").Trim();
                        if (columnName.Length == 0) columnName = pi.Name;
                    }
                    else if (attr.AttributeType == typeof(PrimaryKeyAttribute))
                        isPrimaryKey = true;
                }

                if (columnName != null)
                {
                    object? value = pi.GetValue(this);
                    if ((insertPrimaryKey || !isPrimaryKey) && value != null)
                    {
                        parts.Add(columnName, value);
                    }

                    if (isPrimaryKey)
                        lastPrimaryKeyInfo = pi;
                }
            }

            string commandText = "INSERT INTO " + tableName + " (" + string.Join(", ", parts.Keys) + ") VALUES ("
                + string.Join(", ", parts.Keys.Select(x => "@val_" + x)) + ");";

            MySqlCommand command = new(commandText, connection.Connection);
            foreach (KeyValuePair<string, object> kv in parts)
                command.Parameters.AddWithValue("@val_" + kv.Key, kv.Value);

            int affected = command.ExecuteNonQuery();
            if (affected != 1)
                throw new Exception($"Error while inserting into {tableName}, no row was affected!");

            if (!insertPrimaryKey && lastPrimaryKeyInfo != null)
            {
                object? newPrimaryKey = Convert.ChangeType(command.LastInsertedId, lastPrimaryKeyInfo.PropertyType);
                lastPrimaryKeyInfo.SetValue(this, newPrimaryKey);
            }

            if(isUnique == false)
            {
                EnsureInstancesTypeExists(currentType);
                existingInstances[currentType].Add(this);
                isUnique = true;
            }

            isRemote = true;
        }

        public void Update(string tableName, DbConnection connection)
        {
            ThrowIfInvalidTableName(tableName, "update");

            if (!isRemote)
                throw new InvalidDataException("This instance doesn't not represent a record in the database! Was it fetched using the DbConnector or INSERTed?");

            Dictionary<string, object?> newValues = new();
            Dictionary<string, object?> primaryValues = new();

            Type currentType = GetType();
            foreach (var pi in currentType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                bool isPrimaryKey = false;
                string? columnName = null;
                foreach (var attr in pi.CustomAttributes)
                {
                    if (attr.AttributeType == typeof(DbColumnAttribute))
                    {
                        columnName = (attr.ConstructorArguments[0].Value as string ?? "").Trim();
                        if (columnName.Length == 0) columnName = pi.Name;
                    }
                    else if (attr.AttributeType == typeof(PrimaryKeyAttribute))
                        isPrimaryKey = true;
                }

                if (columnName != null)
                {
                    object? value = pi.GetValue(this);
                    if (isPrimaryKey)
                        primaryValues.Add(columnName, value);
                    else newValues.Add(columnName, value);
                }
            }

            string commandText = "UPDATE " + tableName + " SET " + string.Join(", ", newValues.Keys.Select(x => x + " = @val_" + x))
                + " WHERE " + string.Join(" AND ", primaryValues.Keys.Select(x => x + " = @primary_" + x)) + ";";

            MySqlCommand command = new(commandText, connection.Connection);
            foreach (KeyValuePair<string, object?> kv in newValues)
                command.Parameters.AddWithValue("@val_" + kv.Key, kv.Value);
            foreach (KeyValuePair<string, object?> kv in primaryValues)
                command.Parameters.AddWithValue("@primary_" + kv.Key, kv.Value);

            int affected = command.ExecuteNonQuery();
            if (affected != 1)
                throw new Exception($"Error while updating {tableName}, no row was affected!");
        }

        public void DeleteFrom(string tableName, DbConnection connection)
        {
            ThrowIfInvalidTableName(tableName, "delete");

            if (!isRemote)
                throw new InvalidDataException("This instance doesn't not represent a record in the database! Was it fetched using the DbConnector or INSERTed?");

            Dictionary<string, object?> primaryValues = new();

            Type currentType = GetType();
            foreach (var pi in currentType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                bool isPrimaryKey = false;
                string? columnName = null;
                foreach (var attr in pi.CustomAttributes)
                {
                    if (attr.AttributeType == typeof(DbColumnAttribute))
                    {
                        columnName = (attr.ConstructorArguments[0].Value as string ?? "").Trim();
                        if (columnName.Length == 0) columnName = pi.Name;
                    }
                    else if (attr.AttributeType == typeof(PrimaryKeyAttribute))
                        isPrimaryKey = true;
                }

                if (columnName != null)
                {
                    object? value = pi.GetValue(this);
                    if (isPrimaryKey)
                        primaryValues.Add(columnName, value);
                }
            }

            string commandText = "DELETE FROM " + tableName + " WHERE " + string.Join(" AND ", primaryValues.Keys.Select(x => x + " = @primary_" + x)) + ";";

            MySqlCommand command = new(commandText, connection.Connection);
            foreach (KeyValuePair<string, object?> kv in primaryValues)
                command.Parameters.AddWithValue("@primary_" + kv.Key, kv.Value);

            int affected = command.ExecuteNonQuery();
            if (affected != 1)
                throw new Exception($"Error while updating {tableName}, no row was affected!");

            existingInstances[currentType].Remove(this);
        }

        public void FetchForeignReferences(DbConnection connection)
        {
            Type currentType = GetType();
            foreach (var pi in currentType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                foreach (var attr in pi.CustomAttributes)
                {
                    if (attr.AttributeType == typeof(ForeignReferenceAttribute))
                    {
                        var arguments = attr.ConstructorArguments.ToArray();
                        string referencedTable = (arguments[0].Value as string ?? "").Trim();
                        string foreignKey = (arguments[1].Value as string ?? "").Trim();
                        string localKeyPropertyName = (arguments[2].Value as string ?? "").Trim();

                        if (referencedTable.Length == 0 || foreignKey.Length == 0 || localKeyPropertyName.Length == 0)
                            continue;

                        if (!NameSecurity.CheckMySqlName(referencedTable) || !NameSecurity.CheckMySqlName(foreignKey))
                            throw new AccessViolationException("Referenced table or foreign key name is invalid!");

                        object? localValue = currentType.GetProperty(localKeyPropertyName)?.GetValue(this);
                        if (localValue == null) break;

                        Type foreignType = pi.PropertyType;
                        MethodInfo? foreignSelectMethod = connection.GetType().GetMethod("SelectSingleRecord")?.MakeGenericMethod(foreignType);

                        if (foreignSelectMethod == null)
                            throw new Exception("Cannot use this type as foreign key");

                        object? foreignValue = foreignSelectMethod.Invoke(connection, new object[] {
                            "SELECT * FROM " + referencedTable + " WHERE " + referencedTable + "." + foreignKey + " = @localValue",
                             new DbParam[] { new("@localValue", localValue) }
                        });

                        pi.SetValue(this, foreignValue);
                    }
                }
            }
        }

        public static T CreateEmptyOrGetInstance<T>(params object[] primaryKeys) where T : DbRecord, new()
        {
            Type t = typeof(T);
            EnsureInstancesTypeExists(t);

            foreach (object obj in existingInstances[t])
            {
                T? record = (T)obj;
                if (Enumerable.SequenceEqual(record.GetPrimaryKeyValues<T>(), primaryKeys)) return record;
            }

            T? newRecord = new();
            if (newRecord == null)
                throw new Exception();

            newRecord.SetPrimaryKeyValues<T>(primaryKeys);
            newRecord.isUnique = true;

            existingInstances[t].Add(newRecord);


            return newRecord;
        }

        private static uint CountPrimaryKeys<T>() where T : DbRecord, new()
        {
            uint count = 0;

            foreach (var mi in typeof(T).GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                foreach (var attr in mi.CustomAttributes)
                {
                    if (attr.AttributeType == typeof(PrimaryKeyAttribute))
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        public static T CreateFromReader<T>(MySqlDataReader reader) where T : DbRecord, new()
        {
            Type childType = typeof(T);
            object[] primaryKeyValues = new object[CountPrimaryKeys<T>()];
            Dictionary<string, PropertyInfo> allColumnsMembers = new();

            // T? child = (T?)Activator.CreateInstance(childType);
            //if (child == null) throw new ArgumentNullException("Cannot create instance of type " + childType.Name + " because it doesn't have a default constructor!");

            foreach (var pi in childType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                object? value = null;
                int pKeyPart = -1;
                foreach (var attr in pi.CustomAttributes)
                {
                    if (attr.AttributeType == typeof(DbColumnAttribute))
                    {
                        var arguments = attr.ConstructorArguments.ToArray();
                        string dbColumnName = (arguments[0].Value as string ?? "").Trim();
                        if (dbColumnName.Length == 0) dbColumnName = pi.Name;

                        allColumnsMembers.Add(dbColumnName, pi);
                        value = reader[dbColumnName];
                    }
                    else if (attr.AttributeType == typeof(PrimaryKeyAttribute))
                    {
                        pKeyPart = (int)(attr.ConstructorArguments[0].Value ?? 0);
                    }
                }

                if (value != null && pKeyPart >= 0)
                    primaryKeyValues[pKeyPart] = value;
            }

            T child = CreateEmptyOrGetInstance<T>(primaryKeyValues);
            child.isRemote = true;
            foreach (KeyValuePair<string, PropertyInfo> pair in allColumnsMembers)
            {
                pair.Value.SetValue(child, Convert.ChangeType(reader[pair.Key], pair.Value.PropertyType));
            }

            return child;
        }

        public object[] GetPrimaryKeyValues<T>() where T : DbRecord, new()
        {
            object[] values = new object[CountPrimaryKeys<T>()];
            int found = 0;
            foreach (var pi in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                foreach (var attr in pi.CustomAttributes)
                {
                    if (attr.AttributeType == typeof(PrimaryKeyAttribute))
                    {
                        object? value = pi.GetValue(this);
                        int keyPart = (int)(attr.ConstructorArguments[0].Value ?? 0);

                        if (value != null)
                        {
                            values[keyPart] = value;
                            found++;
                        }
                    }
                }
            }

            if (found == values.Length) return values;
            else throw new Exception("Cannot find primary key member for type " + typeof(T).Name);
        }

        public void SetPrimaryKeyValues<T>(params object[] values) where T : DbRecord, new()
        {
            int set = 0;
            foreach (var pi in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                foreach (var attr in pi.CustomAttributes)
                {
                    if (attr.AttributeType == typeof(PrimaryKeyAttribute))
                    {
                        int keyPart = (int)(attr.ConstructorArguments[0].Value ?? 0);
                        pi.SetValue(this, values[keyPart]);
                        set++;
                    }
                }
            }

            if (set != values.Length)
                throw new FieldAccessException("Cannot access primary key because no PrimaryKeyAttribute was set and this method was not overriden!");
        }

        // DOES NOT WORK!!!!
        //public static bool operator ==(DbRecord? a, DbRecord? b)
        //{
        //    if (ReferenceEquals(a, b)) return true;
        //    if (ReferenceEquals(a, null) || ReferenceEquals(b, null)) return false;

        //    Type typeA = a.GetType();
        //    Type typeB = b.GetType();

        //    if (typeA != typeB) return false;

        //    MethodInfo? mi = typeA.GetMethod("GetPrimaryKeyValues")?.MakeGenericMethod(typeA);
        //    if (mi == null) return false;

        //    return mi.Invoke(a, null) == mi.Invoke(b, null);
        //}

        //public static bool operator !=(DbRecord? a, DbRecord? b)
        //{
        //    return !(a == b);
        //}

        //public override int GetHashCode()
        //{
        //    Type type = GetType();
        //    MethodInfo? mi = type.GetMethod("GetPrimaryKeyValues")?.MakeGenericMethod(type);

        //    return ((object[]?)mi?.Invoke(this, null))?.GetHashCode() ?? base.GetHashCode();
        //}

        //public override bool Equals(object? other)
        //{
        //    return other is DbRecord rec && this == rec;
        //}
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class DbColumnAttribute : Attribute
    {
        private readonly string columnName;

        public DbColumnAttribute(string columnName = "")
        {
            this.columnName = columnName;
        }

        public string ColumnName
        {
            get => columnName;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class ForeignReferenceAttribute : Attribute
    {
        private readonly string referencedTable;
        private readonly string foreignKey;
        private readonly string localKeyPropertyName;

        public ForeignReferenceAttribute(string referencedTable, string foreignKey, string localKeyPropertyName)
        {
            this.referencedTable = referencedTable;
            this.foreignKey = foreignKey;
            this.localKeyPropertyName = localKeyPropertyName;
        }

        public string ReferencedTable
        {
            get => referencedTable;
        }

        public string ForeignKey
        {
            get => foreignKey;
        }

        public string LocalKeyPropertyName
        {
            get => localKeyPropertyName;
        }
    }


    [AttributeUsage(AttributeTargets.Property)]
    public class PrimaryKeyAttribute : Attribute
    {
        private readonly int keyPart;

        public PrimaryKeyAttribute(int keyPart = 0)
        {
            this.keyPart = keyPart;
        }

        public int KeyPart
        {
            get => keyPart;
        }
    }
}
