using System.Collections.Generic;


namespace pbt.Customers.Dto
{
    public class CustomerAssignToSaleDto
    {
        public long UserId { get; set; }

        public List<long> CustomerIds { get; set; } 
    }
}
