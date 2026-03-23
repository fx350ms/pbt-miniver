using System;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using pbt.Entities;
using pbt.ShippingCosts.Dto;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pbt.ShippingRates.Dto;

namespace pbt.ShippingCosts
{
    public class ShippingCostGroupAppService : AsyncCrudAppService<
        ShippingCostGroup, // Entity
        ShippingCostGroupDto, // DTO để trả về
        long, // Kiểu dữ liệu của khóa chính
        PagedResultRequestDto, // DTO để phân trang
        ShippingCostGroupDto, // DTO để tạo mới
        ShippingCostGroupDto>, // DTO để cập nhật
        IShippingCostGroupAppService
    {
        private readonly IRepository<ShippingCostBase, long> _shippingRateRepository;
        private IShippingCostTierAppService _shippingCostTierAppService;
        public ShippingCostGroupAppService(
            IRepository<ShippingCostGroup, long> repository,
            IRepository<ShippingCostBase, long> shippingRateRepository,
            IShippingCostTierAppService shippingCostTierAppService)
            : base(repository)
        {
            _shippingRateRepository = shippingRateRepository;
            _shippingCostTierAppService = shippingCostTierAppService;
        }
        
        // public override async Task DeleteAsync(EntityDto<long> input)
        // {
        //     // Kiểm tra xem ShippingRateGroup có tồn tại không
        //     var shippingRateGroup = Repository.GetAll().FirstOrDefault(x => x.Id == input.Id);
        //     if (shippingRateGroup == null)
        //     {
        //         throw new Exception("ShippingRateGroup not found");
        //     }
        //
        //     try
        //     {
        //         var excuteResult = await Repository.GetDbContext().Database.ExecuteSqlRawAsync(
        //             "EXEC SP_ShippingRateGroups_Delete @id",
        //             new SqlParameter("@id", input.Id)
        //         );
        //     }
        //     catch (Exception ex)
        //     {
        //         throw new UserFriendlyException($"Error: {ex.Message}");
        //     }
        // }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<IActionResult> saveShippingCosts(SaveShippingCostDto input)
        {
            try
            {
                foreach (var rate in input.ShippingCosts)
                {
                    // Kiểm tra nếu ShippingRate đã tồn tại
                    if (rate.Id > 0)
                    {
                        // Update existing ShippingRate
                        var existingRate = await _shippingRateRepository.GetAllIncluding(r => r.Tiers)
                            .FirstOrDefaultAsync(r => r.Id == rate.Id);
                        if (existingRate == null)
                        {
                            return new JsonResult(new { success = false, message = "Shipping rate not found." });
                        }
                        await _shippingCostTierAppService.UpdateTierList(rate.Id, rate.Tiers);
                    }
                    else
                    {
                        // Tạo mới ShippingRate nếu chưa tồn tại
                        var newRate = ObjectMapper.Map<ShippingCostBase>(rate);
                        await _shippingRateRepository.InsertAsync(newRate);
                    }
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = "Cập nhật không thành công" });
            }

            return new JsonResult(new { success = true, message = "Cập nhật thành công" });

        }

        public override Task<ShippingCostGroupDto> CreateAsync(ShippingCostGroupDto input)
        {
            try
            {
                var result =  base.CreateAsync(input);

                return result;
            }
            catch (System.Exception ex)
            {

                throw ex;
            }
        }
        // update ShippingCostGroup
        public override Task<ShippingCostGroupDto> UpdateAsync(ShippingCostGroupDto input)
        {
            try
            {
                var result =  base.UpdateAsync(input);

                return result;
            }
            catch (System.Exception ex)
            {

                throw ex;
            }
        }
    }
}