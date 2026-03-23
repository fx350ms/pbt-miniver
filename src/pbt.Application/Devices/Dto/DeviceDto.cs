using Abp.Application.Services.Dto;

namespace pbt.Devices.Dto
{
    public class DeviceDto : FullAuditedEntityDto<long>
    {
        public string DeviceName { get; set; } // Tên thiết bị
        public string DeviceId { get; set; } // ID thiết bị: Serial Number
        public bool Enable { get; set; }
    }
}