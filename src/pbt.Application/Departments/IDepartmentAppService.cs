using Abp.Application.Services.Dto;
using Abp.Application.Services;
using pbt.Departments.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using pbt.Users.Dto;

namespace pbt.Departments
{
    public interface IDepartmentAppService : IAsyncCrudAppService<DepartmentDto, int, PagedDepartmentResultRequestDto, CreateUpdateDepartmentDto, DepartmentDto>
    {
        public Task<CreateUpdateDepartmentDto> GetAsync(int id);
    }
}
