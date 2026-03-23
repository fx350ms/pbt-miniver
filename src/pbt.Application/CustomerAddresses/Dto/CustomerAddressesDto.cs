using Abp.Application.Services.Dto;
 

namespace pbt.CustomerAddresss.Dto
{
    public class CustomerAddressDto : FullAuditedEntityDto<long>
    {
        
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public long CustomerId { get; set; }
        public int ProvinceId { get; set; }
        public int DistrictId { get; set; }
        public int WardId { get; set; }
        public string Address { get; set; }
        public string FullAddress { get; set; }
        public bool IsDefault { get; set; }
        public bool IsReceiptAddress { get; set; }

    }
}
