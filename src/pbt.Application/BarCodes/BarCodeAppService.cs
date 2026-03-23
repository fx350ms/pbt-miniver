using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.EntityFrameworkCore.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NPOI.HSSF.Record.Chart;
using Org.BouncyCastle.Crypto;
using pbt.ApplicationUtils;
using pbt.Authorization;
using pbt.BarCodes.Dto;
using pbt.Core;
using pbt.Customers.Dto;
using pbt.Entities;
using pbt.Orders.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pbt.BarCodes
{

    public class BarCodeAppService : AsyncCrudAppService<BarCode, BarCodeDto, long, PagedResultRequestDto, BarCodeDto, BarCodeDto>, IBarCodeAppService
    {
        private pbtAppSession _pbtAppSession;
        private IRepository<Warehouse, int> _warehouseRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string[] _roles;
        public BarCodeAppService(
            IRepository<BarCode, long> repository,
            IRepository<Warehouse, int> warehouseRepository,
            pbtAppSession pbtAppSession,
            IHttpContextAccessor httpContextAccessor
            )
            : base(repository)
        {
            _pbtAppSession = pbtAppSession;
            _warehouseRepository = warehouseRepository;
            _httpContextAccessor = httpContextAccessor;
            _roles = _httpContextAccessor.HttpContext.User.Claims
                .Where(c => c.Type == System.Security.Claims.ClaimTypes.Role) // Lọc claims loại "role"
                .Select(c => c.Value)
                .ToArray();
        }

        [Authorize]
        public override Task<BarCodeDto> CreateAsync(BarCodeDto input)
        {
            try
            {
                input.CreatorUserName = _pbtAppSession.UserName;
                return base.CreateAsync(input);
            }
            catch (System.Exception ex)
            {

            }
            return null;
        }


        public override async Task<PagedResultDto<BarCodeDto>> GetAllAsync(PagedResultRequestDto input)
        {
            var currentUserId = AbpSession.UserId;
            var query = base.CreateFilteredQuery(input);
            if (!PermissionChecker.IsGranted(PermissionNames.Function_ScanCodeViewAll))
            {
            }
            else
            {
                query = query.Where(x => x.CreatorUserId == currentUserId);
            }
            
            // get current user warehouse id
            var warehouseId = _pbtAppSession.WarehouseId;
            if(_roles.Contains(RoleConstants.warehouseVN) && !_roles.Contains(RoleConstants.admin))
            {
            }

            var count = query.Count();
            query = ApplySorting(query, input);
            query = ApplyPaging(query, input);
            var data = ObjectMapper.Map<List<BarCodeDto>>(query.ToList());

            return new PagedResultDto<BarCodeDto>()
            {
                Items = data,
                TotalCount = count
            };
        }

        public  async Task<PagedResultDto<BarCodeDto>> GetAllOnCreateViewAsync(PagedBarCodeOnCreateViewRequestDto input)
        {
            var currentUserId = AbpSession.UserId;
            var query = base.CreateFilteredQuery(input);
            if (!PermissionChecker.IsGranted(PermissionNames.Function_ScanCodeViewAll) || input.OnlyMyCode)
            {
                query = query.Where(x => x.CreatorUserId == currentUserId);
            }


            var count = query.Count();
            query = ApplySorting(query, input);
            query = ApplyPaging(query, input);
            var data = ObjectMapper.Map<List<BarCodeDto>>(query.ToList());

            return new PagedResultDto<BarCodeDto>()
            {
                Items = data,
                TotalCount = count
            };
        }


        public async Task<PagedResultDto<BarCodeDto>> GetDataAsync(PagedBarCodeResultRequestDto input)
        {
            var query = (await Repository.GetAllAsync()).IgnoreQueryFilters();

            // Filter by ActionType (Action)
            if (input.ActionType.HasValue && input.ActionType > 0)
            {
                query = query.Where(x => x.Action == input.ActionType.Value);
            }

            // Filter by CodeType
            if (input.CodeType.HasValue && input.CodeType > 0)
            {
                query = query.Where(x => x.CodeType == input.CodeType.Value);
            }

            // Filter by Warehouse (SourceWarehouseId or DestinationWarehouseId)
            if (input.Warehouse.HasValue && input.Warehouse > 0)
            {
                query = query.Where(x => x.SourceWarehouseId == input.Warehouse.Value || x.DestinationWarehouseId == input.Warehouse.Value);
            }

            // Filter by UserId (CreatorUserId)
            if (input.UserId.HasValue && input.UserId > 0)
            {
                query = query.Where(x => x.CreatorUserId == input.UserId.Value);
            }

            // Filter by Keyword (ScanCode, PackageCode, Content)
            if (!string.IsNullOrWhiteSpace(input.Keyword))
            {
                var keyword = input.Keyword.Trim();
                query = query.Where(x =>
                    x.ScanCode.Contains(keyword)   
                );
            }

            // Filter by Create Date
            if (input.StartCreateDate.HasValue)
            {
                query = query.Where(x => x.CreationTime >= input.StartCreateDate.Value);
            }
            if (input.EndCreateDate.HasValue)
            {
                // Add 1 day to include the end date fully
                //var endDate = input.EndCreateDate.Value.AddDays(1);
                query = query.Where(x => x.CreationTime <= input.EndCreateDate.Value);
            }
            var totalCount = await query.CountAsync();
            var entities = await query
                .OrderByDescending(x => x.Id) // Sort by CreationTime descending
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToListAsync();
            var dtos = ObjectMapper.Map<List<BarCodeDto>>(entities);
            // set SourceWarehouseName
            foreach (var dto in dtos.Where(dto => dto.SourceWarehouseId.HasValue))
            {
                var sourceWarehouse = await _warehouseRepository.FirstOrDefaultAsync(x => x.Id == dto.SourceWarehouseId.Value);
                dto.SourceWarehouseName = sourceWarehouse?.Name;
            }
            return new PagedResultDto<BarCodeDto>
            {
                Items = dtos,
                TotalCount = totalCount
            };
        }

        public async Task<int> DeleteByIdsAsync(List<long> ids)
        {
            try
            {
                var result = await Repository.GetDbContext().Database.ExecuteSqlRawAsync(
                   "EXEC SP_BarCodes_DeleteByIds @Ids",

                   new SqlParameter("@Ids", string.Join(',', ids))
               );
                return result;
            }
            catch (Exception ex)
            {
                return -1;
            }
        }


        [Authorize]
        public async Task<JsonResult> DeleteAllByCurrentUser()
        {
            try
            {
                var result = await ConnectDb.ExecuteNonQueryAsync( "SP_BarCodes_DeleteByCreatorUserId" , System.Data.CommandType.StoredProcedure,
                    new[]
                    {
                        new SqlParameter("@CreatorUserId", AbpSession.UserId.HasValue ? AbpSession.UserId.Value : 0)
                    } );
                return new JsonResult( new
                {
                    Success = result >= 0,
                    Message = result >= 0 ? $"Xóa thành công {result} bản ghi" : "Xóa thất bại"
                } );
            }
            catch (Exception ex)
            {
                throw;
            }
        }


    }
}
