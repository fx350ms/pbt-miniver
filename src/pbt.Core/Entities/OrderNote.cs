using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;

namespace pbt.Entities
{
    public class OrderNote : Entity<long>
    {
        public long OrderId { get; set; }
        /// <summary>
        /// Nội dung ghi chú
        /// </summary>
        [Required]
        [MaxLength(1000)]
        public string Content { get; set; }

        /// <summary>
        /// Tên người tạo ghi chú
        /// </summary>
        [MaxLength(256)]
        public string CreatorUserName { get; set; }

        
        public DateTime CreationTime { get; set; } 

    }
}