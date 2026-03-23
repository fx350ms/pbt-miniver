using pbt.ApplicationUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pbt.Packages.Dto
{
    public class PackageDownloadByBagDto
    {
        public string PackageNumber { get; set; }

        public string BagNumber { get; set; }

        public int ShippingStatus { get; set; }

        public string TrackingNumber { get; set; }

        public string CustomerName { get; set; }

        public decimal Weight { get; set; }

        public string CurrentWarehouseName { get; set; }
        public string TargetWarehouseName { get; set; } //
        
        public string ShippingStatusName
        {
            get
            {
                var status = (PackageDeliveryStatusEnum)ShippingStatus;
                return status.GetDescription();
            }
        } // trạng thái vận chuyên

        public string WaybillNumber { get; set; }  // Mã vận đơn gốc    
        
        public decimal? Volume { get; set; } // thể tích
    }
}
