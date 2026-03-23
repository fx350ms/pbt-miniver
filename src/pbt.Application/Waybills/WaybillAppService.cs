using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using pbt.Authorization;
using pbt.Waybills.Dto;
using pbt.Entities;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;
using Abp.Authorization;
using Abp.Auditing;
using Abp.UI;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.Formula.Functions;
using pbt.ApplicationUtils;

namespace pbt.Waybills
{
    [AbpAuthorize(PermissionNames.Pages_Waybill)]
    [Audited]
    public class WaybillAppService : AsyncCrudAppService<Waybill, WaybillDto, long, PagedResultRequestDto, WaybillDto, WaybillDto>, IWaybillAppService
    {
        private pbtAppSession _pbtAppSession;
        public WaybillAppService(IRepository<Waybill, long> repository,
            pbtAppSession pbtAppSession
            )
            : base(repository)
        {
            _pbtAppSession = pbtAppSession;
        }
        public override async Task<PagedResultDto<WaybillDto>> GetAllAsync(PagedResultRequestDto input)
        {
            try
            {
                var data = await base.GetAllAsync(input);
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
         
        }
        public async Task<List<string>> GetUnmatchedWaybillCodesAsync(string keyword)
        {
            try
            {
                var unmatchedWaybills = await Repository.GetAllListAsync();
                var x = unmatchedWaybills.ToList();
                var result = unmatchedWaybills.Where(
                    w =>
                    w.Status == 0 && (string.IsNullOrEmpty(keyword) || w.WaybillCode.ToUpper().Contains(keyword.ToUpper())));
                return result.Select(w => w.WaybillCode).ToList();
            }
            catch (System.Exception ex)
            {

                throw;
            }

        }
        
        [AllowAnonymous]
        [AbpAllowAnonymous]
        public async Task<WaybillDto> GetByCode(string code)
        {
            var data = await Repository.GetAllListAsync(x => x.WaybillCode.ToUpper() == code.ToUpper());
            return ObjectMapper.Map<WaybillDto>(data.FirstOrDefault());
        }

        public async Task<int> CreateListAsync(CreateWaybillListDto input)
        {
            var waybillCodes = input.WaybillCodes.Split(',');
            int createdCount = 0;
            foreach (var code in waybillCodes)
            {
                var waybill = new Waybill
                {
                    WaybillCode = code.Trim(),
                    Note = input.Note,
                    CreationTime = DateTime.Now,
                    Status = 0 // chờ khớp đơn
                };

                await Repository.InsertAsync(waybill);
                createdCount++;
            }

            await CurrentUnitOfWork.SaveChangesAsync();

            return createdCount;
        }

        public async Task<WaybillDto> CreateSimple(CreateWaybillSimpleDto input)
        {
            var dto = new WaybillDto()
            {
                WaybillCode = input.WaybillCode.Trim(),
                Status = 0 // chờ khớp đơn
            };
            return await base.CreateAsync(dto);
        }
         
    }
}
