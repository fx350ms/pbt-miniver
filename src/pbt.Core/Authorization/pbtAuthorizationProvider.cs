using Abp.Authorization;
using Abp.Localization;
using Abp.MultiTenancy;

namespace pbt.Authorization
{
    public class pbtAuthorizationProvider : AuthorizationProvider
    {
        public override void SetPermissions(IPermissionDefinitionContext context)
        {
            context.CreatePermission(PermissionNames.Pages_Users, L("Pages_Users"));
            context.CreatePermission(PermissionNames.Pages_Users_Activation, L("Pages_UsersActivation"));
            context.CreatePermission(PermissionNames.Pages_Roles, L("Pages_Roles"));
            context.CreatePermission(PermissionNames.Pages_Tenants, L("Pages_Tenants"), multiTenancySides: MultiTenancySides.Host);
            context.CreatePermission(PermissionNames.Pages_Dictionaries, L("Pages_Dictionaries"));
          //  context.CreatePermission(PermissionNames.Pages_ShippingPartners, L("ShippingPartner"));
            //context.CreatePermission(PermissionNames.Pages_Warehouses, L("Warehouse"));

            context.CreatePermission(PermissionNames.Pages_CustomerCode, L("Pages_CustomerCode"));
            context.CreatePermission(PermissionNames.Pages_TrackingNumber, L("Pages_TrackingNumber"));
            context.CreatePermission(PermissionNames.Pages_PackageCode, L("Pages_PackageCode"));
            context.CreatePermission(PermissionNames.Pages_Weight, L("Pages_Weight"));
            context.CreatePermission(PermissionNames.Pages_Service, L("Pages_Service"));
            context.CreatePermission(PermissionNames.Pages_SellingPrice, L("Pages_SellingPrice"));
            context.CreatePermission(PermissionNames.Pages_Line, L("Pages_Line"));

            context.CreatePermission(PermissionNames.Pages_VCQTPartner, L("Pages_VCQTPartner"));
            context.CreatePermission(PermissionNames.Pages_VCQTCostPrice, L("Pages_VCQTCostPrice"));
            context.CreatePermission(PermissionNames.Pages_VNCDCostPrice, L("Pages_VNCDCostPrice"));
            context.CreatePermission(PermissionNames.Pages_VNCDPartner, L("Pages_VNCDPartner"));
            context.CreatePermission(PermissionNames.Pages_TotalAmount, L("Pages_TotalAmount"));
            context.CreatePermission(PermissionNames.Pages_Status, L("Pages_Status"));
            context.CreatePermission(PermissionNames.Pages_Action, L("Pages_Action"));
            context.CreatePermission(PermissionNames.Pages_Profit, L("Pages_Profit"));
            context.CreatePermission(PermissionNames.Pages_Complaint, L("Pages_Complaint"));

            context.CreatePermission(PermissionNames.Pages_Orders, L("Đơn hàng"));

            context.CreatePermission(PermissionNames.Pages_Customers, L("Pages_Customers"));
            context.CreatePermission(PermissionNames.Pages_CustomerFakes, L("Pages_CustomerFakes"));
            context.CreatePermission(PermissionNames.Pages_Warehouses, L("Pages_Warehouses"));
            context.CreatePermission(PermissionNames.Pages_WarehouseTransfer, L("Pages_WarehouseTransfer"));
            
            context.CreatePermission(PermissionNames.Pages_ShippingPartners, L("Pages_ShippingPartners"));
            context.CreatePermission(PermissionNames.Pages_Departments, L("Pages_Departments"));
            context.CreatePermission(PermissionNames.Pages_Bags, L("Pages_Bags"));
            context.CreatePermission(PermissionNames.Pages_Packages, L("Pages_Packages"));
            context.CreatePermission(PermissionNames.Pages_DeliveryRequest, L("Pages_DeliveryRequest"));
            context.CreatePermission(PermissionNames.Pages_RequestRefund, L("Pages_RequestRefund"));
            context.CreatePermission(PermissionNames.Pages_Configs, L("Pages_Configs"));
            context.CreatePermission(PermissionNames.Pages_Export, L("Pages_Export"));
            context.CreatePermission(PermissionNames.Pages_Transactions, L("Pages_Transactions"));
            context.CreatePermission(PermissionNames.Pages_OrderCode, L("Pages_OrderCode"));
            context.CreatePermission(PermissionNames.Pages_TransactionHistory, L("Pages_TransactionHistory"));
            context.CreatePermission(PermissionNames.Pages_Report, L("Pages_Report"));

            context.CreatePermission(PermissionNames.Pages_Process, L("Pages_Process"));
            context.CreatePermission(PermissionNames.Pages_FundAccount, L("Pages_FundAccount"));
            context.CreatePermission(PermissionNames.Pages_PendingTransactionVoucher, L("Pages_PendingTransactionVoucher"));
            context.CreatePermission(PermissionNames.Pages_PaymentVoucher, L("Pages_PaymentVoucher"));
            context.CreatePermission(PermissionNames.Pages_ReceiptVoucher, L("Pages_ReceiptVoucher"));

            context.CreatePermission(PermissionNames.Pages_ImportExport, L("Pages_ImportExport"));
            context.CreatePermission(PermissionNames.Pages_BagWithPartner, L("Pages_BagWithPartner"));

            context.CreatePermission(PermissionNames.Role_Admin, L("Role.Admin"));
            context.CreatePermission(PermissionNames.Role_Warehouse, L("Role.WareHouse"));
            context.CreatePermission(PermissionNames.Role_WarehouseVN, L("Role.WareHouseVN"));
            context.CreatePermission(PermissionNames.Role_WarehouseCN, L("Role.WareHouseCN"));
            context.CreatePermission(PermissionNames.Role_Accounting, L("Role.Accounting"));
            context.CreatePermission(PermissionNames.Role_Customer, L("Role.Customer"));
            context.CreatePermission(PermissionNames.Role_Sale, L("Role.Sale"));
            context.CreatePermission(PermissionNames.Role_SaleAdmin, L("Role.SaleAdmin"));
            context.CreatePermission(PermissionNames.Role_SaleCustom, L("Role.SaleCustom"));
            

            context.CreatePermission(PermissionNames.Function_Customer, L("Function.Customer"));
            context.CreatePermission(PermissionNames.Function_ViewAllCustomer, L("Function.ViewAllCustomer"));
            context.CreatePermission(PermissionNames.Pages_Waybill, L("Pages_Waybill"));
            context.CreatePermission(PermissionNames.Pages_CustomerTransactions, L("Pages_CustomerTransactions"));
            context.CreatePermission(PermissionNames.Function_ViewAllOrder, L("Function_OrderViewAll"));
            context.CreatePermission(PermissionNames.Function_TransactionViewAll, L("Function_TransactionViewAll"));
            context.CreatePermission(PermissionNames.Function_TransactionApprove, L("Function_TransactionApprove"));

            context.CreatePermission(PermissionNames.Function_ScanCodeViewAll, L("Function_ScanCodeViewAll"));
            context.CreatePermission(PermissionNames.Function_PackageViewAll, L("Function_PackageViewAll"));
            context.CreatePermission(PermissionNames.Function_BagViewAll, L("Function_BagViewAll"));
            context.CreatePermission(PermissionNames.Pages_IndexChild, L("Function_IndexChild"));
            context.CreatePermission(PermissionNames.Function_BagDelete, L("Function_BagDelete"));
            context.CreatePermission(PermissionNames.Pages_DeliveryRequest_edit, L("Pages_DeliveryRequest_edit"));
            context.CreatePermission(PermissionNames.Pages_DeliveryNote_ViewAllWarehouse, L("Pages_DeliveryNote_ViewAllWarehouse"));

            context.CreatePermission(PermissionNames.Function_EditCustomerAmount, L("Function_EditCustomerAmount"));
            context.CreatePermission(PermissionNames.Function_EditUserSpecialPIN, L("Function_EditUserSpecialPIN"));

            context.CreatePermission(PermissionNames.Function_Package_Finance, L("Function_Package_Finance"));

            context.CreatePermission(PermissionNames.Function_BagCreate, L("Function_BagCreate"));
            context.CreatePermission(PermissionNames.Function_BagEdit, L("Function_BagEdit"));

            context.CreatePermission(PermissionNames.Function_PackageCreate, L("Function_PackageCreate"));
            context.CreatePermission(PermissionNames.Function_PackageEdit, L("Function_PackageEdit"));

            context.CreatePermission(PermissionNames.Function_Order_Rematch, L("Function_Order_Rematch"));
        }

        private static ILocalizableString L(string name)
        {
            return new LocalizableString(name, pbtConsts.LocalizationSourceName);
        }
    }
}
