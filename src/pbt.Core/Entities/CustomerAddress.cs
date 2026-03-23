using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pbt.Entities
{
    public class CustomerAddress : Entity<long>
    {
        /// <summary>
        /// Người nhận
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Số điện thoại
        /// </summary>
        public string PhoneNumber { get; set; }
        public long CustomerId { get; set; }
       
        public string Address { get; set; }
        public string FullAddress { get; set; }
        public bool IsDefault { get; set; }

        [DefaultValue(false)]
        public bool IsDefaultForAllCustomer { get; set; }

    }
}
