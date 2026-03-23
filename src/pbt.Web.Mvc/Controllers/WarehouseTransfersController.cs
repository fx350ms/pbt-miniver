using Abp.Application.Services.Dto;
using Abp.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp;
using Newtonsoft.Json;
using pbt.Application.WarehouseTransfers;
using pbt.Authorization;
using pbt.Controllers;
using pbt.Customers;
using pbt.Warehouses;
using pbt.WarehouseTransfers;
using pbt.Web.Models.Warehouses;
using pbt.Web.Models.WarehouseTransfers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace pbt.Web.Controllers
{
    // [AbpMvcAuthorize(PermissionNames.Pages_Departments)]
    //  [AbpMvcAuthorize]
    public class WarehouseTransfersController : pbtControllerBase
    {
        private readonly IWarehouseAppService _warehouseAppService;
        private readonly IWarehouseTransferAppService _warehouseTransferAppService;
        private readonly IWarehouseTransferDetailAppService _warehouseTransferDetailAppService;
        private readonly ICustomerAppService _customerAppService;
        private pbtAppSession _pbtAppSession;

        public WarehouseTransfersController(IWarehouseAppService warehouseAppService,
            IWarehouseTransferAppService warehouseTransferAppService,
            IWarehouseTransferDetailAppService warehouseTransferDetailAppService,
            ICustomerAppService customerAppService,

            pbtAppSession pbtAppSession
            )
        {
            _warehouseAppService = warehouseAppService;
            _warehouseTransferAppService = warehouseTransferAppService;
            _warehouseTransferDetailAppService = warehouseTransferDetailAppService;
            _customerAppService = customerAppService;
            _pbtAppSession = pbtAppSession;
        }

        public async Task<IActionResult> Index()
        {
            var warehouses = await _warehouseAppService.GetByCountry(2);
            var customers = await _customerAppService.GetCustomerListByCurrentUserWarehouseByAsync();
            var model = new WarehouseTransferIndexViewModel
            {
                Warehouses = warehouses,
                Customers = customers,
            };
            return View(model);
          
        }


        public async Task<IActionResult> Create()
        {
            var warehouses = await _warehouseAppService.GetByCountry(2);
            var customers = await _customerAppService.GetCustomerListByCurrentUserWarehouseByAsync();
            var currentUserWarehouseId = _pbtAppSession.WarehouseId;
            var model = new CreateWarehouseTransferViewModel
            {
                Warehouses = warehouses,
                Customers = customers,
                CurrentUserWarehouseId = currentUserWarehouseId

            };
            return View(model);
        }

        public async Task<ActionResult> Detail(int id)
        {
            // Lấy thông tin phiếu chuyển kho từ database
            var dto = await _warehouseTransferAppService.GetAsync(new EntityDto<int> { Id = id });

            if (dto == null)
            {
                throw new UserFriendlyException("Không tìm thấy phiếu chuyển kho.");
            }

            var customer = await _customerAppService.GetCustomerById(dto.CustomerId);


            var details = await _warehouseTransferDetailAppService.GetDetailsByWarehouseTransferIdAsync(id);
            var model = new WarehouseTransferDetailViewModel()
            {
                Dto = dto,
                CustomerName = customer?.Username,
                Items = details
            };

            return View(model);
        }
    }
}
