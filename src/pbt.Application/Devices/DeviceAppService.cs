using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using pbt.Entities;
using pbt.Devices.Dto;

namespace pbt.Devices
{
    public class DeviceAppService : AsyncCrudAppService<Device, DeviceDto, long, PagedResultRequestDto, DeviceDto, DeviceDto>, IDeviceAppService
    {
        public DeviceAppService(IRepository<Device, long> repository)
            : base(repository)
        {
        }
    }
}