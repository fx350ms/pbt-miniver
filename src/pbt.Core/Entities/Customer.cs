using Abp.Domain.Entities.Auditing;
using pbt.Authorization.Users;
using System;
using System.ComponentModel;
using JetBrains.Annotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pbt.Entities
{
    public class Customer : FullAuditedEntity<long>
    {
        public string CustomerCode { get; set; }    // Mã khách hàng
        public string FullName { get; set; }           // Họ và tên đầy đủ
        public string Email { get; set; }              // Email
        public string PhoneNumber { get; set; }        // Số điện thoại
        public string Address { get; set; }            // Địa chỉ
        public string AddressReceipt { get; set; }     // Địa chỉ nhận hàng
        public DateTime? DateOfBirth { get; set; }     // Ngày sinh
        public string Gender { get; set; }             // Giới tính
        public DateTime RegistrationDate { get; set; } // Ngày đăng ký
        public int Status { get; set; }             // Trạng thái
        public string Notes { get; set; }              // Ghi chú
        public string RefCode { get; set; }

        public int? PiorityStorageId { get; set; }
        
        public long? UserId { get; set; } // ID của tài khoản (User)
        [CanBeNull]
        [ForeignKey("UserId")]
        public virtual User User { get; set; } // Tài khoản khách hàng (User)


        public string Username { get; set; } // Tài khoản khách hàng
        public int? AddressId { get; set; }
        public int? AddressReceiptId { get; set; }

        public decimal CurrentAmount { get; set; }
        public decimal CurrentDebt { get; set; }
        public decimal MaxDebt { get; set; }

        public int VipLevel { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int LineId { get; set; }

        /// <summary>
        /// Tiềm năng khách hàng
        /// </summary>
        public int PotentialLevel { get; set; }

        /// <summary>
        /// Là Khách hàng đại lý
        /// </summary>
        public bool IsAgent { get; set; }

        /// <summary>
        /// Cấp của đại lý
        /// </summary>
        public int AgentLevel { get; set; }

        /// <summary>
        /// Khách hàng cha
        /// </summary>
        public long ParentId { get; set; }

        /// <summary>
        /// Id của nhân viên kinh doanh
        /// </summary>
        public long SaleId { get; set; }
        
        public int? WarehouseId { get; set; }
        [DefaultValue(0)]
        public decimal? InsurancePercentage { get; set; }
        
        [CanBeNull] public virtual Warehouse Warehouse { get; set; }
        
        // BagPrefix (max length 2)
        [CanBeNull] public string BagPrefix { get; set; }

    }
}
