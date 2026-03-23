using Abp.Application.Services.Dto;


namespace pbt.Warehouses.Dto
{
    public class WarehouseDto : EntityDto<int>
    {
       
        public string Code { get; set; }
        public string Name { get; set; }
        public string? Phone { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public string? Ward { get; set; }
        public string Address { get; set; }
        public string FullAddress { get; set; }
        public bool Status { get; set; }

        public string Receiver { get; set; }
        public string PostCode { get; set; }
        /// <summary>
        /// 1: TQ
        /// 2: VN
        /// </summary>
        public int CountryId { get; set; }


    }

    public class WarehouseNameAndCodeDto : EntityDto<int>
    {

        public string Code { get; set; }
        public string Name { get; set; }
    }
}
