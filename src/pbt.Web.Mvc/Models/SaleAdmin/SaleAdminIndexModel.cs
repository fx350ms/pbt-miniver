using pbt.Customers.Dto;
using pbt.ShippingPartners.Dto;
using System.Collections.Generic;

namespace pbt.Web.Models.SaleAdmin
{
    public class SaleAdminIndexModel
    {
        public int TotalCustomer { get; set; }
        public int TotalRevenue { get; set; }
        public int ConversionRate { get; set; }


        public int TotalPending { get; set; }
        public int TotalProcessing { get; set; }    
        public int TotalCompleted { get; set; }
        public int TotalCancel {  get; set; }

        public List<ShippingPartnerDto> ShippingPartners { get; set; }
    }


    public class SaleAdminOrderModel
    {
        public int TotalCustomer { get; set; }
        public int TotalRevenue { get; set; }
        public int ConversionRate { get; set; }


        public int TotalPending { get; set; }
        public int TotalProcessing { get; set; }
        public int TotalCompleted { get; set; }
        public int TotalCancel { get; set; }
    }

    public class SaleAdminImportExportModel
    {
        public int TotalCustomer { get; set; }
        public int TotalRevenue { get; set; }
        public int ConversionRate { get; set; }


        public int TotalPending { get; set; }
        public int TotalProcessing { get; set; }
        public int TotalCompleted { get; set; }
        public int TotalCancel { get; set; }

        /// <summary>
        /// Đối tác vận chuyển nội địa Việt Nam
        /// </summary>
        public List<ShippingPartnerDto> ShippingPartnersDomestic { get; set; }

        /// <summary>
        /// Đối tác vận chuyển quốc tế
        /// </summary>
        public List<ShippingPartnerDto> ShippingPartnersIntern { get; set; }

        /// <summary>
        /// Khách hàng
        /// </summary>
        public List<CustomerDto> Customers { get; set; }
    }

}
