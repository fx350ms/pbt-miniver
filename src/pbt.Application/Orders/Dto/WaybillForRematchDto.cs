using Abp.Application.Services.Dto;

namespace pbt.Orders.Dto
{
    public class WaybillForRematchDto : EntityDto<long>
    {
        public string WaybillNumber { get; set; }
    }
}
