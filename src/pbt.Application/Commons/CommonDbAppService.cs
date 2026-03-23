using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Entities;
using Abp.Domain.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using pbt.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace pbt.Application.Common
{
    /// <summary>
    /// Lớp nền mở rộng từ AsyncCrudAppService cho phép thực thi ADO.NET query trực tiếp
    /// </summary>
    public abstract class CommonDbAppService<TEntity, TEntityDto, TPrimaryKey, TGetAllInput, TCreateInput, TUpdateInput>
        : AsyncCrudAppService<TEntity, TEntityDto, TPrimaryKey, TGetAllInput, TCreateInput, TUpdateInput>,
          ICommonDbAppService<TEntityDto, TPrimaryKey, TGetAllInput, TCreateInput, TUpdateInput>
        where TEntity : class, IEntity<TPrimaryKey>
        where TEntityDto : IEntityDto<TPrimaryKey>
        where TUpdateInput : IEntityDto<TPrimaryKey>
    {

        protected CommonDbAppService(IRepository<TEntity, TPrimaryKey> repository)
            : base(repository)
        {
        }

        /// <summary>
        /// Thực thi câu lệnh SQL và trả về DataTable
        /// </summary>
        public virtual async Task<DataTable> ExecuteQueryAsync(string query, CommandType commandType = CommandType.StoredProcedure, SqlParameter[] parameters = null)
        {
            return await Task.Run(() => ConnectDb.ExecuteQuery(query, commandType, parameters));
        }

        /// <summary>
        /// Thực thi câu lệnh SQL không trả về kết quả (INSERT/UPDATE/DELETE)
        /// </summary>
        public virtual async Task<int> ExecuteNonQueryAsync(string query, CommandType commandType = CommandType.StoredProcedure, SqlParameter[] parameters = null)
        {
            return await Task.Run(() => ConnectDb.ExecuteNonQuery(query, commandType, parameters));
        }

        /// <summary>
        /// Thực thi câu lệnh SQL và trả về giá trị duy nhất (Scalar)
        /// </summary>
        public virtual async Task<object> ExecuteScalarAsync(string query, CommandType commandType = CommandType.StoredProcedure, SqlParameter[] parameters = null)
        {
            return await Task.Run(() => ConnectDb.ExecuteScalar(query, commandType, parameters));
        }

        /// <summary>
        /// Thực thi câu lệnh SQL và trả về một đối tượng kiểu T
        /// </summary>
        public static T GetItem<T>(string query, CommandType commandType = CommandType.StoredProcedure, SqlParameter[] parameters = null)
        {
            var dt = ConnectDb.ExecuteQuery(query, commandType, parameters);
            if (dt.Rows.Count == 0)
                return default;

            var row = dt.Rows[0];
            var obj = Activator.CreateInstance<T>();
            foreach (var prop in typeof(T).GetProperties())
            {
                if (dt.Columns.Contains(prop.Name) && row[prop.Name] != DBNull.Value)
                {
                    prop.SetValue(obj, Convert.ChangeType(row[prop.Name], prop.PropertyType));
                }
            }
            return obj;
        }

        /// <summary>
        /// Thực thi câu lệnh SQL và trả về danh sách đối tượng kiểu T
        /// </summary>
        public static List<T> GetList<T>(string query, CommandType commandType = CommandType.StoredProcedure, SqlParameter[] parameters = null)
        {
            var dt = ConnectDb.ExecuteQuery(query, commandType, parameters);
            var list = new List<T>();
            foreach (DataRow row in dt.Rows)
            {
                var obj = Activator.CreateInstance<T>();
                foreach (var prop in typeof(T).GetProperties())
                {
                    if (dt.Columns.Contains(prop.Name) && row[prop.Name] != DBNull.Value)
                    {
                        prop.SetValue(obj, Convert.ChangeType(row[prop.Name], prop.PropertyType));
                    }
                }
                list.Add(obj);
            }
            return list;
        }

        /// <summary>
        /// Thực thi câu lệnh SQL và trả về một đối tượng kiểu T (Async)
        /// </summary>
        public virtual async Task<T> GetItemAsync<T>(string query, CommandType commandType = CommandType.StoredProcedure, SqlParameter[] parameters = null) where T : class, new()
        {
            var dataTable = await Task.Run(() => ConnectDb.ExecuteQuery(query, commandType, parameters));
            if (dataTable.Rows.Count == 0)
                return null;

            var row = dataTable.Rows[0];
            var obj = new T();
            foreach (var prop in typeof(T).GetProperties())
            {
                if (dataTable.Columns.Contains(prop.Name) && row[prop.Name] != DBNull.Value)
                {
                    prop.SetValue(obj, Convert.ChangeType(row[prop.Name], prop.PropertyType));
                }
            }
            return obj;
        }

        /// <summary>
        /// Thực thi câu lệnh SQL và trả về danh sách đối tượng kiểu T (Async)
        /// </summary>
        public virtual async Task<List<T>> GetListAsync<T>(string query, CommandType commandType = CommandType.StoredProcedure, SqlParameter[] parameters = null) where T : class, new()
        {
            var dataTable = await Task.Run(() => ConnectDb.ExecuteQuery(query, commandType, parameters));
            var list = new List<T>();
            foreach (DataRow row in dataTable.Rows)
            {
                var obj = new T();
                foreach (var prop in typeof(T).GetProperties())
                {
                    if (dataTable.Columns.Contains(prop.Name) && row[prop.Name] != DBNull.Value)
                    {
                        prop.SetValue(obj, Convert.ChangeType(row[prop.Name], prop.PropertyType));
                    }
                }
                list.Add(obj);
            }
            return list;
        }
    }
}