using Abp.Application.Services;
using Abp.Domain.Repositories;
using pbt.Entities;
using System.Linq;
using System.Threading.Tasks;
using Abp.UI;
using Abp.Linq.Extensions;
using pbt.Warehouses.Dto;


using pbt.Provinces.Dto;

using System.Collections.Generic;

namespace pbt.Warehouses
{
    public class WarehouseAppService : AsyncCrudAppService<Warehouse, WarehouseDto, int, PagedWarehouseResultRequestDto, WarehouseDto, WarehouseDto>, IWarehouseAppService
    {

        public WarehouseAppService(IRepository<Warehouse, int> repository)
            : base(repository)
        {

        }

        public async Task<WarehouseDto> GetAsync(int id)
        {
            var warehouse = await Repository.GetAsync(id);
            if (warehouse == null)
            {
                throw new UserFriendlyException($"Warehouse with Id {id} not found");
            }
            // Ánh xạ thực thể sang DTO
            return ObjectMapper.Map<WarehouseDto>(warehouse);
        }

        protected override IQueryable<Warehouse> CreateFilteredQuery(PagedWarehouseResultRequestDto input)
        {
            var query = Repository.GetAll()
               .WhereIf(!string.IsNullOrEmpty(input.Keyword), u => u.Name.Contains(input.Keyword));
            return query;

        }


        public async Task<List<WarehouseDto>> GetByCountry(int countryId)
        {
            var query = Repository.GetAll()
             .WhereIf(true, u => u.CountryId == countryId && u.Status) ;
                
            return ObjectMapper.Map<List<WarehouseDto>>(query.ToList());


        }
        public async Task<List<WarehouseDto>> GetFull()
        {
            var data = await Repository.GetAllAsync();
            data = data.Where(u => u.Status);
            if (data == null)
            {
                throw new UserFriendlyException($"Warehouse is empty");
            }
            return ObjectMapper.Map<List<WarehouseDto>>(data);

        }
    }
}
