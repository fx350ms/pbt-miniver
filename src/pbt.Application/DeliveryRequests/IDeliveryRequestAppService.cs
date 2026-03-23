using System.Collections.Generic;
using Abp.Application.Services;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Microsoft.AspNetCore.Mvc;
using pbt.DeliveryRequests.Dto;
using pbt.Packages.Dto;
using pbt.Commons.Dto;
using pbt.Orders.Dto;

namespace pbt.DeliveryRequests
{
    public interface IDeliveryRequestAppService : IAsyncCrudAppService<Dto.DeliveryRequestDto, int, PagedDeliveryRequestsResultRequestDto, DeliveryRequestDto, DeliveryRequestDto>
    {

        /// <summary>
        /// Tạo yêu cầu giao hàng
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public Task<DeliveryRequestDto> CreateDeliveryRequestAsync(CreateUpdateDeliveryRequestDto input);
        /// <summary>
        /// Lấy danh sách yêu cầu giao hàng
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>

       // public Task<JsonResult> CreateDeliveryRequest(CreateUpdateDeliveryRequestDto input);

        public Task<PagedResultDto<DeliveryRequestDto>> GetDeliveryRequestFilter(PagedDeliveryRequestsResultRequestDto input);

        /// <summary>
        /// Lấy tổng trọng lượng của yêu cầu giao hàng
        /// </summary>
        /// <param name="deliveryRequestId"></param>
        /// <returns></returns>
        public decimal? GetTotalWeightDeliveryRequest(int deliveryRequestId);

        /// <summary>
        /// Lấy các kiện hàng của yêu cầu giao hàng
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public Task<PagedResultDto<PackageDto>> GetPackages(PagedPackagesResultRequestDto input);

        /// <summary>
        /// Lấy danh sách yêu cầu giao mới
        /// </summary>
        /// <returns></returns>
        public Task<int> GetNewDeliveryRequest();

        /// <summary>
        /// Cập nhật trạng thái yêu cầu giao hàng
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public Task<JsonResult> UpdateDeliveryRequest(int id, int status);

        /// <summary>
        /// Hủy yêu cầu giao hàng
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public Task<JsonResult> CancelDeliveryRequest(int Id);


        /// <summary>
        /// Xác nhận thanh toán yêu cầu giao hàng
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public Task<PagedResultDto<ItemInHouseVnDto>> GetOrdersInVietnamWarehouseByCustomerId(PagedResultRequestByCustomerDto input);

    }
}

