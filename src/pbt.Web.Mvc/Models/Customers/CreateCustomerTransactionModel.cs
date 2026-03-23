using pbt.Customers.Dto;

namespace pbt.Web.Models.Customers
{
    public class CreateCustomerTransactionModel
    {
        public long CustomerId { get; set; }
        public CustomerDto Dto { get; set; }

    }
}
