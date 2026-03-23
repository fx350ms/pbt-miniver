using Abp.Application.Services.Dto;
using Abp.Application.Services;
using Abp.Domain.Repositories;
using pbt.Entities;
using System.Threading.Tasks;
using Abp.UI;
using pbt.ShippingPartners.Dto;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using pbt.Authorization.Users;

namespace pbt.ShippingPartners
{
    public class ShippingPartnerAppService : AsyncCrudAppService<ShippingPartner, ShippingPartnerDto, int, PagedAndSortedResultRequestDto, ShippingPartnerDto, ShippingPartnerDto>, IShippingPartnerAppService
    {
        private readonly UserManager _userManager;
        public ShippingPartnerAppService(
            IRepository<ShippingPartner, int> repository,
            UserManager userManager
            )
            : base(repository)
        {
            _userManager = userManager;
        }

        public  async Task<ShippingPartnerDto> GetAsync(int id)
        {
            var department = await Repository.GetAsync(id);
            if (department == null)
            {
                throw new UserFriendlyException($"Department with Id {id} not found");
            }
            // Ánh xạ thực thể sang DTO
            return ObjectMapper.Map<ShippingPartnerDto>(department);
        }
        /// <summary>
        /// Lấy danh sách đối tác vận chuyển
        /// </summary>
        /// <returns></returns>
        public async Task<List<ShippingPartnerDto>> GetFull()
        {
	        var partners = await Repository.GetAllListAsync(); 
	        return ObjectMapper.Map<List<ShippingPartnerDto>>(partners);
        }
		public async Task<ListResultDto<ShippingPartnerDto>> GetAllShippingPartnersAsync()
		{
			var partners = await Repository.GetAllListAsync();

			return new ListResultDto<ShippingPartnerDto>(
				ObjectMapper.Map<List<ShippingPartnerDto>>(partners)
			);
		}

        /// <summary>
        /// Lấy danh sách đối tác vận chuyển theo loại
        /// </summary>
        /// <param name="type">
        /// -1: Lấy tất cả
        /// 1: Đối tác vận chuyển nội địa
        /// 0. Đối tác vận chuyển quốc tế
        /// </param>
        /// <returns></returns>
        public async Task<List<ShippingPartnerDto>> GetAllShippingPartnersByLocationAsync(int type)
        {
            // get curent user login
            var partners = (await Repository.GetAllListAsync()).Where(x => x.Status);
            if (type != -1)
            {
                partners = partners.Where(x => x.Type == type);
            }
            return ObjectMapper.Map<List<ShippingPartnerDto>>(partners);
        }
    }
}
