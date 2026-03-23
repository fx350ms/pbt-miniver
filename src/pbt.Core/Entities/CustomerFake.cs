using Abp.Domain.Entities;

using System;
 
namespace pbt.Entities
{
    public class CustomerFake : Entity<long>
    {
        public string FullName { get; set; }           // Họ và tên đầy đủ
        public string PhoneNumber { get; set; }        // Số điện thoại
        public string Address { get; set; }            // Địa chỉ
    }
}
