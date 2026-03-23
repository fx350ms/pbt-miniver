using Abp.Application.Services;
using System.Threading.Tasks;
using pbt.Warehouses.Dto;
using System.Collections.Generic;

namespace pbt.Warehouses
{
    public interface IWarehouseAppService : IAsyncCrudAppService<Dto.WarehouseDto, int, PagedWarehouseResultRequestDto, WarehouseDto, WarehouseDto>
    {
        public Task<WarehouseDto> GetAsync(int id);


        /// <summary>
        /// Lấy danh sách kho theo countryId
        /// </summary>
        /// <param name="countryId">1.Việt Nam 2.Trung Quốc</param>
        /// <returns></returns>
        public Task<List<WarehouseDto>> GetByCountry(int countryId);

        public Task<List<WarehouseDto>> GetFull();


    }
}
