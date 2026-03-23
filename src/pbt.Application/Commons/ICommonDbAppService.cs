using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Dependency;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Threading.Tasks;


namespace pbt.Application.Common
{
    public interface ICommonDbAppService<TEntityDto, TPrimaryKey, in TGetAllInput, in TCreateInput, in TUpdateInput>
       : IAsyncCrudAppService<TEntityDto, TPrimaryKey, TGetAllInput, TCreateInput, TUpdateInput>
       where TEntityDto : IEntityDto<TPrimaryKey>
       where TUpdateInput : IEntityDto<TPrimaryKey>
    {
        /// <summary>
        /// Thực thi câu lệnh SQL và trả về DataTable
        /// </summary>
        Task<DataTable> ExecuteQueryAsync(string query, CommandType commandType = CommandType.StoredProcedure, SqlParameter[] parameters = null);

        /// <summary>
        /// Thực thi câu lệnh SQL không trả về kết quả (INSERT/UPDATE/DELETE)
        /// </summary>
        Task<int> ExecuteNonQueryAsync(string query, CommandType commandType = CommandType.StoredProcedure, SqlParameter[] parameters = null);

        /// <summary>
        /// Thực thi câu lệnh SQL và trả về giá trị duy nhất (Scalar)
        /// </summary>
        Task<object> ExecuteScalarAsync(string query, CommandType commandType = CommandType.StoredProcedure, SqlParameter[] parameters = null);


    }
}