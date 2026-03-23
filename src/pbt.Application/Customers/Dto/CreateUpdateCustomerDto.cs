using Abp.Application.Services.Dto;
using System;
using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;

namespace pbt.Customers.Dto
{
    public class CreateUpdateCustomerDto : EntityDto<long>
    {

        public string Username { get; set; }
        [Required]
        [StringLength(128)]
        public string FullName { get; set; }           // Họ và tên đầy đủ

        [StringLength(256)]
        [EmailAddress]
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
        public int? AddressId { get; set; }
        public int? AddressReceiptId { get; set; }


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
        public decimal? InsurancePercentage { get; set; }
        
        [CanBeNull] public string BagPrefix { get; set; }
    }
}
