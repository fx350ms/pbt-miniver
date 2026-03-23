using Abp.Application.Services.Dto;
using pbt.ApplicationUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pbt.Packages.Dto
{
    public class PackageNewByCreatorDto : EntityDto<int>
    {

        public string PackageNumber { get; set; }
        public string TrackingNumber { get; set; }
        public decimal? Weight { get; set; } // Giả định kiểu decimal cho Weight
        public decimal? TotalPrice { get; set; } // Giả định kiểu decimal cho TotalPrice
        public DateTime CreationTime { get; set; } // Giả định kiểu DateTime cho thời gian
        public long? CustomerId { get; set; }

        // --- Thuộc tính Khách hàng (Customer: c) ---
        // c.UserName AS CustomerName
        public string CustomerName { get; set; }

        // --- Thuộc tính Kho hàng (Warehouse: w) ---
        // w.Name AS WarehouseName
        public int? WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public string WarehouseCode { get; set; }

        public int? ShippingLineId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ShippingLineShortString
        {
            get
            {
                CustomerLineShortStr shippingLine = (CustomerLineShortStr)0;
                if (ShippingLineId != null) shippingLine = (CustomerLineShortStr)ShippingLineId;
                return shippingLine.GetDescription();
            }
        }
    }
}
