using pbt.Customers.Dto;
using pbt.Users.Dto;

namespace pbt.Web.Models.Users
{
    public class UserInformationViewModel
    {
        public CustomerDto Customer { get; set; }

        public UserDto User { get; set; }
    }
}
