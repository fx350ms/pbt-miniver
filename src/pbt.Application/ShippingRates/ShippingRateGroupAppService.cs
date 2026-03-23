using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Microsoft.AspNetCore.Http;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using pbt.Entities;
using pbt.ShippingRates.Dto;
using System.Collections.Generic;
using System.IO;
using System;
using System.Threading.Tasks;
using System.Linq.Dynamic.Core;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.SqlClient;
using pbt.ApplicationUtils;
using pbt.ChangeLogger;
using Abp.EntityFrameworkCore.Repositories;

namespace pbt.ShippingRates
{
    public class ShippingRateGroupAppService : AsyncCrudAppService<ShippingRateGroup, ShippingRateGroupDto, long, PagedAndSortedResultRequestDto, ShippingRateGroupDto, ShippingRateGroupDto>, IShippingRateGroupAppService
    {
        public readonly IShippingRateTierAppService _shippingRateTierAppService;
        public readonly IRepository<ShippingRate, long> _shippingRateRepository;
        public readonly IRepository<ShippingRateCustomer, long> _shippingRateCustomer;

        public ShippingRateGroupAppService(IRepository<ShippingRateGroup, long> repository,
            IShippingRateTierAppService shippingRateTierAppService,
            IRepository<ShippingRate, long> shippingRateRepository,
            IRepository<ShippingRateCustomer, long> shippingRateCustomer)
            : base(repository)
        {
            _shippingRateTierAppService = shippingRateTierAppService;
            _shippingRateRepository = shippingRateRepository;
            _shippingRateCustomer = shippingRateCustomer;
        }

        public async Task<PagedResultDto<ShippingRateGroupDto>> GetDataAsync(PagedAndSortedShipingGroupResultRequestDto input)
        {
            //input.Sorting = "IsDefaultForCustomer,Id";
            //return base.GetAllAsync(input); 
            var query = await (Repository.GetAllAsync());
            if (!string.IsNullOrEmpty(input.Keyword))
            {
                query = query.Where(x => x.Name.Contains(input.Keyword));
            }


            var totalCount = query.Count();
            var items = query.OrderByDescending(x => x.IsDefaultForCustomer)
                .ThenByDescending(x => x.Id)
                .PageBy(input).ToList();
            var result = new PagedResultDto<ShippingRateGroupDto>
            {
                TotalCount = totalCount,
                Items = ObjectMapper.Map<List<ShippingRateGroupDto>>(items)
            };

            return result;

        }

        public async Task<ShippingRateGroupDto> CreateWithResetDefaultAsync(ShippingRateGroupDto input)
        {
            input.IsActived = true;
            if (input.IsDefaultForCustomer)
            {
                await ResetDefaultForCustomer();
            }
            return await base.CreateAsync(input);
        }


        private async Task ResetDefaultForCustomer()
        {
            var shippingRateGroupsQuery = await Repository.GetAllAsync();
            shippingRateGroupsQuery = shippingRateGroupsQuery.Where(x => x.IsDefaultForCustomer);
            var shippingRateGroups = await shippingRateGroupsQuery.ToListAsync();
            foreach (var group in shippingRateGroups)
            {
                group.IsDefaultForCustomer = false;
                await Repository.UpdateAsync(group);
            }
        }


        public async Task SetAsDefault(long id)
        {
            await ResetDefaultForCustomer();
            var shippingRateGroup = await Repository.GetAsync(id);
            if (shippingRateGroup != null)
            {
                shippingRateGroup.IsDefaultForCustomer = true;
                await Repository.UpdateAsync(shippingRateGroup);
            }
        }

        public override async Task DeleteAsync(EntityDto<long> input)
        {
            // Kiểm tra xem ShippingRateGroup có tồn tại không
            var shippingRateGroup = Repository.GetAll().FirstOrDefault(x => x.Id == input.Id);
            if (shippingRateGroup == null)
            {
                throw new Exception("ShippingRateGroup not found");
            }

            try
            {
                var excuteResult = await Repository.GetDbContext().Database.ExecuteSqlRawAsync(
                  "EXEC SP_ShippingRateGroups_Delete @id",
                    new SqlParameter("@id", input.Id)
                );
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException($"Error: {ex.Message}");
            }


        }


        public async Task<List<ShippingRateGroupDto>> GetAllListAsync()
        {
            var query = await Repository.GetAllAsync();
            var items = query.OrderByDescending(x => x.IsDefaultForCustomer)
                .ThenByDescending(x => x.Id).ToList();
            return ObjectMapper.Map<List<ShippingRateGroupDto>>(items);
        }


        public async Task<IActionResult> SaveShippingRates(SaveShippingRateDto input)
        {
            try
            {
                foreach (var rate in input.ShippingRates)
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
                        await _shippingRateTierAppService.UpdateTierList(rate.Id, rate.Tiers);

                    }
                    else
                    {
                        // Tạo mới ShippingRate nếu chưa tồn tại
                        var newRate = ObjectMapper.Map<ShippingRate>(rate);
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


        public async Task<ShippingRateGroupDto> GetByCustomerIdAsync(long customerId)
        {

            var shippingRateGroupCustomer = await (await _shippingRateCustomer.GetAllAsync())
                .FirstOrDefaultAsync(u => u.CustomerId == customerId);

            // Lấy ShippingRateGroup mặc định cho khách hàng
            var shippingRateGroup = await (await Repository.GetAllAsync())
                .Where(x =>
                shippingRateGroupCustomer != null
                && shippingRateGroupCustomer.ShippingRateGroupId == x.Id
                && x.IsActived)

                .FirstOrDefaultAsync();


            if (shippingRateGroup == null)
            {
                shippingRateGroup = await (await Repository.GetAllAsync())
               .Where(x => x.IsDefaultForCustomer && x.IsActived).FirstOrDefaultAsync();
            }

            if (shippingRateGroup == null)
            {
                throw new UserFriendlyException("Không tìm thấy bảng giá vận chuyển mặc định cho khách hàng này.");
            }

            return ObjectMapper.Map<ShippingRateGroupDto>(shippingRateGroup);
        }

        public async Task AddCustomerAsync(long id, long customerId)
        {
            var group = await Repository.GetAsync(id);
            if (group == null)
            {
                throw new UserFriendlyException("Shipping rate group not found.");
            }

            var customerGroup = await _shippingRateCustomer.FirstOrDefaultAsync(x => x.CustomerId == customerId);
            if (customerGroup != null)
            {
                await _shippingRateCustomer.DeleteAsync(customerGroup);
            }

            var shippingRateCustomer = new ShippingRateCustomer
            {
                ShippingRateGroupId = id,
                CustomerId = customerId
            };

            try
            {
                await _shippingRateCustomer.InsertAsync(shippingRateCustomer);
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException($"Error adding customer to shipping rate group: {ex.Message}");
            }
        }
    }
}