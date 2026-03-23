using Abp.AutoMapper;
using Abp.Localization;
using Abp.Modules;
using Abp.Reflection.Extensions;
using pbt.Application.WarehouseTransfers.Dto;
using pbt.ApplicationUtils;
using pbt.Authorization;
using pbt.Bags.Dto;
using pbt.BarCodes.Dto;
using pbt.CompaintReasons.Dto;
using pbt.ComplaintImages.Dto;
using pbt.Complaints.Dto;
using pbt.ConfigurationSettings.Dto;
using pbt.CustomerAddresss.Dto;
using pbt.CustomerFakes.Dto;
using pbt.Customers.Dto;
using pbt.DeliveryNotes.Dto;
using pbt.DeliveryRequests.Dto;
using pbt.Departments.Dto;
using pbt.Dictionary.Dto;
using pbt.Districts.Dto;
using pbt.Entities;
using pbt.Entities.Locally;
using pbt.EntityAuditLogs;
using pbt.FileUploads.Dto;
using pbt.FundAccounts.Dto;
using pbt.Messages.Dto;
using pbt.OrderHistories.Dto;
using pbt.OrderLogs.Dto;
using pbt.OrderNumbers.Dto;
using pbt.Orders.Dto;
using pbt.Packages.Dto;
using pbt.Provinces.Dto;
using pbt.ShippingCosts.Dto;
using pbt.ShippingPartners.Dto;
using pbt.ShippingRates;
using pbt.ShippingRates.Dto;
using pbt.Transactions.Dto;
using pbt.Wards.Dto;
using pbt.Warehouses.Dto;
using pbt.WarehouseTransfers.Dto;
using pbt.Waybills.Dto;
using pbt.WoodenPackings.Dto;
using System;


