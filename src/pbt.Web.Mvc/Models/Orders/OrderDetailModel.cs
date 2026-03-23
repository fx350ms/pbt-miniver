using pbt.Orders.Dto;
using System.Collections.Generic;
using pbt.Packages.Dto;
using pbt.CustomerAddresss.Dto;
using pbt.Complaints.Dto;
using pbt.Entities;
using pbt.OrderLogs.Dto;
using pbt.OrderHistories.Dto;
using pbt.Waybills.Dto;


namespace pbt.Web.Models.Orders
{
    public class OrderDetailModel
    {
        public OrderDto Dto { get; set; }
        public PackageDto PackageDto { get; set; }
        public OrderDto ParentOrder { get; set; }
        public List<PackageDto> Packages { get; set; }
        public CustomerAddressDto CustomerAddress { get; set; }
        public List<ComplaintDto> Complaints { get; set; }
        public List<OrderHistoryDto> Histories { get; set; }
        public List<OrderLogDto> Logs { get; set; }
    }
}
