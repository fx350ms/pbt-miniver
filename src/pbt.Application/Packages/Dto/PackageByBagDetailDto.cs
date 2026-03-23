using Abp.Application.Services.Dto;
using JetBrains.Annotations;
using pbt.ApplicationUtils;
using pbt.Bags.Dto;
using pbt.Customers.Dto;
using pbt.Entities;
using pbt.Orders.Dto;
using pbt.ShippingPartners.Dto;
using pbt.Warehouses.Dto;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pbt.Packages.Dto
{
    public class PackageByBagDetailDto
    {
        public int Id { get; set; }
        public string PackageNumber { get; set; }
        public string TrackingNumber { get; set; }
        public string WaybillNumber { get; set; }
        public string CustomerName { get; set; }
        public decimal? Weight { get; set; } // kg
        public string WeightString => Weight.HasValue ? Weight.Value.ToString("0.##") : string.Empty;
        public decimal? Volume { get; set; } // thể tích
        
        public string VolumeString => Volume.HasValue ? Volume.Value.ToThousandFormat() : string.Empty;
        public string Note { get; set; }
      

        public DateTime? DeliveryTime { get; set; } // thời gian xuất kho

        public DateTime? ExportDate { get; set; } // thời gian xuất kho ở TQ
        public DateTime? ImportDate { get; set; } // thời gian nhập kho ở VN

        public string CurrentWarehouseName { get; set; }
        public int WarehouseStatus { get; set; } // trạng thái kho

        public int ShippingStatus { get; set; } // trạng thái vận chuyên


        public DateTime CreationTime { get; set; } // thời gian tạo
        public string CreationTimeFormat => CreationTime.ToString("dd/MM/yyyy HH:mm"); // thời gian tạo

        public DateTime? MatchTime { get; set; } // thời gian khớp

        public string MatchTimeFormat
        {
            get { return MatchTime is not null ? MatchTime?.ToString("dd/MM/yyyy HH:mm") : null; }
        } // thời gian xuất kho

        public bool IsRepresentForWeightCover { get; set; }
    }
}