namespace pbt
{
    [DependsOn(
        typeof(pbtCoreModule),
        typeof(AbpAutoMapperModule))]
    public class pbtApplicationModule : AbpModule
    {
        public override void PreInitialize()
        {
            Configuration.BackgroundJobs.IsJobExecutionEnabled = false;
            Configuration.Authorization.Providers.Add<pbtAuthorizationProvider>();
            Configuration.Localization.Languages.Add(new LanguageInfo("vi", "Tiếng Việt", isDefault: true));
            Configuration.Auditing.IsEnabled = false;
            Configuration.Auditing.IsEnabledForAnonymousUsers = false;
           

            Configuration.Modules.AbpAutoMapper().Configurators.Add(cfg =>
            {
                // Ánh xạ từ CreateCategoryDto sang Category
                cfg.CreateMap<CreateUpdateDepartmentDto, Department>().ReverseMap();
                cfg.CreateMap<CreateUpdateDictionaryDto, pbt.Entities.Dictionary>().ReverseMap();
                cfg.CreateMap<CreateUpdateShippingPartnerDto, ShippingPartner>().ReverseMap();
                cfg.CreateMap<CreateUpdateWarehouseDto, Warehouse>().ReverseMap();
                cfg.CreateMap<CreateUpdatePackageDto, Package>().ReverseMap();
                cfg.CreateMap<EditPackageDto, Package>().ReverseMap();
                cfg.CreateMap<PackageViewForSaleDto, Package>().ReverseMap();
                cfg.CreateMap<PackageFinanceDto, Package>().ReverseMap();
                cfg.CreateMap<CreateUpdateBagDto, Bag>().ReverseMap();
                cfg.CreateMap<CreateUpdateBagDto, BagDto>().ReverseMap();
                cfg.CreateMap<UpdateBagDto, Bag>().ReverseMap();
                cfg.CreateMap<BagDto, Bag>().ReverseMap();
                cfg.CreateMap<Bag, Bag>().ReverseMap();
                cfg.CreateMap<BagStampDto, Bag>().ReverseMap();
                cfg.CreateMap<BagStampDto, BagDto>().ReverseMap();
                // Nếu cần thêm các ánh xạ khác
                cfg.CreateMap<DepartmentDto, Department>().ReverseMap();

                cfg.CreateMap<CustomerDto, Customer>().ReverseMap();
                cfg.CreateMap<CreateUpdateCustomerDto, Customer>().ReverseMap();
                cfg.CreateMap<DictionaryDto, pbt.Entities.Dictionary>().ReverseMap();
                cfg.CreateMap<WarehouseDto, Warehouse>().ReverseMap();
                cfg.CreateMap<PackageDto, Package>().ReverseMap();
                cfg.CreateMap<PackageDto, PackageDto>();
                cfg.CreateMap<Package, Package>().ReverseMap();
                cfg.CreateMap<PackageWithOrderDto, PackageDto>().ReverseMap();
                cfg.CreateMap<PackageWithOrderDto, Package>().ReverseMap();

                cfg.CreateMap<PackageDeliveryRequestDto, Package>().ReverseMap();
                cfg.CreateMap<BagDeliveryRequestDto, Bag>().ReverseMap();

                cfg.CreateMap<PackageNewByCreatorDto, Package>().ReverseMap();
                cfg.CreateMap<PackageDetailDto, Package>().ReverseMap();
                cfg.CreateMap<PackageDetailDto, PackageDto>().ReverseMap();
                cfg.CreateMap<CustomerFakeDto, CustomerFake>().ReverseMap();
                cfg.CreateMap<CreateUpdateCustomerFakeDto, CustomerFake>().ReverseMap();
                cfg.CreateMap<ShippingPartnerDto, ShippingPartner>().ReverseMap();

                cfg.CreateMap<CustomerAddressDto, CustomerAddress>().ReverseMap();

                cfg.CreateMap<ProvinceDto, Province>().ReverseMap();
                cfg.CreateMap<DistrictDto, District>().ReverseMap();
                cfg.CreateMap<WardDto, Ward>().ReverseMap();

                // Transaction
                cfg.CreateMap<CustomerTransactionDto, CustomerTransaction>().ReverseMap();
                cfg.CreateMap<OrderDto, Order>().ReverseMap();
                cfg.CreateMap<AllMyOrderItemDto, Order>().ReverseMap();

                cfg.CreateMap<OrderWaybillDto, Order>().ReverseMap();
                cfg.CreateMap<CreateMyOrderDto, Order>().ReverseMap();
                cfg.CreateMap<CreateUpdateOrderDto, Order>().ReverseMap();
                cfg.CreateMap<ComplaintDto, Complaint>().ReverseMap();
                cfg.CreateMap<CreateUpdateComplaintDto, Complaint>().ReverseMap();

                cfg.CreateMap<ComplaintImageDto, ComplaintImage>().ReverseMap();

                cfg.CreateMap<ComplaintReasonDto, ComplaintReason>().ReverseMap();

                cfg.CreateMap<OrderLogDto, OrderLog>().ReverseMap();
                cfg.CreateMap<OrderHistoryDto, OrderHistory>().ReverseMap();

                cfg.CreateMap<DeliveryRequestDto, DeliveryRequest>().ReverseMap();
                // delivery note
                cfg.CreateMap<CreateUpdateDeliveryNoteDto, DeliveryNote>().ReverseMap();
                cfg.CreateMap<DeliveryNoteDto, DeliveryNote>().ReverseMap();

                cfg.CreateMap<WaybillDto, Waybill>().ReverseMap();

                cfg.CreateMap<BarCodeDto, BarCode>().ReverseMap();

                cfg.CreateMap<ChargingRequestDto, ChargingRequest>().ReverseMap();
                cfg.CreateMap<CharingSourceDto, CharingSource>().ReverseMap();
                cfg.CreateMap<FundAccountDto, FundAccount>().ReverseMap();

                cfg.CreateMap<TransactionDto, Transaction>().ReverseMap();
                cfg.CreateMap<CreateTransactionWithAttachmentDto, Transaction>().ReverseMap();
                cfg.CreateMap<CreateTransactionWithAttachmentDto, TransactionDto>().ReverseMap();

                cfg.CreateMap<IdentityCodeDto, IdentityCode>().ReverseMap();

                cfg.CreateMap<MessageDto, Message>().ReverseMap();
                cfg.CreateMap<ReceiveMessageDto, Message>().ReverseMap();
                cfg.CreateMap<ReceiveMessageDto, MessageDto>().ReverseMap();

                cfg.CreateMap<ConfigurationSettingDto, ConfigurationSetting>().ReverseMap();
                cfg.CreateMap<FundAccountPermission, FundAccountPermissionDto>().ReverseMap();
                cfg.CreateMap<CreateFundAccountPermissionDto, FundAccountPermission>();



                cfg.CreateMap<ShippingRateCustomer, ShippingRateCustomerDto>().ReverseMap();
                cfg.CreateMap<ShippingRate, ShippingRateDto>().ReverseMap();
                cfg.CreateMap<ShippingRateTier, ShippingRateTierDto>().ReverseMap();
                cfg.CreateMap<ProductGroupType, ProductGroupTypeDto>().ReverseMap();
                cfg.CreateMap<ShippingRateGroup, ShippingRateGroupDto>().ReverseMap();

                cfg.CreateMap<FileUploadDto, FileUpload>().ReverseMap();
                cfg.CreateMap<OrderNoteDto, OrderNote>().ReverseMap();
                cfg.CreateMap<WarehouseTransferDto, WarehouseTransfer>().ReverseMap();
                cfg.CreateMap<WarehouseTransferDetailDto, WarehouseTransferDetail>().ReverseMap();
                cfg.CreateMap<WoodenPacking, WoodenPackingDto>().ReverseMap();

                cfg.CreateMap<ShippingCostGroupDto, ShippingCostGroup>().ReverseMap();
                cfg.CreateMap<ShippingCostBaseDto, ShippingCostBase>().ReverseMap();
                cfg.CreateMap<ShippingCostTierDto, ShippingCostTier>().ReverseMap();

            });
            // Register the new service
            IocManager.Register<IShippingCostAppService, ShippingCostAppService>();

            // IocManager.Register<IEntityAuditLogService, EntityAuditLogService>();
            IocManager.Register<IEntityAuditLogApiClient, EntityAuditLogApiClient>();

        }

        public override void Initialize()
        {
            var thisAssembly = typeof(pbtApplicationModule).GetAssembly();

            IocManager.RegisterAssemblyByConvention(thisAssembly);

            Configuration.Modules.AbpAutoMapper().Configurators.Add(
                // Scan the assembly for classes which inherit from AutoMapper.Profile
                cfg => cfg.AddMaps(thisAssembly)
            );
        }
    }
}
