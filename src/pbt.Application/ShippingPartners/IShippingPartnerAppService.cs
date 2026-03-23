using System.Collections.Generic;
using Abp.Application.Services.Dto;
using Abp.Application.Services;
using System.Threading.Tasks;
using pbt.ShippingPartners.Dto;

namespace pbt.ShippingPartners
{
    public interface IShippingPartnerAppService : IAsyncCrudAppService<ShippingPartnerDto, int, PagedAndSortedResultRequestDto, ShippingPartnerDto, ShippingPartnerDto>
    {
		Task<ListResultDto<ShippingPartnerDto>> GetAllShippingPartnersAsync();
		public Task<ShippingPartnerDto> GetAsync(int id);
		public Task<List<ShippingPartnerDto>> GetFull();

        /// <summary>
        /// Lấy danh sách đối tác vận chuyển theo loại
        /// </summary>
        /// <param name="type">
        /// -1: Lấy tất cả
        /// 0: Đối tác vận chuyển quốc tế
        /// 1: Đối tác vận chuyển nội địa
        /// </param>
        /// <returns></returns>
        public Task<List<ShippingPartnerDto>> GetAllShippingPartnersByLocationAsync(int type);
    }
}
