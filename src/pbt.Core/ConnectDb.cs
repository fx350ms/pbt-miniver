using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace pbt.Core
{
    public static class ConnectDb
    {
        private static string _connectionString;

        public static void Initialize(string connectionString)
        {
            _connectionString = connectionString;
        }

        private static SqlConnection OpenConnection()
        {
            var connection = new SqlConnection(_connectionString);
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
            return connection;
        }

        public static void CloseConnection(SqlConnection connection)
        {
            if (connection != null && connection.State == ConnectionState.Open)
            {
                connection.Close();
            }
        }

        public static DataTable ExecuteQuery(string query, CommandType commandType = CommandType.StoredProcedure, SqlParameter[] parameters = null)
        {
            using (var connection = OpenConnection())
            using (var command = new SqlCommand(query, connection))
            {
                command.CommandType = commandType;
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                using (var adapter = new SqlDataAdapter(command))
                {
                    var dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    return dataTable;
                }
            }
        }

        public static T GetItem<T>(string query, CommandType commandType = CommandType.StoredProcedure, SqlParameter[] parameters = null) where T : class, new()
        {
            using (var connection = OpenConnection())
            using (var command = new SqlCommand(query, connection))
            {
                command.CommandType = commandType;
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var item = new T();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            var property = typeof(T).GetProperty(reader.GetName(i));
                            if (property != null && !reader.IsDBNull(i))
                            {
                                property.SetValue(item, reader.GetValue(i));
                            }
                        }
                        return item;
                    }
                }
            }
            return null;
        }

        public static List<T> GetList<T>(string query, CommandType commandType = CommandType.StoredProcedure, SqlParameter[] parameters = null) where T : class, new()
        {
            var list = new List<T>();
            using (var connection = OpenConnection())
            using (var command = new SqlCommand(query, connection))
            {
                command.CommandType = commandType;
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var item = new T();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            var property = typeof(T).GetProperty(reader.GetName(i));
                            if (property != null && !reader.IsDBNull(i))
                            {
                                property.SetValue(item, reader.GetValue(i));
                            }
                        }
                        list.Add(item);
                    }
                }
            }
            return list;
        }

        public static int ExecuteNonQuery(string query, CommandType commandType = CommandType.StoredProcedure, SqlParameter[] parameters = null)
        {
            using (var connection = OpenConnection())
            using (var command = new SqlCommand(query, connection))
            {
                command.CommandType = commandType;
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }
                return command.ExecuteNonQuery();
            }
        }

        public static object ExecuteScalar(string query, CommandType commandType = CommandType.StoredProcedure, SqlParameter[] parameters = null)
        {
            using (var connection = OpenConnection())
            using (var command = new SqlCommand(query, connection))
            {
                command.CommandType = commandType;
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }
                return command.ExecuteScalar();
            }
        }

        private static async Task<SqlConnection> OpenConnectionAsync()
        {
            var connection = new SqlConnection(_connectionString);
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();
            }
            return connection;
        }

        public static async Task<DataTable> ExecuteQueryAsync(string query, CommandType commandType = CommandType.StoredProcedure, SqlParameter[] parameters = null)
        {
            using (var connection = await OpenConnectionAsync())
            using (var command = new SqlCommand(query, connection))
            {
                command.CommandType = commandType;
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                using (var adapter = new SqlDataAdapter(command))
                {
                    var dataTable = new DataTable();
                    await Task.Run(() => adapter.Fill(dataTable)); // Fill không hỗ trợ async, nên dùng Task.Run
                    return dataTable;
                }
            }
        }


        public static async Task<T> GetItemAsync<T>(
    string query,
    CommandType commandType = CommandType.StoredProcedure,
    SqlParameter[] parameters = null
) where T : class, new()
        {
            using var connection = await OpenConnectionAsync();
            using var command = new SqlCommand(query, connection) { CommandType = commandType };
            if (parameters != null)
                command.Parameters.AddRange(parameters);

            using var reader = await command.ExecuteReaderAsync();
            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                 .ToDictionary(p => p.Name.ToLower(), p => p);

            if (await reader.ReadAsync())
            {
                var item = new T();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var colName = reader.GetName(i).ToLower();
                    if (props.TryGetValue(colName, out var prop) && !reader.IsDBNull(i))
                    {
                        var value = reader.GetValue(i);
                        var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                        prop.SetValue(item, Convert.ChangeType(value, targetType));
                    }
                }
                return item;
            }
            return null;
        }

        public static async Task<List<T>> GetListAsync<T>(
            string query,
            CommandType commandType = CommandType.StoredProcedure,
            SqlParameter[] parameters = null
        ) where T : class, new()
        {
            using var connection = await OpenConnectionAsync();
            using var command = new SqlCommand(query, connection) { CommandType = commandType };
            if (parameters != null)
                command.Parameters.AddRange(parameters);

            using var reader = await command.ExecuteReaderAsync();
            var list = new List<T>();
            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                 .ToDictionary(p => p.Name.ToLower(), p => p);

            while (await reader.ReadAsync())
            {
                var item = new T();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var colName = reader.GetName(i).ToLower();
                    if (props.TryGetValue(colName, out var prop) && !reader.IsDBNull(i))
                    {
                        var value = reader.GetValue(i);
                        var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                        prop.SetValue(item, Convert.ChangeType(value, targetType));
                    }
                }
                list.Add(item);
            }
            return list;
        }


        //public static async Task<List<T>> GetListAsync<T>(string query, CommandType commandType = CommandType.StoredProcedure, SqlParameter[] parameters = null) where T : class, new()
        //{
        //    var list = new List<T>();
        //    using (var connection = await OpenConnectionAsync())
        //    using (var command = new SqlCommand(query, connection))
        //    {
        //        command.CommandType = commandType;
        //        if (parameters != null)
        //        {
        //            command.Parameters.AddRange(parameters);
        //        }

        //        using (var reader = await command.ExecuteReaderAsync())
        //        {
        //            while (await reader.ReadAsync())
        //            {
        //                var item = new T();
        //                for (int i = 0; i < reader.FieldCount; i++)
        //                {
        //                    var property = typeof(T).GetProperty(reader.GetName(i));
        //                    if (property != null && !reader.IsDBNull(i))
        //                    {
        //                        property.SetValue(item, reader.GetValue(i));
        //                    }
        //                }
        //                list.Add(item);
        //            }
        //        }
        //    }
        //    return list;
        //}

        public static async Task<int> ExecuteNonQueryAsync(string query, CommandType commandType = CommandType.StoredProcedure, SqlParameter[] parameters = null)
        {
            using (var connection = await OpenConnectionAsync())
            using (var command = new SqlCommand(query, connection))
            {
                command.CommandType = commandType;
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }
                return await command.ExecuteNonQueryAsync();
            }
        }

        public static async Task<object> ExecuteScalarAsync(string query, CommandType commandType = CommandType.StoredProcedure, SqlParameter[] parameters = null)
        {
            using (var connection = await OpenConnectionAsync())
            using (var command = new SqlCommand(query, connection))
            {
                command.CommandType = commandType;
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }
                return await command.ExecuteScalarAsync();
            }
        }
    }
}