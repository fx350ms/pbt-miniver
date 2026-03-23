using Abp.Application.Services.Dto;

namespace pbt.Packages.Dto
{
    public class PackageInVietnamWarehouseDto : EntityDto<long>
    {
        /// <summary>
        /// Mã vận đơn
        /// </summary>
        public string TrackingNumber { get; set; }

        /// <summary>
        /// Mã kiện
        /// </summary>
        public string PackageNumber { get; set; }

        /// <summary>
        /// Mã bao
        /// </summary>
        public string BagNumber { get; set; }

        /// <summary>
        /// Tổng kiện
        /// </summary>
        public int TotalPackages { get; set; }

        /// <summary>
        /// Kho
        /// </summary>
        public string WarehouseName { get; set; }

        /// <summary>
        /// Tổng thanh toán
        /// </summary>
        public decimal TotalPayment { get; set; }

        /// <summary>
        /// Đã thanh toán
        /// </summary>
        public decimal PaidAmount { get; set; }

        /// <summary>
        /// Cần thanh toán
        /// </summary>
        public decimal AmountDue { get; set; }

        /// <summary>
        /// Địa chỉ nhận hàng
        /// </summary>
        public string DeliveryAddress { get; set; }
    }
}