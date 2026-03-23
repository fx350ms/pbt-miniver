using Abp.Application.Services;
using Abp.Application.Services.Dto;
using pbt.Devices.Dto;

namespace pbt.Devices
{
    public interface IDeviceAppService : IAsyncCrudAppService<DeviceDto, long, PagedResultRequestDto, DeviceDto, DeviceDto>
    {
    }
}