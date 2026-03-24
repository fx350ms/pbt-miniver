using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pbt.ApplicationUtils
{

    public class PrefixConst
    {
        public const string BagCode = "BB";
        public const string PackageCode = "QV";
        public const string OrderCode = "DH";
        public const string DeliveryNote = "PNX";
        public const string FakeOrderCode = "AO";
    }

    public class RoleConstants
    {
        public const string admin = "admin";
        public const string saleadmin = "saleadmin";

        public const string sale = "sale";

        public const string customer = "customer";

        public const string warehouse = "warehouse";
        public const string warehouseVN = "warehousevn";
        public const string warehouseCN = "warehousetq";
    }

    public enum CustomerStatus
    {
        Active = 1,
        Block = 2
    }

    public enum ComplaintStatus
    {
        /// <summary>
        /// Chờ tiếp nhận
        /// </summary>
        [Description("Chờ tiếp nhận")]
        Pending = 1, // Chờ tiếp nhận


        /// <summary>
        /// Đang giải quyết
        /// </summary>
        [Description("Đang giải quyết")]
        InProgress = 2, // Đang giải quyết

        /// <summary>
        /// Hoàn tiền
        /// </summary>
        [Description("Hoàn tiền")]
        Refunded = 3, // Hoàn tiền

        /// <summary>
        /// Từ chối
        /// </summary>
        [Description("Từ chối")]
        Rejected = 4 // Từ chối
    }

    public enum ComplaintResolutionType
    {
        /// <summary>
        /// Đổi trả hàng
        /// </summary>
        [Description("Đổi trả")]
        ReturnAndExchange = 1,

        /// <summary>
        /// Hoàn tiền
        /// </summary>
        [Description("Hoàn tiền")]
        Refund = 2,
    }



    public enum ShippingStatus
    {
        [Description("Cần cập nhật thông tin")]
        New = 0,

        [Description("Chờ lấy hàng")]
        PendingPickup = 1,

        [Description("Đang giao")]
        InTransit = 2,

        [Description("Đã giao")]
        Delivered = 3,

        [Description("Giao thất bại")]
        FailedDelivery = 4
    }


    public enum LineShipping
    {
        [Description("Hàng lô")]
        InformalTrade = 1,
        [Description("TMĐT")]
        Ecommerce = 2,
        [Description("Chính ngạch")]
        OfficialChannel = 3,
        [Description("Xách tay")]
        Portable = 4
    }

    public enum ShippingType
    {

        [Description("Đối tác vận chuyển")]
        Partner = 1,

        [Description("Nhân viên vận chuyển")]
        InHouse = 2
    }

    public enum PotentialLevel
    {

        [Description("Vip 1")]
        Level1 = 1,
        [Description("Vip 2")]
        Level2 = 2,
        [Description("Vip 3")]
        Level3 = 3,
        [Description("Vip 4")]
        Level4 = 4,
    }

    public enum CustomerLine
    {

        /// <summary>
        /// Vận chuyển hàng lô / Vận chuyển hàng lẻ / Tiểu ngạch
        /// </summary>
        [Description("Hàng lô")]
        Line1 = 1,

        /// <summary>
        /// Vận chuyển TMĐT
        /// </summary>
        [Description("Thương mại điện tử")]
        Line2 = 2,


        /// <summary>
        /// Chính ngạch
        /// </summary>
        [Description("Chính ngạch")]
        OfficialTransport = 3,

        /// <summary>
        /// Xách tay
        /// </summary>
        [Description("Xách tay")]
        Portable = 4
    }

    /// <summary>
    /// Enum cho phép hiển thị tên ngắn gọn hơn cho các loại vận chuyển
    /// </summary>
    public enum CustomerLineShortStr
    {
        /// <summary>
        /// Vận chuyển hàng lô / Vận chuyển hàng lẻ / Tiểu ngạch
        /// </summary>
        [Description("LO")]
        Line1 = CustomerLine.Line1,

        /// <summary>
        /// Vận chuyển TMĐT
        /// </summary>
        [Description("TMDT")]
        Line2 = CustomerLine.Line2,

        /// <summary>
        /// Chính ngạch
        /// </summary>
        [Description("CN")]
        OfficialTransport = CustomerLine.OfficialTransport,

        /// <summary>
        /// Xách tay
        /// </summary>
        [Description("XT")]
        Portable = CustomerLine.Portable
    }

    public enum OrderType
    {
        /// <summary>
        /// Bình thường
        /// </summary>
        [Description("Bình thường")]
        Regular = 1,

        /// <summary>
        ///Ký gửi
        /// </summary>
        [Description("Ký gửi")]
        Consignment = 2,

        /// <summary>
        /// Thanh toán hộ
        /// </summary>
        [Description("Thanh toán hộ")]
        PayOnBehalf = 3

    }

    public enum WarehouseStatus
    {
        [Description("Trong kho")]
        InStock = 1,
        [Description("Ngoài kho")]
        OutOfStock = 2,
    }

    public enum BagTypeEnum
    {

        /// <summary>
        /// Bao riêng
        /// </summary>
        [Description("Bao riêng")]
        SeparateBag = 1,

        /// <summary>
        /// Bao ghép
        /// </summary>
        [Description("Bao ghép")]
        InclusiveBag = 2,
    }


    public enum BagFilterPackageTypeEnum
    {

        /// <summary>
        /// Bao riêng
        /// </summary>
        [Description("Có kiện")]
        HasPackage = 1,

        /// <summary>
        /// Bao ghép
        /// </summary>
        [Description("Không kiện")]
        NoPackage = 2,
    }

    public enum ShippingMethod
    {
        [Description("Giao hàng")]
        Delivery = 1,
        [Description("Nhận tại kho")]
        Warehouse = 2,
    }

    public enum DeliveryRequestStatus
    {
        /// <summary>
        /// Yêu cầu mới
        /// </summary>
        [Description("Yêu cầu mới")]
        New = 1,

        /// <summary>
        /// Đang xử lý
        /// </summary>s 
        [Description("Đã gửi")]
        Submited = 2,
        
        /// <summary>
        /// Hoàn thành đơn
        /// </summary>
        [Description("Hoàn thành")]
        Completed = 3,

        /// <summary>
        /// Hủy
        /// </summary>
        [Description("Hủy")]
        Cancel = 4,
 
    }

    /// <summary>
    /// 
    /// </summary>
    public enum DeliveryNoteStatus
    {
        [Description("Chưa xuất")]
        New = 0,
        [Description("Đã xuất")]
        Delivered = 1,

        [Description("Hủy bỏ")]
        Cancel = 2,

    }


    public enum DeliveryNoteItemType
    {
        Package = 1,
        Bag = 2
    }

    public enum CharingSourceType
    {
        Mobile = 1,
        BackendAdmin = 2,

    }

    public enum TransactionDirectionEnum
    {
        /// <summary>
        /// Phiếu thu
        /// </summary>
        /// 
        [Description("Phiếu thu")]
        Receipt = 1,

        /// <summary>
        /// Phiếu chi
        /// </summary>
        /// 
        [Description("Phiếu chi")]
        Expense = 2
    }

    public enum TransactionStatusEnum
    {
        /// <summary>
        /// Chờ duyệt
        /// </summary>
        PendingApprove = 1,

        /// <summary>
        /// Đã duyệt
        /// </summary>
        Approved = 2,

        /// <summary>
        /// Từ chối
        /// </summary>
        Rejected = 3

    }

    public enum TransactionTypeEnum
    {
        /// <summary>
        /// Nạp tiền, cộng tiền vào tài khoản quỹ
        /// </summary>
        [Description("Nạp tiền")]
        Deposit = 1,

        /// <summary>
        /// Thanh toán, trừ tiền từ tài khoản quỹ
        /// </summary>
        /// 
        [Description("Thanh toán")]
        Payment = 2,

        /// <summary>
        /// Rút tiền, trừ tiền từ tài khoản quỹ
        /// </summary>
        /// 
        [Description("Rút tiền")]
        Withdraw = 3,

        /// <summary>
        /// Không xác định, không cộng, không trừ tài khoản quỹ
        /// </summary>
        /// 
        [Description("Khác")]
        None = 4

    }

    public enum CustomerTransactionUpdateTypeEnum
    {
        /// <summary>
        /// Không làm gì
        /// </summary>
        [Description("Không làm gì")]
        None = 0,

        ///// <summary>
        ///// Cộng công nợ
        ///// </summary>
        //[Description("Cộng công nợ")]
        //AddDebt = 1,

        ///// <summary>
        ///// Trừ công nợ
        ///// </summary>
        //[Description("Trừ công nợ")]
        //SubtractDebt = 2,

        /// <summary>
        /// Cộng ví
        /// </summary>
        [Description("Cộng ví")]
        AddWallet = 3,

        /// <summary>
        /// Trừ ví
        /// </summary>
        [Description("Trừ ví")]
        SubtractWallet = 4,
    }

    public enum TransactionSourceEnum
    {
        /// <summary>
        /// Thủ công
        /// </summary>
        Manual = 1,

        /// <summary>
        /// Tự động
        /// </summary>
        Auto = 2

    }

    public enum PackageDeliveryStatusEnum
    {
        [Description("Thiếu thông tin")] MissingInfo = 0,
        /// <summary>
        /// Khởi tạo, đã kí gửi
        /// </summary>
        [Description("Khởi tạo")] Initiate = 1,
        /// <summary>
        /// Đang ở kho QT, chờ vc về VN
        /// </summary>
        [Description("Chờ vận chuyển")] WaitingForShipping = 3,
        /// <summary>
        /// Đang vận chuyển về VN
        /// </summary>
        [Description("Đang vận chuyển")] Shipping = 4,
        /// <summary>
        /// 
        /// </summary>
        [Description("Đã về kho VN")] InWarehouseVN = 5,
        /// <summary>
        /// Chờ giao ở VN
        /// </summary>
        [Description("Chờ giao")] WaitingForDelivery = 6,

        /// <summary>
        /// Yêu cầu giao
        /// </summary>
        [Description("Yêu cầu giao")] DeliveryRequest = 7,

        /// <summary>
        /// Đang giao
        /// </summary>
        [Description("Đang giao")] DeliveryInProgress = 8,

        /// <summary>
        /// Đã giao
        /// </summary>
        [Description("Đã giao")] Delivered = 9,
        [Description("Hoàn thành")] Completed = 10, // Hoàn thành
        [Description("Khiếu nại")] Complaint = 11, // Khiếu nại

        [Description("Trung chuyển")]
        WarehouseTransfer = 13 // Chuyển kho

    }


    public enum OrderStatus
    {
        /// <summary>
        /// Đã ký gửi, thiếu thông tin
        /// </summary>
        [Description("Đã ký gửi, thiếu thông tin")]
        New = 0,
        /// <summary>
        /// Đã ký gửi
        /// </summary>
        [Description("Đã ký gửi")]
        Sent = 1,

        /// <summary>
        /// Hàng về kho TQ
        /// </summary>
        [Description("Hàng về kho TQ")]
        InChinaWarehouse = 2,

        /// <summary>
        /// Đang vận chuyển quốc tế
        /// </summary>
        [Description("Đang vận chuyển quốc tế")]
        InTransit = 3,

        /// <summary>
        /// Đã đến kho VN
        /// </summary>
        [Description("Đã đến kho VN")]
        InVietnamWarehouse = 4,

        /// <summary>
        /// Đang giao đến khách
        /// </summary>
        [Description("Đang giao đến khách")]
        OutForDelivery = 5,

        /// <summary>
        /// Đã giao
        /// </summary>
        [Description("Đã giao")]
        Delivered = 6,

        /// <summary>
        /// Khiếu nại
        /// </summary>
        [Description("Khiếu nại")]
        Complaint = 7,


        /// <summary>
        /// Hoàn tiền
        /// </summary>
        [Description("Hoàn tiền")]
        Refund = 8,

        /// <summary>
        /// Huỷ
        /// </summary>
        [Description("Huỷ")]
        Cancelled = 9,

        /// <summary>
        /// Hoàn thành đơn
        /// </summary>
        [Description("Hoàn thành đơn")]
        OrderCompleted = 10,

        [Description("Trung chuyển")]
        WarehouseTransfer = 13 // Chuyển kho
    }




    /// <summary>
    /// 
    /// </summary>
    public enum BagShippingStatus
    {
        /// <summary>
        /// Mới tạo
        /// </summary>
        [Description("Mới tạo")]
        Initiated = 1, // Mới tạo

        /// <summary>
        /// Chờ vận chuyển QT
        /// </summary>
        [Description("Chờ vận chuyển")]
        WaitingForShipping = 2, // Chờ vận chuyển

        /// <summary>
        /// Đang vận chuyển
        /// </summary>
        [Description("Đang vận chuyển")]
        InTransit = 3, // Đang vận chuyển

        /// <summary>
        /// Tới đích
        /// </summary>

        [Description("Tới đích")]
        GoToWarehouse = 4, // 

        ///// <summary>
        ///// Chờ giao hàng ở VN
        ///// </summary>
        [Description("Chờ giao")]
        WaitingForDelivery = 5, //Chờ giao

        ///// <summary>
        ///// Đang giao
        ///// </summary>
        [Description("Đang giao")]
        Delivery = 6, //đang giao

        ///// <summary>
        ///// Đã giao
        ///// </summary>
        [Description("Đã giao")]
        Delivered = 7, //Đã giao


        /// <summary>
        /// Khiếu nại đã được giải quyết
        /// </summary>
        [Description("Đã xử lý khiếu nại")]
        ComplaintResolved = 8, // Khiếu nại đã được giải quyết

        [Description("Yêu cầu giao")] DeliveryRequest = 12,

        [Description("Trung chuyển")]
        WarehouseTransfer = 13 // Chuyển kho


    }

    /// <summary>
    /// 
    /// </summary>
    public enum OperateActionType
    {
        /// <summary>
        /// 
        /// </summary>
        /// 
        [Description("Nhập")]
        In = 1, // nhập hàng
        /// <summary>
        /// 
        /// </summary>
        /// 
        [Description("Xuất")]
        Out = 2  // xuất hàng
    }

    /// <summary>
    /// 
    /// </summary>
    public enum WarehouseType
    {
        /// <summary>
        /// Kho TQ
        /// </summary>
        Cn = 1,
        /// <summary>
        /// Kho VN
        /// </summary>
        Vn = 2
    }



    public enum ReceiveMessageResult
    {
        /// <summary>
        /// Thành công
        /// </summary>
        [Description("Thành công")]
        Success = 200,

        /// <summary>
        /// Dữ liệu đầu vào không hợp lệ
        /// </summary>
        [Description("Dữ liệu đầu vào không hợp lệ")]
        InvalidInput = -201,

        /// <summary>
        /// Hệ thống gặp sự cố
        /// </summary>
        [Description("Hệ thống gặp sự cố")]
        SystemError = -202,

        /// <summary>
        /// Không tìm thấy thông tin khách hàng
        /// </summary>
        [Description("Không tìm thấy thông tin khách hàng")]
        NotFoundCustomer = -203,

        /// <summary>
        /// Sai cú pháp
        /// </summary>
        [Description("Sai cú pháp")]
        ErrorSyntax = -204,

        /// <summary>
        /// Sai cú pháp
        /// </summary>
        [Description("Không tìm thấy thông tin ngân hàng")]
        NotFoundBankAccount = -205
    }

    public enum MessageStatus
    {
        [Description("Đã nhận")]
        Received = 1,

        [Description("Đang xử lý")]
        InProcess = 2,

        [Description("Hoàn thành")]
        Completed = 3,

        [Description("Thất bại")]
        Failed = 4
    }


    public enum MessageType
    {

        /// <summary>
        /// Không rõ
        /// </summary>
        [Description("Không xác định")]
        Unknown = 0,

        /// <summary>
        /// Cộng tiền
        /// </summary>
        [Description("Cộng tiền")]
        Deposit = 1,

        /// <summary>
        /// Trừ tiền
        /// </summary>
        [Description("Trừ tiền")]
        Transfer = 2
    }

    public enum FundAccountType
    {

        /// <summary>
        /// Ngân hàng
        /// </summary>
        [Description("Ngân hàng")]
        Bank = 1,

        /// <summary>
        /// Tiền mặt
        /// </summary>
        [Description("Tiền mặt")]
        Cash = 2

    }

    public enum PackageDataFilter
    {
        [Description("Tất cả")]
        All = -1,
        [Description("Kiện ký gửi")]
        Shipment = 1,

        [Description("Kiện đã xóa")]
        Deleted = 2,
        [Description("Kiện lỗi")]
        Fail = 3,
        [Description("Kiện xuất không đóng bao")]
        NonPack = 4
    }

    public enum CodeType
    {
        [Description("Kiện hàng")]
        Package = 1,
        [Description("Bao hàng")]
        Bag = 2,
        [Description("Đơn hàng")]
        Order = 3,
        [Description("Mã vận đơn")]
        Waybill = 4
    }

    public enum PaymentStatus
    {
        [Description("Chưa thanh toán")]
        Unpaid = 1,
        [Description("Đã thanh toán")]
        Paid = 2
    }

    public enum PaymentMethod
    {
        [Description("Ví")]
        Wallet = 1,

        [Description("Công nợ")]
        Debt = 2,
    }

    public enum ApiResponseCode
    {
        [Description("Thành công")]
        Success = 1,
        [Description("Thất bại")]
        Failed = 0
    }

    public enum DeliveryFeeType
    {

        /// <summary>
        /// Kho trả phí, không thu lại tiền
        /// </summary>
        [Description("Kho trả phí, không thu lại tiền")]
        WithoutFee = 1,

        /// <summary>
        /// Kho trả phí, thu lại tiền
        /// </summary>
        [Description("Kho trả phí, thu lại tiền")]
        WithFee = 2,


        // Khách tự đến lấy
        [Description("Khách tự đến lấy")]
        CustomerPickup = 3,

        // Giao cho đối tác
        [Description("Giao cho đối tác")]
        PartnerDelivery = 4


    }


    public enum DeliveryNoteRemoveItemType
    {
        Package = 1,
        Bag = 2
    }

    public enum ShippingPartnerType
    {
        International = 0, // Vận chuyển quốc tế
        Vietnam = 1 // Vận chuyển việt nam
    }


    public enum WarehouseTransferStatusEnum
    {
        [Description("Mới tạo")]
        New = 1,
        [Description("Đang chuyển")]
        InTransit = 2,
        [Description("Đã nhận")]
        Received = 3,
    }


    public enum WarehouseTransferItemTypeEnum
    {
        Package = 1,
        Bag = 2
    }

    // ProductMiscellaneousId = 8

    /// <summary>
    /// 
    /// </summary>
    public static class ProductMiscellaneousConstants
    {
        /// <summary>
        /// ID của sản phẩm tạp
        /// </summary>
        public const int ProductMiscellaneousId = 8;
    }

    public enum ExecuteStatus
    {
        [Description("Thành công")]
        Success = 1,
        [Description("Thất bại")]
        Failed = 0
    }

    public enum CustomerType
    {
        [Description("Đại lý")]
        Agent = 1,
        [Description("Cá nhân")]
        Individual = 2
    }

    public enum InBagType
    {
        /// <summary>
        /// Trong bao
        /// </summary>
        [Description("Trong bao")]
        InBag = 1,

        /// <summary>
        /// Chưa được vào bao
        /// </summary>
        [Description("Chưa vào bao")]
        UnBag = 2
    }

    public enum ExcludeCoverWeightTypeFilter
    {
        [Description("Tất cả")]
        All = -1,

        [Description("Loại trừ kiện bì")]
        ExcludeCoverWeight = 1,

        [Description("Chỉ kiện bì")]
        CoverWeightOnly = 2
    }


    public enum EntityAuditLogMethodName
    {
        Create = 1,
        Update = 2,
        Delete = 3
    }

    public enum EntityAuditLogConnectType
    {
        Disabled = 0,
        RabbitMQ = 1,
        HttpApi = 2
    }
    //public enum InDeliveryNoteType
    //{
    //    /// <summary>
    //    /// Trong phiếu xuất
    //    /// </summary>
    //    [Description("Trong phiếu xuất")]
    //    InDeliveryNote = 1,
    //    /// <summary>
    //    /// Chưa được vào phiếu xuất
    //    /// </summary>
    //    [Description("Chưa vào phiếu xuất")]
    //    UnDeliveryNote = 2
    //}

}
