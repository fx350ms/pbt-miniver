using Abp.Application.Services;
using Abp.Application.Services.Dto;
using pbt.Entities;
using pbt.OrderNumbers.Dto;
using System;
using System.Threading.Tasks;

namespace pbt.OrderNumbers
{
    public interface IIdentityCodeAppService : IAsyncCrudAppService<IdentityCodeDto, long, PagedResultRequestDto, IdentityCodeDto, IdentityCodeDto>
    {
        public Task<IdentityCodeDto> GenerateNewSequentialNumberAsync(string prefix);
        public Task<IdentityCodeDto> GenerateNewSequentialNumberAsync(string prefix, DateTime date);
    }
}