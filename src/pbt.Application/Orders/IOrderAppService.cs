using System.Collections.Generic;
using Abp.Application.Services.Dto;
using Abp.Application.Services;
using pbt.Customers.Dto;
using System.Threading.Tasks;
using pbt.Orders.Dto;
using pbt.ApplicationUtils;
using pbt.Commons.Dto;
using pbt.Packages.Dto;

namespace pbt.Orders
{
    public interface IOrderAppService : IAsyncCrudAppService<OrderDto, long, PagedResultRequestDto, CreateUpdateOrderDto, OrderDto>
    {

      //  public Task<PagedResultDto<AllMyOrderItemDto>> GetAllMyOrders(PagedAndSortedOrderResultRequestDto input);
        //  public Task<PagedResultDto<OrderDto>> GetAllMyOrders(PagedAndSortedOrderResultRequestDto input);
        public Task<List<OrderDto>> GetFull();
        public Task<List<OrderDto>> GetByStatus(int status);
        public Task<OrderSummaryDto> GetSummary();
        public Task<PagedResultDto<OrderDto>> GetByCustomer(PagedOrderCustomerRequestDto input);
        public Task<PagedResultDto<OrderDto>> GetPageByCustomer(PagedResultRequestByCustomerDto input);
        public Task<OrderDto> CreateMyOrderAsync(CreateUpdateOrderDto input);
        public Task<OrderDto> CreateCustomerOrderAsync(CreateUpdateOrderDto input);
        public Task<OrderDto> CreateOrderBySaleAsync(CreateUpdateOrderDto input);


        /// <summary>
        /// Lấy danh sách đơn hàng trong kho Việt Nam theo mã khách hàng
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>

        public Task<TrackingDto> LookupAsync(string waybillCode, string phone);

        /// <summary>
        /// Lấy danh sách đơn hàng theo nhân viên kinh doanh
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public Task<PagedResultDto<OrderDto>> GetAllBySaleAsync(PagedAndSortedOrderResultRequestDto input);

        /// <summary>
        /// Cập nhật phí vận chuyển cho đơn hàng
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public Task UpdateDeliveryFee(UpdateDeliveryFeeDto input);

        public Task UpdateInsurance(UpdateInsuranceDto input);
        public Task UpdateWoodenPackagingFee(UpdateWoodenPackagingFeeDto input);

        public Task<OrderDto> GetByWaybillAsync(string waybilNumber);
        /// <summary>
        /// Lấy danh sách đơn hàng con của một đơn hàng cha
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public Task<List<OrderDto>> GetChildOrders(long orderId);
        public Task<PagedResultDto<AllMyOrderItemDto>> GetAllMyOrders(PagedMyOrderRequestDto input);

        /// <summary>
        ///     
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public Task<List<CustomerDto>> GetCustomerFilter(PagedAndSortedOrderResultRequestDto input);

        /// <summary>
        ///     
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public Task<List<OptionItemDto>> GetAllMyWaybillForSelection(GetAllMyWaybillForSelectionRequestDto input);

        public Task<List<PackageWithOrderDto>> GetAllPackageByMyOrders(PagedMyOrderRequestDto input);
        Task<List<WaybillForRematchDto>> GetWaybillByIds(string orderIds);
    }
}
