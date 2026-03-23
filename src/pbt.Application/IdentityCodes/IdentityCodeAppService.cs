using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using pbt.Authorization.Users;
using pbt.Entities;
using pbt.OrderNumbers.Dto;

namespace pbt.OrderNumbers
{
    public class IdentityCodeAppService : AsyncCrudAppService<IdentityCode, IdentityCodeDto, long, PagedResultRequestDto, IdentityCodeDto, IdentityCodeDto>, IIdentityCodeAppService
    {
        private readonly string _connectionString;
        public IdentityCodeAppService(IRepository<IdentityCode, long> repository,
            IConfiguration configuration)
            : base(repository)
        {
            _connectionString = configuration.GetConnectionString("Default");
        }

        public async Task<IdentityCodeDto> GenerateNewSequentialNumberAsync(string prefix)
        {

            long currentDate = Convert.ToInt64(DateTime.Now.ToString("yMMdd"));
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand("SP_GenerateNewSequentialNumber", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@Prefix", prefix);
                command.Parameters.AddWithValue("@Date", currentDate);
                connection.Open();
                var dataReader = await command.ExecuteReaderAsync();
                if (dataReader.Read())
                {
                    var newSequentialNumber = Convert.ToInt64(dataReader["SequentialNumber"]);
                    var newRecord = new IdentityCodeDto
                    {
                        Date = currentDate,
                        Prefix = prefix,
                        SequentialNumber = newSequentialNumber
                    };

                    return newRecord;
                }

            }
            return new IdentityCodeDto()
            {
                Date = currentDate,
                Prefix = prefix,
                SequentialNumber = 1
            };
        }


       

    }
}