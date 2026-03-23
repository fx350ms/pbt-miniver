using System.Collections.Generic;
using Abp.Application.Services;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Microsoft.AspNetCore.Mvc;
using pbt.Bags.Dto;
using pbt.Customers.Dto;
using pbt.Packages.Dto;
using pbt.Waybills.Dto;
using pbt.Orders.Dto;

namespace pbt.Packages
{
    public interface IPackageAppService : IAsyncCrudAppService<Dto.PackageDto, int, PagedResultRequestDto, CreateUpdatePackageDto, PackageDto>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<PackageDetailDto> GetDetailAsync(int id, bool printStamp = false);
        
        public Task<List<PackageDto>> GetByOrder(long orderId);
        public Task<List<PackageDto>> GetPackageByBag(int bagId);
        
        public Task<List<PackageReadyForCreateDeliveryNoteDto>> GetPackageByCustomer(int customerId);
        public Task UpdatePackageAsync(UpdatePackageDto input);

      //  public Task<List<WaybillDto>> GetFullWaybill();

        public Task<List<PackageDetailDto>> GetListDetailAsync(string ids, bool printStamp = false);
        /// <summary>
        /// Lấy danh sách package theo yêu cầu giao hàng
        //
        /// </summary>
        /// <param name="deliveryId"></param>
        /// <returns></returns>
        public Task<List<PackageDto>> GetAllByDeliveryRequestAsync(int deliveryId);

        public Task<List<PackageDto>> GetPackageFakes(long packageId);

        /// <summary>
        /// Lấy danh sách package theo đơn hàng, bao gồm cả thông tin của bao
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public Task<List<PackageDto>> GetListWithBagInfoByOrder(long orderId);
        /// <summary>
        /// Get order by waybill number
        /// </summary>
        /// <param name="waybillNumber"></param>
        /// <returns></returns>
        public Task<OrderDto> GetByWaybillNumber(string waybillNumber);
       

        public Task<List<PackageDto>> GetPackagesByOrderIds(List<long> orderIds);


        public Task<PagedSaleViewResultDto<PackageImportExportWithBagDto>> GetPackageImportExportWithBagAsync(ImportExportWithBagRequestDto input);
        Task RemoveQuickBaggingAsync(PackageRemoveQuickBaggingDto input);
        Task<List<PackageDto>> GetByIdsAsync(List<int> ids);
        Task<List<PackageDto>> GetPackagesByDeliveryNoteIdAsync(long deliveryNoteId);
        Task<PagedResultDto<PackageDto>> GetAllPackagesByCurrentUserOrderViewAsync(PagedPackageResultRequestDto input);
        Task<List<PackageDto>> GetPackagesByOrderId(long orderId);
        Task<PackageFinanceDto> GetWithFinanceAsync(int id);
        Task<List<PackageDownloadByBagDto>> GetPackageDownloadByBag(BagDownloadFileRequestDto input);
        Task<PagedPackagesFilterResultDto> GetAllPackagesFilterAsync(PagedPackageResultRequestDto input);
        Task EditStatus(EditPackageStatusInputDto input);
        Task<PackageEditByAdminDto> GetForAdminEditByIdAsync(int id);
        Task<List<PackageByBagDetailDto>> GetAllPackagesListByBagIdAsync(int bagId);
    }
}
