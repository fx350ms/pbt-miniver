using Microsoft.EntityFrameworkCore;
using Abp.Zero.EntityFrameworkCore;
using pbt.Authorization.Roles;
using pbt.Authorization.Users;
using pbt.MultiTenancy;
using pbt.Entities;
using pbt.Entities.Locally;

namespace pbt.EntityFrameworkCore
{
    public class pbtDbContext : AbpZeroDbContext<Tenant, Role, User, pbtDbContext>
    {
        /* Define a DbSet for each entity of the application */

        #region Locally

        public DbSet<Province> Provinces { get; set; }
        public DbSet<District> Districts { get; set; }
        public DbSet<Ward> Wards { get; set; }

        #endregion


        public DbSet<Department> Departments { get; set; }
        public DbSet<Dictionary> Dictionarys { get; set; }
        public DbSet<ShippingPartner> ShippingPartners { get; set; }
        public DbSet<Warehouse> Warehouses { get; set; }

        public DbSet<Customer> Customers { get; set; }

        public DbSet<CustomerFake> CustomerFakes { get; set; }

        public DbSet<CustomerAddress> CustomerAddresses { get; set; }
         
        public DbSet<CustomerTransaction> CustomerTransactions { get; set; }
       
        public DbSet<Package> Packages { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderHistory> OrderHistories { get; set; }
        public DbSet<OrderLog> OrderLogs { get; set; }

        public DbSet<Bag> Bags { get; set; }

        public DbSet<Complaint> Complaints { get; set; }
        public DbSet<ComplaintImage> ComplaintImages { get; set; }
        public DbSet<ComplaintReason> ComplaintReasons { get; set; }
        public DbSet<DeliveryRequest> DeliveryRequests { get; set; }
        public DbSet<DeliveryRequestItem> DeliveryRequestItems { get; set; }
        public DbSet<DeliveryRequestOrder> DeliveryRequestsOrders { get; set; }

        public DbSet<Waybill> Waybills { get; set; }
        public DbSet<BarCode> BarCodes { get; set; }
        public DbSet<DeliveryNote> DeliveryNotes { get; set; }

        public DbSet<CharingSource> CharingSources { get; set; }
        public DbSet<ChargingRequest> ChargingRequests { get; set; }


        /// <summary>
        /// Tài khoản quỹ
        /// </summary>
        public DbSet<FundAccount> FundAccounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        public DbSet<FundAccountPermission> FundAccountPermissions { get; set; }

        public DbSet<IdentityCode> IdentityCodes { get; set; }

        public DbSet<Message> Messages { get; set; }
        public DbSet<Device> Devices{ get; set; }
        public DbSet<EntityChangeDetail> EntityChangeDetail{ get; set; }
        public DbSet<EntityChangeLog> EntityChangeLog{ get; set; }

        public DbSet<ConfigurationSetting> ConfigurationSettings { get; set; }

        public DbSet<FileUpload> FileUploads { get; set; }
        public DbSet<OrderNote> OrderNotes { get; set; }
        public DbSet<WarehouseTransfer> WarehouseTransfers { get; set; }
        public DbSet<WarehouseTransferDetail> WarehouseTransferDetails { get; set; }
        public DbSet<WoodenPacking> WoodenPackings { get; set; }

        #region Tính giá vận chuyển
        public DbSet<ShippingRate> ShippingRates { get; set; }
        public DbSet<ShippingRateCustomer> ShippingRateCustomers { get; set; }
        public DbSet<ShippingRateTier> ShippingRateTiers { get; set; }
        public DbSet<ProductGroupType> ProductGroupTypes { get; set; }
        public DbSet<ShippingRateGroup> ShippingRateGroups { get; set; }

        #endregion

        #region Tính giá vốn

        public DbSet<ShippingCostGroup>  ShippingCostGroups { get; set; }
        public DbSet<ShippingCostBase>  ShippingCostBases { get; set; }
        public DbSet<ShippingCostTier>  ShippingCostTiers { get; set; }

        #endregion


        public pbtDbContext(DbContextOptions<pbtDbContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure the one-to-one relationship between User and Customer
            modelBuilder.Entity<User>()
                .HasOne(u => u.Customer)
                .WithOne()
                .HasForeignKey<User>(u => u.CustomerId)
                .IsRequired(false);
        }

    }
}
