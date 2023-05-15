using FleuristeVirtuel_API.Types;
using FleuristeVirtuel_API.Utils;
using MySql.Data.MySqlClient;
using System.Data;
using System.Reflection;

namespace FleuristeVirtuel_API
{
    public class DbConnection
    {
        private const string DB_SERVER = "localhost";
        private const uint DB_PORT = 3306;
        private const string DB_NAME = "fleurs";

        public MySqlConnection? Connection { get; private set; }

        public DbConnection(MySqlConnection connection)
        {
            connection.Open();
            Connection = connection;
        }

        public DbConnection() { }

        ~DbConnection()
        {
            DisposeCurrentConnection();
        }

        public void DisposeCurrentConnection()
        {
            Connection?.Dispose();
        }

        public void Open(string server, uint port, string name, string uid, string password)
        {
            DisposeCurrentConnection();

            Connection = new($"SERVER={server};PORT={port};DATABASE={name};UID={uid};PASSWORD={password}");
            Connection.Open();
        }

        public void Open(string uid, string password)
        {
            Open(DB_SERVER, DB_PORT, DB_NAME, uid, password);
        }

        public MySqlCommand PrepareCommand(string commandText, params DbParam[] parameters)
        {
            if (Connection == null || Connection.State != ConnectionState.Open)
                throw new Exception("Database connection not opened!");

            MySqlCommand command = new(commandText, Connection);
            foreach (var p in parameters)
            {
                if (p.Type == null) command.Parameters.AddWithValue(p.Name, p.Value);
                else
                {
                    MySqlParameter mysqlP = new(p.Name, (MySqlDbType)p.Type);
                    mysqlP.Value = p.Value;
                    command.Parameters.Add(mysqlP);
                }
            }

            return command;
        }

        // ALL THE FOLLOWING METHODS ARE DEFINED IN 2 WAYS, NORMAL (SYNC) AND ASYNC

        public int ExecuteNonQuery(string commandText, params DbParam[] parameters)
        {
            return PrepareCommand(commandText, parameters).ExecuteNonQuery();
        }

        /*public Task<int> ExecuteNonQueryAsync(string commandText, params DbParameter[] parameters)
        {
            return PrepareCommand(commandText, parameters).ExecuteNonQueryAsync();
        }*/

        public MySqlDataReader ExecuteReader(string commandText, params DbParam[] parameters)
        {
            return PrepareCommand(commandText, parameters).ExecuteReader();
        }

        /*public Task<MySqlDataReader> ExecuteReaderAsync(string commandText, params DbParameter[] parameters)
        {
            return PrepareCommand(commandText, parameters).ExecuteReaderAsync().ContinueWith(task => (MySqlDataReader)task.Result);
        }*/

        public T? SelectSingleCell<T>(string commandText, object? columnIndex = null, params DbParam[] parameters)
        {
            object nonNullableColumn = columnIndex ?? 0;

            using MySqlDataReader reader = ExecuteReader(commandText, parameters);
            if (reader.HasRows)
            {
                reader.Read();
                Type columnIndexType = nonNullableColumn.GetType();
                object? readObject = null;
                if (columnIndexType == typeof(int)) readObject = reader[(int)nonNullableColumn];
                else if (columnIndexType == typeof(string)) readObject = reader[(string)nonNullableColumn];

                if(readObject is DBNull) readObject = null;
                return (T?)CodeExtensions.ChangeNullableType(readObject, typeof(T));
            }

            return default;
        }

        public List<object[]> SelectMultipleRows(string commandText, params DbParam[] parameters)
        {
            using MySqlDataReader reader = ExecuteReader(commandText, parameters);
            List<object[]> result = new();

            if (reader.HasRows)
            {

                while (reader.Read())
                {
                    object[] temp = new object[reader.FieldCount];
                    reader.GetValues(temp);
                    result.Add(temp);
                }
            }

            return result;
        }

        public T? SelectSingleRecord<T>(string commandText, params DbParam[] parameters) where T : DbRecord, new()
        {
            using MySqlDataReader reader = ExecuteReader(commandText, parameters);
            if (reader.HasRows)
            {
                reader.Read();
                return DbRecord.CreateFromReader<T>(reader);
            }

            return null;
        }

        public List<T> SelectMultipleRecords<T>(string commandText, params DbParam[] parameters) where T : DbRecord, new()
        {
            using MySqlDataReader reader = ExecuteReader(commandText, parameters);
            List<T> result = new();

            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    result.Add(DbRecord.CreateFromReader<T>(reader));
                }
            }

            return result;
        }

        /*public async Task<T?> SelectSingleCellAsync<T>(string commandText, object? columnIndex = null, params DbParameter[] parameters)
        {
            object nonNullableColumn = columnIndex ?? 0;

            MySqlDataReader reader = await ExecuteReaderAsync(commandText, parameters);
            if (reader.HasRows)
            {
                reader.Read();
                Type columnIndexType = nonNullableColumn.GetType();
                if (columnIndexType == typeof(int)) return (T)reader[(int)nonNullableColumn];
                else if (columnIndexType == typeof(string)) return (T)reader[(string)nonNullableColumn];
            }

            return default;
        }*/

        // END OF 2-WAYS METHODS
    }

    public struct DbParam
    {
        public string Name { get; private set; }
        public MySqlDbType? Type { get; private set; }
        public object Value { get; private set; }

        public DbParam(string name, MySqlDbType type, object value)
        {
            Name = name;
            Type = type;
            Value = value;
        }

        public DbParam(string name, object value)
        {
            Name = name;
            Type = null;
            Value = value;
        }
    }
}