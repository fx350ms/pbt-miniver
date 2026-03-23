using Abp.Application.Services;
using System.Threading.Tasks;
using pbt.Warehouses.Dto;
using System.Collections.Generic;
using Abp.Application.Services.Dto;
using pbt.Bags.Dto;
using pbt.Packages.Dto;
using Microsoft.AspNetCore.Mvc;
using pbt.Customers.Dto;

namespace pbt.Bags
{
    public interface IBagAppService : IAsyncCrudAppService<Dto.BagDto, int, PagedResultRequestDto, BagDto, BagDto>
    {
        public Task<CreateUpdateBagDto> GetAsync(int id);
        public Task<List<BagDto>> GetFull();
        public Task<ListResultDto<BagDto>> getBagsFilter(PagedBagResultRequestDto input);
        Task<List<CustomerDto>> getCustomerFilter(PagedBagResultRequestDto input);
        public Task<ListResultDto<PackageDto>> GetPackagesByBag(PagedPackageResultRequestDto input);
        public Task<List<BagDto>> GetBagsTodayAsync();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public Task<JsonResult> AddPackageToBagAsync(AddPackageToBagRequestDto input);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Task<List<BagClosedDto>> GetBagsClosedAsync();

        public Task<BagDto> GetBagDeliveryRequestById(int id);
        public Task<BagDto> GetBagDeliveryRequestByCode(string bagCode);
        public Task<JsonResult> UpdateBagAsync(UpdateBagDto input);
        public Task<BagStampDto> GetForStampAsync(int id, bool printStamp = false);
        public Task<List<PackageDetailDto>> GetListDetailAsync(string ids);
        public Task<BagDto> CreateSimilarBagAsync(int bagId);

        /// <summary>
        /// Xóa kiện khỏi bao hàng
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public Task<JsonResult> RemovePackageFromBagAsync(AddPackageToBagRequestDto input);


        public Task<ListResultDto<BagDto>> GetDataForDownload(BagDownloadFileRequestDto input);
        public Task<ListResultDto<PackageDto>> GetDataManifestForDownload(BagDownloadFileRequestDto input);

        /// <summary>
        /// Cập nhật trạng thái bao, kiện, đơn khi về kho VN
        /// </summary>
        /// <param name="bagId"></param>
        /// <param name="bagStatus"></param>
        /// <param name="warehouseStatus"></param>
        /// <param name="orderStatus"></param>
        /// <param name="packageStatus"></param>
        /// <returns></returns>
        public Task<int> UpdateImportStatus(long bagId, int bagStatus, int warehouseStatus, int orderStatus, int packageStatus);
        Task<PagedResultDto<BagViewForPartnerDto>> GetBagsForPartnerAsync(PagedBagResultRequestDto input);
        Task<List<PackageDto>> GetPackageDataForDownload(BagDownloadFileRequestDto input);
        Task<List<BagDto>> GetByIdsAsync(List<int> ids);
    }
}
