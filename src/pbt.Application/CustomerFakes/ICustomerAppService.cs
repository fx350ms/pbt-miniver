using Abp.Application.Services.Dto;
using Abp.Application.Services;
using pbt.CustomerFakes.Dto;
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

namespace pbt.CustomerFakes
{
    public interface ICustomerFakeAppService : IAsyncCrudAppService<CustomerFakeDto, long, PagedAndSortedResultRequestDto, CreateUpdateCustomerFakeDto, CustomerFakeDto>
    {

        Task ImportCustomerFakesAsync(IFormFile file);

        Task<byte[]> ExportCustomerFakesToExcelAsync();
    }
}
