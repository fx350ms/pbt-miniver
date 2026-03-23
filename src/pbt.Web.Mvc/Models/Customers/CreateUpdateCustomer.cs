using pbt.Customers.Dto;
using System.ComponentModel.DataAnnotations;

namespace pbt.Web.Models.Customers
{
    public class CreateUpdateCustomer 
    {
        public CustomerDto Customer { get; set; }
    }
}
