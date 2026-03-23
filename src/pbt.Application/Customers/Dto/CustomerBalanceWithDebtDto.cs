
namespace pbt.Customers.Dto
{
    public class CustomerBalanceWithDebtDto
    {
        public long CustomerId { get; set; }
        public string CustomerName { get; set; } // Tên khách hàng
        public string Username { get; set; } // Tên đăng nhập
        public decimal AvailableCreditLimit { get; set; } = 0.00m; // Giới hạn công nợ còn lại
        public decimal CurrentAmount { get; set; } // số dư hiện tại
        public decimal CurrentDebt { get; set; } // công nợ hiện tại
        public decimal MaxDebt { get; set; } // Giới hạn công nợ tối đa
    }
}
