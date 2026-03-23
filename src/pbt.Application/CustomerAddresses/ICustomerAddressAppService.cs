using Abp.Application.Services.Dto;
using Abp.Application.Services;
using pbt.CustomerAddresss.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using pbt.Entities;
using System.IO;

namespace pbt.CustomerAddresss
{
    public interface ICustomerAddressAppService : IAsyncCrudAppService<CustomerAddressDto, long, PagedAndSortedResultRequestDto, CustomerAddressDto, CustomerAddressDto>
    {
        Task<List<CustomerAddressDto>> GetByCustomerId(long customerId);

        Task<int> UpdateDefault(long customerId);

        Task<PagedResultDto<CustomerAddressDto>> GetByCurrentCustomer(PagedAndSortedResultRequestDto input);
    }
}
