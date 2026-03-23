using Abp.Application.Services.Dto;
using Abp.Application.Services;
using pbt.Customers.Dto;
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
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using pbt.Commons.Dto;
using pbt.Users.Dto;
using ChangePasswordDto = pbt.Customers.Dto.ChangePasswordDto;

namespace pbt.Customers
{
    /// <summary>
    /// Interface for Customer Application Service, providing methods for managing customer data.
    /// </summary>
    public interface ICustomerAppService : IAsyncCrudAppService<CustomerDto, long, CustomerListRequestDto, CreateUpdateCustomerDto, CustomerDto>
    {
        /// <summary>
        /// Imports customers from an uploaded file.
        /// </summary>
        /// <param name="file">The file containing customer data.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task ImportCustomersAsync(IFormFile file);

        /// <summary>
        /// Exports the list of customers to an Excel file.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation, containing the Excel file as a byte array.</returns>
        Task<byte[]> ExportCustomersToExcelAsync();

        /// <summary>
        /// Retrieves customer information by user ID.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>A task representing the asynchronous operation, containing the customer data.</returns>
        Task<CustomerDto> GetByUserId(long userId);

        /// <summary>
        /// Assigns a customer to a sales representative.
        /// </summary>
        /// <param name="data">The data containing assignment information.</param>
        /// <returns>A task representing the asynchronous operation, returning the result of the assignment.</returns>
        Task<int> AssignToSale(CustomerAssignToSaleDto data);

        /// <summary>
        /// Links a customer to a user.
        /// </summary>
        /// <param name="data">The data containing linking information.</param>
        /// <returns>A task representing the asynchronous operation, returning the result of the linking.</returns>
        Task<int> LinkToUser(LinkToUserDto data);

        /// <summary>
        /// Retrieves a list of child customers based on the parent customer ID.
        /// </summary>
        /// <param name="parentId">The ID of the parent customer.</param>
        /// <returns>A task representing the asynchronous operation, containing a list of child customers.</returns>
        Task<List<CustomerDto>> GetChildren(long parentId);

        /// <summary>
        /// Retrieves a list of child customers for a select box based on a search query.
        /// </summary>
        /// <param name="q">The search query.</param>
        /// <returns>A task representing the asynchronous operation, containing a list of customers for the select box.</returns>
        Task<List<CustomerForSelectBoxDto>> GetChildrenForSelectBox(string q);

        /// <summary>
        /// Retrieves a list of customers associated with a specific sales representative ID.
        /// </summary>
        /// <param name="saleId">The ID of the sales representative.</param>
        /// <returns>A task representing the asynchronous operation, containing a list of customers.</returns>
        Task<List<CustomerDto>> GetBySale(long saleId);

        /// <summary>
        /// Retrieves a paged list of customers associated with the current sales representative.
        /// </summary>
        /// <param name="input">The pagination and filtering input.</param>
        /// <returns>A task representing the asynchronous operation, containing a paged result of customers.</returns>
        Task<PagedResultDto<CustomerDto>> GetByCurrentSale(PagedResultRequestDto input);

        /// <summary>
        /// Retrieves the full list of customers.
        /// </summary>
        /// <returns>A task representing the asynchronous operation, containing a list of all customers.</returns>
        Task<List<CustomerDto>> GetFull();

        public Task<PagedResultDto<CustomerDto>> GetFullFilter([FromQuery] string filter,
            [FromQuery] int maxResultCount = 10, [FromQuery] int skipCount = 0);


        /// <summary>
        /// Retrieves customer information by customer ID.
        /// </summary>
        /// <param name="customerId">The ID of the customer.</param>
        /// <returns>A task representing the asynchronous operation, containing the customer data.</returns>
        Task<CustomerDto> GetByCustomerIdAsync(long customerId);

        /// <summary>
        /// Searches for customers based on a query string.
        /// </summary>
        /// <param name="keyword">The search query.</param>
        /// <returns>A task representing the asynchronous operation, containing a list of matching customers.</returns>
        Task<List<CustomerDto>> SearchCustomersAsync(string keyword);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="customerDto"></param>
        /// <returns></returns>
        Task<CustomerDto> CreateCustomerByRegistration(CreateUpdateCustomerDto customerDto);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="PhoneNumber"></param>
        /// <returns></returns>
        Task<CustomerDto> GetByPhoneNumber(string PhoneNumber);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        Task<CustomerDto> GetByUsernameOrPhone(string keyword);


        /// <summary>
        /// Lấy danh sách khách hàng theo nhân viên kinh doanh
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        Task<List<OptionItemDto>> GetCustomerListByCurrentSaleForSelect(string q);

        /// <summary>
        /// Lấy danh sách khách hàng theo nhân viên kinh doanh hoặc khách hàng cha
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        Task<List<OptionItemDto>> GetCustomerBySaleOrParentForSelectAsync(string q);


        /// <summary>
        /// Lấy tất cả khách hàng nếu người đăng nhập là admin, sale, sale admin
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        Task<List<OptionItemDto>> GetAllForSelectBySaleAsync(string q);


        public Task<List<OptionItemDto>> GetAllForSelectAsync(string q = "");

        public Task<bool> UpdatePasswordAsync(ChangePasswordDto input);

        public Task AddCustomersByUsernameListAsync(List<string> usernames);
        public Task<CustomerDto> GetCustomerById(long id);

        public Task<List<CustomerDto>> GetCustomersByCurrentUserAsync();

        Task<List<UserDto>> GetUserSale();

        /// <summary>
        /// Lấy thông tin khách hàng theo tên đăng nhập với cache
        /// </summary>
        /// <param name="customerName"></param>
        /// <returns></returns>
        public Task<CustomerDto> GetCustomerByUserNameWithCacheAsync(string customerName);

        public Task<decimal> GetInsurancePercentage(long customerId);
        /// <summary>
        /// Xóa ParentId của khách hàng
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public Task RemoveParentIdAsync(long customerId);

        public Task<PagedResultDto<CustomerDto>> GetAllChildAsync(CustomerListRequestDto input);
        public Task<List<CustomerDto>> GetAllChildren();
        Task<List<CustomerFinancialInfoDto>> GetCustomerListWithFinancialAsync();
        public Task<bool> LockCustomerAsync(long customerId);
        Task DeleteCustomerAsync(EntityDto<long> input);
        Task<CustomerDto> GetLoginCustomerAsync();


        Task<List<CustomerDto>> GetCustomersByCurrentUserForSelectOrderViewAsync(string query);
        Task<List<CustomerDto>> GetCustomerListByCurrentUserWarehouseByAsync();
        Task<List<CustomerWithWarehouseDto>> GetAllCustomerWithWarehouses();
    }
}
