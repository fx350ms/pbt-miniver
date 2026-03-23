using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pbt.Customers.Dto
{
    public class UpdateAmountDto
    {
        public long CustomerId { get; set; }

        [Range(double.MinValue, double.MaxValue, ErrorMessage = "Giá trị không hợp lệ")]
        public decimal NewAmount { get; set; }

        public string SpecialPIN { get; set; }

    }
}
