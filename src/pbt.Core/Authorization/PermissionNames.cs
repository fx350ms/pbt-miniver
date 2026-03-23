using System;

namespace pbt.Authorization
{
    public static class PermissionNames
    {

        #region Trang/Link truy cập
        public const string Pages_Tenants = "Pages.Tenants";
        public const string Pages_Users = "Pages.Users";
        public const string Pages_Users_Activation = "Pages.Users.Activation";
        public const string Pages_Roles = "Pages.Roles";
        public const string Pages_Departments = "Pages.Departments";
        public const string Pages_Dictionaries = "Pages.Dictionaries";
        public const string Pages_CustomerCode = "Page.CustomerCode";
        public const string Pages_TrackingNumber = "Page.TrackingNumber";
        public const string Pages_PackageCode = "Page.PackageCode";
        public const string Pages_Weight = "Page.Weight";
        public const string Pages_Service = "Page.Service";
        public const string Pages_SellingPrice = "Page.SellingPrice";
        public const string Pages_Line = "Page.Line";
        public const string Pages_VCQTPartner = "Page.VCQTPartner";
        public const string Pages_VCQTCostPrice = "Page.VCQTCostPrice";
        public const string Pages_VNCDCostPrice = "Page.VNCDCostPrice";
        public const string Pages_VNCDPartner = "Page.VNCDPartner";
        public const string Pages_TotalAmount = "Page.TotalAmount";
        public const string Pages_Status = "Page.Status";
        public const string Pages_Action = "Page.Action";
        public const string Pages_Profit = "Page.Profit";
        public const string Pages_Complaint = "Page.Complaint";
        public const string Pages_Orders = "Pages.Orders";
        public const string Pages_Customers = "Pages.Customers";
        public const string Pages_IndexChild = "Pages.IndexChild";
        public const string Pages_CustomerFakes = "Pages.CustomerFakes";
        public const string Pages_Warehouses = "Pages.Warehouses";
        public const string Pages_WarehouseTransfer = "Pages.WarehouseTransfer";
        public const string Pages_ShippingPartners = "Pages.ShippingPartners";
        public const string Pages_Export = "Pages.Export";
        public const string Pages_DeliveryRequest = "Pages.DeliveryRequest";
        public const string Pages_Bags = "Pages.Bags";
        public const string Pages_Packages = "Pages.Packages";
        public const string Pages_BagWithPartner = "Pages.BagWithPartner";// Bao đối tác
        public const string Pages_ImportExport = "Pages.ImportExport"; // Nhập xuất

        public const string Pages_Transactions = "Pages.Transactions";
        public const string Pages_RequestRefund = "Pages.RequestRefund";
        public const string Pages_Configs = "Pages.Configs";
        public const string Pages_OrderCode = "Pages.OrderCode";
        public const string Pages_Waybill = "Pages.Waybill";
        public const string Pages_TransactionHistory = "Pages.TransactionHistory";
        public const string Pages_Report = "Pages.Report";

        public const string Pages_Process = "Pages.Process";
        public const string Pages_FundAccount = "Pages.FundAccount";
        public const string Pages_PendingTransactionVoucher = "Pages.PendingTransactionVoucher";
        public const string Pages_PaymentVoucher = "Pages.PaymentVoucher";
        public const string Pages_ReceiptVoucher = "Pages.ReceiptVoucher";

        public const string Pages_CustomerTransactions = "Pages.CustomerTransactions";
        
        public const string Pages_DeliveryRequest_edit = "Pages.DeliveryRequest_edit";

        public const string Pages_DeliveryNote_ViewAllWarehouse = "Pages.DeliveryNote.ViewAllWarehouse";

        #endregion

        #region Role
        public const string Role_Admin = "Role.Admin";
        public const string Role_Warehouse = "Role.WareHouse";
        public const string Role_WarehouseVN = "Role.WareHouseVN";
        public const string Role_WarehouseCN = "Role.WareHouseCN";
        public const string Role_Sale = "Role.Sale";
        public const string Role_SaleAdmin = "Role.SaleAdmin";
        public const string Role_Customer = "Role.Customer";
        public const string Role_Accounting = "Role.Accounting";
        public const string Role_SaleCustom = "Role.SaleCustom";
        #endregion
        #region Function
        public const string Function_ViewAllCustomer = "Function.ViewAllCustomer";
        public const string Function_Customer = "Function.Customer";
        public const string Function_ViewAllOrder = "Function.Order.ViewAll";

        public const string Function_TransactionViewAll = "Function.Transaction.ViewAll";
        public const string Function_TransactionApprove = "Function.Transaction.Approve";
        
        public const string Function_ScanCodeViewAll = "Function.ScanCode.ViewAll";
        public const string Function_PackageViewAll = "Function.Package.ViewAll";
        public const string Function_PackageCreate = "Function.Package.Create";
        public const string Function_PackageEdit = "Function.Package.Edit";
        public const string Function_BagViewAll = "Function.Bag.ViewAll";

        public const string Function_BagDelete = "Function.Bag.Delete";
        public const string Function_BagCreate = "Function.Bag.Create";
        public const string Function_BagEdit= "Function.Bag.Edit";

        /// <summary>
        /// Cho phép sửa số tiền khách hàng
        /// </summary>
        public const string Function_EditCustomerAmount = "Function.EditCustomerAmount";

        /// <summary>
        /// Cho phép sửa mã PIN đặc biệt của User
        /// </summary>
        public const string Function_EditUserSpecialPIN = "Function.EditUserSpecialPIN";

        public const string Function_Package_Finance = "Function.Package.Finance";
        public const string Function_Order_Rematch = "Function.Order.Rematch";

        #endregion
    }
}
