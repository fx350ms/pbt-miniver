using Abp.Application.Services;
using Abp.Domain.Repositories;
using pbt.Departments.Dto;
using pbt.Entities;
using System.Linq;
using System.Threading.Tasks;
using Abp.UI;
using pbt.Users.Dto;
using Abp.Linq.Extensions;

namespace pbt.Departments
{
    public class DepartmentAppService : AsyncCrudAppService<Department, DepartmentDto, int, PagedDepartmentResultRequestDto, CreateUpdateDepartmentDto, DepartmentDto>, IDepartmentAppService
    {

        public DepartmentAppService(IRepository<Department, int> repository)
            : base(repository)
        {

        }

        public  async Task<CreateUpdateDepartmentDto> GetAsync(int id)
        {
            var department = await Repository.GetAsync(id);
            if (department == null)
            {
                throw new UserFriendlyException($"Department with Id {id} not found");
            }
            // Ánh xạ thực thể sang DTO
            return ObjectMapper.Map<CreateUpdateDepartmentDto>(department);
        }

        protected override IQueryable<Department> CreateFilteredQuery(PagedDepartmentResultRequestDto input)
        {
            var query = Repository.GetAll()
               .WhereIf(!string.IsNullOrEmpty(input.Keyword), u => u.Name.Contains(input.Keyword) );
            return query;

        }
    }
}
