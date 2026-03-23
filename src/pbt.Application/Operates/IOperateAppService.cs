using System.Threading.Tasks;
using Abp.Application.Services;
using Microsoft.AspNetCore.Mvc;
using pbt.Operates.Dto;

namespace pbt.Departments
{
    public interface IOperateAppService : IApplicationService
    {
        public Task<JsonResult> UpdatePackageShippingStatus(OperateActionDto input);
    }
}
