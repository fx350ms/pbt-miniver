using Abp.Application.Navigation;
using pbt.Customers.Dto;

namespace pbt.Web.Views.Shared.Components.SideBarMenu
{
    public class SideBarMenuViewModel
    {
        public UserMenu MainMenu { get; set; }
        public int NewDeliveryRequestNumber { get; set; }
        public CustomerDto? Customer { get; set; }
    }
}
