using System.ComponentModel.DataAnnotations;

namespace pbt.Customers.Dto
{
    public class UpdateMaxDebtDto
    {
        public long CustomerId { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Giá trị không hợp lệ")]
        public decimal MaxDebt { get; set; }
    }
}
