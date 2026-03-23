using Abp.Application.Navigation;
using Abp.Domain.Uow;
using Abp.Runtime.Session;
using Microsoft.AspNetCore.Mvc;
using pbt.DeliveryRequests;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using pbt.Authorization;
using pbt.Customers;
using pbt.DeliveryRequests.Dto;

namespace pbt.Web.Views.Shared.Components.SideBarMenu
{
    public class SideBarMenuViewComponent : pbtViewComponent
    {
        private readonly IUserNavigationManager _userNavigationManager;
        private readonly IAbpSession _abpSession;
        private readonly IDeliveryRequestAppService _deliveryRequestAppService;
        private readonly ICustomerAppService  _customerService;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public SideBarMenuViewComponent(
            IUserNavigationManager userNavigationManager,
            IAbpSession abpSession,
            IDeliveryRequestAppService deliveryRequestAppService,
            IUnitOfWorkManager unitOfWorkManager,
            ICustomerAppService customerService
        )
        {
            _userNavigationManager = userNavigationManager;
            _abpSession = abpSession;
            _deliveryRequestAppService = deliveryRequestAppService;
            _unitOfWorkManager = unitOfWorkManager;
            _customerService = customerService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            using (var uow = _unitOfWorkManager.Begin())
            {
                // if\
                PagedResultDto<DeliveryRequestDto> total = null;
                if (await PermissionChecker.IsGrantedAsync(PermissionNames.Pages_DeliveryRequest))
                {
                    //total = await _deliveryRequestAppService.GetNewDeliveryRequest();
                }
                
                // get current customer
                var userId = _abpSession.GetUserId();
                var customer = await _customerService.GetLoginCustomerAsync();
                
                await uow.CompleteAsync();

                var model = new SideBarMenuViewModel
                {
                    MainMenu = await _userNavigationManager.GetMenuAsync("MainMenu", _abpSession.ToUserIdentifier()),
                    //NewDeliveryRequestNumber = total != null ? total.Items.Count : 0,
                    Customer = customer
                };

                return View(model);
            }
        }
    }
}