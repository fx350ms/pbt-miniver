using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Auditing;
using Abp.Authorization;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.EntityFrameworkCore.Repositories;
using Abp.Runtime.Session;
using Abp.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NPOI.HSSF.UserModel;
using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using pbt.Application.Cache;
using pbt.ApplicationUtils;
using pbt.Authorization;
using pbt.Authorization.Roles;
using pbt.Authorization.Users;
using pbt.Bags.Dto;
using pbt.Commons.Dto;
using pbt.ConfigurationSettings;
using pbt.Core;
using pbt.Customers.Dto;
using pbt.Entities;
using pbt.Packages.Dto;
using pbt.Users.Dto;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

using ChangePasswordDto = pbt.Customers.Dto.ChangePasswordDto;

namespace pbt.Customers
{
    [Authorize]
    [Audited]
    public class CustomerAppService :
        AsyncCrudAppService<Customer, CustomerDto, long, CustomerListRequestDto, CreateUpdateCustomerDto, CustomerDto>,
        ICustomerAppService
    {
        private pbtAppSession _pbtAppSession;
        private readonly UserRegistrationManager _userRegistrationManager;
        private readonly UserManager _userManager;
        private readonly RoleManager _roleManager;
        private readonly IRepository<Role> _roleRepository;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IAbpSession _abpSession;
        private readonly LogInManager _logInManager;
        private readonly CookieService _cookieService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ConfigAppCacheService _cacheService;
        private readonly string[] _roles;
        private readonly IConfigurationSettingAppService _configurationSettingAppService;
        private readonly IRepository<ShippingRateGroup, long> _shippingRateGroupRepository;
        private readonly IRepository<ShippingRateCustomer, long> _shippingRateCustomerRepository;
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<Warehouse> _warehouseRepository;
        private readonly IRepository<CustomerTransaction, long> _customerTransactionRepository;
        private readonly IRepository<Package> _packageRepository;
        private readonly IRepository<Bag> _bagRepository;

        private const string CustomerRoleName = "customer";

        public CustomerAppService(IRepository<Customer, long> repository,
            pbtAppSession pbtAppSession,
            UserManager userManager,
            RoleManager roleManager,
            IRepository<Role> roleRepository,
            IPasswordHasher<User> passwordHasher,
            IAbpSession abpSession,
            LogInManager logInManager,
            UserRegistrationManager userRegistrationManager,
            IHttpContextAccessor httpContextAccessor,
            CookieService cookieService,
            ConfigAppCacheService cacheService,
            IConfigurationSettingAppService configurationSettingAppService,
            IRepository<User, long> userRepository,
            IRepository<ShippingRateGroup, long> shippingRateGroupRepository,
            IRepository<ShippingRateCustomer, long> shippingRateCustomerRepository,
            IRepository<Warehouse> wareRepository,
            IRepository<CustomerTransaction, long> customerTransactionRepository,
            IRepository<Package> packageRepository,
            IRepository<Bag> bagRepository

            )
            : base(repository)
        {
            _pbtAppSession = pbtAppSession;

            _httpContextAccessor = httpContextAccessor;
            _abpSession = abpSession;
            _userManager = userManager;
            _roleManager = roleManager;
            _logInManager = logInManager;
            _passwordHasher = passwordHasher;
            _roleRepository = roleRepository;
            _userRegistrationManager = userRegistrationManager;
            _roles = _httpContextAccessor.HttpContext.User.Claims
                .Where(c => c.Type == System.Security.Claims.ClaimTypes.Role) // Lọc claims loại "role"
                .Select(c => c.Value)
                .ToArray();
            _cookieService = cookieService;
            _cacheService = cacheService;
            _configurationSettingAppService = configurationSettingAppService;
            _shippingRateGroupRepository = shippingRateGroupRepository;
            _shippingRateCustomerRepository = shippingRateCustomerRepository;
            _userRepository = userRepository;
            _warehouseRepository = wareRepository;
            _customerTransactionRepository = customerTransactionRepository;
            _packageRepository = packageRepository;
            _bagRepository = bagRepository;
        }


        public async Task<PagedResultDto<CustomerDto>> GetAllWithSaleInfoAsync(CustomerListRequestDto input)
        {
            try
            {
                var data = new List<CustomerDto>();
                var count = 0;

                var checkPermissionResult = GetPermissionCheckerWithCustomerIds();

                if (checkPermissionResult.PermissionCase < 0)
                {
                    return new PagedResultDto<CustomerDto>()
                    {
                        Items = data,
                        TotalCount = count
                    };
                }

                var permissionCase = checkPermissionResult.PermissionCase;
                var customerIds = checkPermissionResult.CustomerIds;
                var query = (await Repository.GetAllAsync()).Where(x =>
                    permissionCase == 1 ||
                   ((permissionCase == 2 || permissionCase == 3 || permissionCase == 4) && customerIds.Contains(x.Id))
                    );

                if (input.WarehouseId.HasValue && input.WarehouseId > 0)
                {
                    query = query.Where(x => x.WarehouseId == input.WarehouseId);
                }

                if (input.SaleId.HasValue && input.SaleId > 0)
                {
                    query = query.Where(x => x.SaleId == input.SaleId);
                }

                if (!string.IsNullOrEmpty(input.Keyword))
                {
                    var keyword = input.Keyword.ToLower();
                    query = query.Where(u =>
                        u.Username.ToLower().Contains(keyword)
                        || u.PhoneNumber.ToLower().Contains(keyword)
                        || u.Email.ToLower().Contains(keyword)
                    );
                }
                if (input.CustomerType > 0)
                {
                    query = query.Where(u =>
                    (u.IsAgent && input.CustomerType == (int)CustomerType.Agent) ||
                    (!u.IsAgent && input.CustomerType == (int)CustomerType.Individual));
                }

                /// Chưa được gán cho SALE
                if (input.SaleType > 0)
                {
                    query = query.Where(u => u.SaleId == 0);
                }

                count = query.Count();
                query = ApplySorting(query, input);
                query = ApplyPaging(query, input);
                // Nếu có CustomerId => Khách hàng đại lý thì lấy ra danh sách khách hàng của mình

                data = ObjectMapper.Map<List<CustomerDto>>(query.ToList());
                var _saleIds = data.Where(x => x.SaleId > 0).Select(y => y.SaleId).Distinct().ToList();
                var _warehouseIds = data.Where(x => x.WarehouseId.HasValue && x.WarehouseId > 0).Select(y => y.WarehouseId.Value).Distinct().ToList();
                var saleUsers = (await _userRepository.GetAllAsync()).Where(u => _saleIds.Contains(u.Id));
                var saleUserDict = saleUsers.ToDictionary(u => u.Id, u => u.UserName);
                var warehouses = (await _warehouseRepository.GetAllAsync()).Where(u => _warehouseIds.Contains(u.Id));
                var warehouseDict = warehouses.ToDictionary(u => u.Id, u => u.Name);
                var users = data.Where(x => x.UserId.HasValue && x.UserId > 0).Select(y => y.UserId.Value).Distinct().ToList();
                var _userLst = (await _userRepository.GetAllAsync()).Where(u => users.Contains(u.Id)).ToList();
                var customerParentIds = data.Where(x => x.ParentId > 0).Select(y => y.ParentId).Distinct().ToList();
                var customerParents = (await Repository.GetAllAsync()).Where(u => customerParentIds.Contains(u.Id)).ToDictionary(u => u.Id, u => u.Username);


                foreach (var item in data)
                {
                    if (item.SaleId > 0)
                        item.SaleUsername = saleUserDict[item.SaleId] ?? "";
                    if (item.WarehouseId.HasValue && item.WarehouseId > 0)
                        item.WarehouseName = warehouseDict.ContainsKey(item.WarehouseId.Value) ? warehouseDict[item.WarehouseId.Value] : "";
                    if (item.UserId > 0)
                    {
                        var user = _userLst.FirstOrDefault(x => x.Id == item.UserId);
                        if (user != null)
                        {
                            item.User = ObjectMapper.Map<UserDto>(user);
                        }
                    }
                    if (item.ParentId > 0 && customerParents != null && customerParents.Count > 0)
                    {
                        item.ParentUsername = customerParents.ContainsKey(item.ParentId) ? customerParents[item.ParentId] : "";
                    }
                }

                return new PagedResultDto<CustomerDto>()
                {
                    Items = data,
                    TotalCount = count
                };
            }
            catch (Exception ex)
            {
                Logger.Error("[GetAllWithSaleInfoAsync] Lỗi lấy danh sách khách hàng", ex);
                throw;
            }
        }

        public async Task<List<UserDto>> GetUserSale()
        {
            var roles = (await _roleRepository.GetAllAsync())
                .Where(c => c.NormalizedName.ToUpper() == RoleConstants.saleadmin.ToUpper() || c.NormalizedName.ToUpper() == RoleConstants.sale.ToUpper())
                .Select(c => c.Id).ToList();
            var query = (await _userRepository.GetAllAsync()).AsQueryable();
            query = query.Include(i => i.Roles);
            query = query.Where(c => c.Roles.Any(r => roles.Contains(r.RoleId)));
            var users = await query.ToListAsync();
            return ObjectMapper.Map<List<UserDto>>(users);
        }

        public async Task<List<CustomerWithWarehouseDto>> GetAllCustomerWithWarehouses()
        {
            var data = await ConnectDb.GetListAsync<CustomerWithWarehouseDto>(
                "SP_Customers_GetAllWithWarehouse",
                commandType: CommandType.StoredProcedure
            );
            return data;
        }

        public async Task<PagedResultDto<CustomerDto>> GetAllChildAsync(CustomerListRequestDto input)
        {
            try
            {
                if (input.ParentId.HasValue && input.ParentId > 0)
                {
                    var query = await Repository.GetAllAsync();
                    query = query.Where(u => u.ParentId == input.ParentId);
                    if (!string.IsNullOrEmpty(input.Keyword))
                    {
                        var keyword = input.Keyword.ToLower();
                        query = query.Where(u =>
                            u.FullName.ToLower().Contains(keyword)
                            || u.PhoneNumber.ToLower().Contains(keyword)
                            || u.Email.ToLower().Contains(keyword)
                        );
                    }

                    query = query.Include(x => x.User);
                    var count = query.Count();
                    query = ApplySorting(query, input);
                    query = ApplyPaging(query, input);
                    return new PagedResultDto<CustomerDto>()
                    {
                        Items = query.ToList().MapTo<List<CustomerDto>>(),
                        TotalCount = count
                    };
                }
                
                return null;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task ImportCustomersAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File không hợp lệ.");

            var customers = new List<Customer>();

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                stream.Position = 0;

                IWorkbook workbook;
                if (Path.GetExtension(file.FileName).Equals(".xls"))
                {
                    workbook = new HSSFWorkbook(stream); // Định dạng .xls
                }
                else if (Path.GetExtension(file.FileName).Equals(".xlsx"))
                {
                    workbook = new XSSFWorkbook(stream); // Định dạng .xlsx
                }
                else
                {
                    throw new ArgumentException("Định dạng tệp không được hỗ trợ.");
                }

                var sheet = workbook.GetSheetAt(0); // Lấy sheet đầu tiên
                for (int i = 1; i <= sheet.LastRowNum; i++) // Bỏ qua hàng tiêu đề
                {
                    var row = sheet.GetRow(i);
                    if (row == null) continue;

                    var customer = new Customer
                    {
                        FullName = row.GetCell(1)?.ToString(),
                        Email = row.GetCell(2)?.ToString(),
                        PhoneNumber = row.GetCell(3)?.ToString(),
                        Address = row.GetCell(4)?.ToString(),
                        AddressReceipt = row.GetCell(5)?.ToString(),
                        DateOfBirth = row.GetCell(6)?.DateCellValue,
                        Gender = row.GetCell(7)?.ToString(),
                        RegistrationDate = DateTime.Now
                    };

                    customers.Add(customer);
                }
            }

            foreach (var customer in customers)
            {
                await Repository.InsertAsync(customer);
            }
        }

        public async Task<byte[]> ExportCustomersToExcelAsync()
        {
            // Lấy danh sách khách hàng từ database
            var customers = await Repository.GetAllListAsync();

            // Tạo workbook và sheet
            IWorkbook workbook = new XSSFWorkbook();
            ISheet sheet = workbook.CreateSheet("Customers");

            // Tạo hàng tiêu đề (header)
            var headerRow = sheet.CreateRow(0);
            headerRow.CreateCell(0).SetCellValue("Id");
            headerRow.CreateCell(1).SetCellValue("Full Name");
            headerRow.CreateCell(2).SetCellValue("Email");
            headerRow.CreateCell(3).SetCellValue("Phone Number");
            headerRow.CreateCell(4).SetCellValue("Address");
            headerRow.CreateCell(5).SetCellValue("Date of Birth");
            headerRow.CreateCell(6).SetCellValue("Gender");
            headerRow.CreateCell(7).SetCellValue("Status");

            // Thêm dữ liệu khách hàng vào sheet
            for (int i = 0; i < customers.Count; i++)
            {
                var customer = customers[i];
                var row = sheet.CreateRow(i + 1);
                row.CreateCell(0).SetCellValue(customer.Id);
                row.CreateCell(1).SetCellValue(customer.FullName ?? "");
                row.CreateCell(2).SetCellValue(customer.Email ?? "");
                row.CreateCell(3).SetCellValue(customer.PhoneNumber ?? "");
                row.CreateCell(4).SetCellValue(customer.Address ?? "");
                row.CreateCell(5).SetCellValue(customer.DateOfBirth?.ToString("yyyy-MM-dd") ?? "");
                row.CreateCell(6).SetCellValue(customer.Gender ?? "");
                row.CreateCell(7).SetCellValue(customer.Status);
            }

            // Ghi dữ liệu ra một mảng byte
            using (var memoryStream = new MemoryStream())
            {
                workbook.Write(memoryStream);
                return memoryStream.ToArray();
            }
        }

        public async Task<CustomerDto> GetByUserId(long userId)
        {

            var query = await Repository.GetAllAsync();
            query = query.Where(u => u.UserId.HasValue && u.UserId.Equals(userId));
            var customer = query.FirstOrDefault();
            return ObjectMapper.Map<CustomerDto>(customer);
        }

        public async Task<CustomerDto> GetByCustomerIdAsync(long customerId)
        {
            var query = await Repository.GetAllAsync();
            query = query.Where(u => u.Id == customerId);
            var customer = query.FirstOrDefault();
            return ObjectMapper.Map<CustomerDto>(customer);
        }

        public async Task<int> AssignToSale(CustomerAssignToSaleDto data)
        {
            try
            {
                //var result = await Repository.GetDbContext().Database.ExecuteSqlRawAsync(
                //    "EXEC SP_Customers_AssignToSale @SaleId, @CustomerIds",
                //    new SqlParameter("@SaleId", data.UserId),
                //    new SqlParameter("@CustomerIds", string.Join(',', data.CustomerIds))
                //);
                //return result;

                return await ConnectDb.ExecuteNonQueryAsync("SP_Customers_AssignToSale", CommandType.StoredProcedure,
                    new[] {
                    new SqlParameter("@SaleId", data.UserId),
                    new SqlParameter("@CustomerIds", string.Join(',', data.CustomerIds))
                    }
                );
            }
            catch (Exception ex)
            {
                return -1;
            }
        }

        public async Task<int> LinkToUser(LinkToUserDto data)
        {
            try
            {
                var result = await Repository.GetDbContext().Database.ExecuteSqlRawAsync(
                    "EXEC SP_Customers_LinkToUser  @Id, @UserId",
                    new SqlParameter("@Id", data.Id),
                    new SqlParameter("@UserId", data.UserId)
                );
                return result;
            }
            catch (Exception ex)
            {
                return -1;
            }
        }


        public async Task<List<CustomerDto>> GetChildren(long parentId)
        {
            try
            {
                if (parentId <= 0)
                {
                    return new List<CustomerDto>();
                }
                var query = Repository.GetAll().Where(u => u.ParentId == parentId);
                return ObjectMapper.Map<List<CustomerDto>>(query.ToList());
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        /// <summary>
        /// Lấy danh sách khách hàng con theo ID của khách hàng cha
        /// </summary>
        /// <param name="parentId"></param>
        /// <returns></returns>
        public async Task<List<CustomerDto>> GetAllChildren()
        {
            try
            {
                var query = Repository.GetAll().Where(u => u.ParentId <= 0 && !u.IsAgent);
                return ObjectMapper.Map<List<CustomerDto>>(query.ToList());
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        // add child to parent
        public async Task<bool> AddChildToParent(AddChildToParentDto input)
        {
            try
            {
                var customer = await Repository.GetAsync(input.CustomerId);
                if (customer == null)
                {
                    throw new UserFriendlyException("Khách hàng không tồn tại");
                }

                // Kiểm tra xem customer đã có parent chưa
                if (customer.ParentId > 0)
                {
                    throw new UserFriendlyException("Khách hàng đã có đại lý");
                }

                customer.ParentId = input.Id;
                await Repository.UpdateAsync(customer);
                return true;
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException("An error occurred while adding child to parent.", ex);
            }
        }


        public async Task<List<CustomerForSelectBoxDto>> GetChildrenForSelectBox(string q)
        {
            try
            {
                var parentId = _pbtAppSession.CustomerId;
                var isSale = _roles.Contains(RoleConstants.sale) ||
                             _roles.Contains(RoleConstants.saleadmin) ||
                             _roles.Contains(RoleConstants.admin);
                var query = Repository.GetAll().Where(u => isSale || u.ParentId == parentId).AsEnumerable();
                if (!string.IsNullOrEmpty(q))
                {
                    query = query.Where(u =>
                        u.FullName.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                        u.PhoneNumber.Contains(q, StringComparison.OrdinalIgnoreCase));
                }

                return query.Select(u => new CustomerForSelectBoxDto
                {
                    Id = u.Id,
                    Text = u.FullName + " - " + u.PhoneNumber
                }).ToList();

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<List<CustomerDto>> GetBySale(long saleId)
        {
            try
            {
                var query = Repository.GetAll().Where(u => u.SaleId == saleId);

                return ObjectMapper.Map<List<CustomerDto>>(query.ToList());
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<List<OptionItemDto>> GetCustomerListByCurrentSaleForSelect(string q)
        {
            try
            {
                var userId = _pbtAppSession.UserId;
                var query = Repository.GetAll()
                    .Where(u => u.SaleId == userId).AsEnumerable();

                if (!string.IsNullOrEmpty(q))
                {
                    query = query.Where(u =>
                        u.FullName.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                        u.PhoneNumber.Contains(q, StringComparison.OrdinalIgnoreCase));
                }

                return query.Select(u => new OptionItemDto
                {
                    id = u.Id.ToString(),
                    text = u.Username
                }).ToList();


            }
            catch (Exception ex)
            {

            }

            return new List<OptionItemDto>();
        }

        public async Task<PagedResultDto<CustomerDto>> GetByCurrentSale(PagedResultRequestDto input)
        {
            var userId = _pbtAppSession.UserId;

            var query = await Repository.GetAllAsync();
            query = query.Where(u => u.SaleId == userId);
            var count = query.Count();
            var data = query.Take(input.MaxResultCount).Skip(input.SkipCount).ToList();

            return new PagedResultDto<CustomerDto>()
            {
                Items = data.MapTo<List<CustomerDto>>(),
                TotalCount = count
            };
        }

        public override async Task<CustomerDto> CreateAsync(CreateUpdateCustomerDto input)
        {
            var existsPrefix =
                await Repository.GetAll().Where(u => u.BagPrefix == input.BagPrefix).FirstOrDefaultAsync();
            if (!string.IsNullOrEmpty(input.BagPrefix) && existsPrefix != null)
                throw new UserFriendlyException("Mã bag đã tồn tại");
            // Nếu như user đăng nhập có customerId ( tức là khách hàng đại lý thì thực hiện tạo khách hàng con bằng cách gán parentId vào)
            if (_pbtAppSession != null && _pbtAppSession.CustomerId.HasValue)
            {
                input.IsAgent = false;
                input.ParentId = _pbtAppSession.CustomerId.Value;
            }
            else
            {
                // Ngược lại thì không gán parentId
                input.IsAgent = true;
                input.AgentLevel = 1;
            }

            var customer = await base.CreateAsync(input);

            return customer;
        }


        public async Task<CustomerDto> CreateWithAccountAsync(CreateUpdateCustomerDto input)
        {
            try
            {
                var existsPrefix = await Repository.GetAll().Where(u => u.BagPrefix == input.BagPrefix)
                    .FirstOrDefaultAsync();
                if (!string.IsNullOrEmpty(input.BagPrefix) && existsPrefix != null)
                    throw new UserFriendlyException("Mã bag đã tồn tại");
                // Kiểm tra tồn tại tài khoản, email, sdt chưa?
                if (await _userManager.FindByNameAsync(input.Username) != null)
                {
                    throw new UserFriendlyException("Tài khoản đã tồn tại.");
                }

                if (await _userManager.FindByEmailAsync(input.Email) != null)
                {
                    throw new UserFriendlyException("Email đã tồn tại.");
                }

                var existingCustomer = await Repository.FirstOrDefaultAsync(c => c.PhoneNumber == input.PhoneNumber);
                if (existingCustomer != null)
                {
                    throw new UserFriendlyException("Số điện thoại đã tồn tại.");
                }

                if (_roles.Contains(RoleConstants.saleadmin) && !_roles.Contains(RoleConstants.admin))
                {
                    input.SaleId = _pbtAppSession.UserId ?? 0;
                }


                var customer = await base.CreateAsync(input);

                var password = "12345678a@";
                var user = await _userRegistrationManager.RegisterAsync(
                    input.FullName,
                    input.PhoneNumber,
                    input.Email,
                    input.Username,
                    password,
                    true // Assumed email address is always confirmed. Change this if you want to implement email confirmation.
                );


                // Gán quyền customer cho user
                await _userManager.SetRolesAsync(user, new[] { CustomerRoleName });

                await Repository.GetDbContext().Database.ExecuteSqlRawAsync(
                    "EXEC SP_Customers_LinkToAccount  @CustomerId, @Username",
                    new SqlParameter("@CustomerId", customer.Id),
                    new SqlParameter("@Username", input.Username)
                );

                // get default shipping rate group
                var defaultShippingRateGroup = await _shippingRateGroupRepository.FirstOrDefaultAsync(
                    u => u.IsDefaultForCustomer);
                // set for current customer
                if (defaultShippingRateGroup != null)
                {
                    var shippingRateCustomer = new ShippingRateCustomer
                    {
                        CustomerId = customer.Id,
                        ShippingRateGroupId = defaultShippingRateGroup.Id
                    };
                    await _shippingRateCustomerRepository.InsertAsync(shippingRateCustomer);
                }

                return customer;

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async Task AddCustomersByUsernameListAsync(List<string> usernames)
        {
            const string defaultPassword = "123456"; // Mật khẩu mặc định

            foreach (var username in usernames)
            {
                try
                {
                    // Tạo khách hàng
                    var customer = new Customer
                    {
                        Username = username,
                        FullName = username, // Giả định tên đầy đủ là tên đăng nhập
                        Email = username + "@mail.com",

                        WarehouseId = 7, // Giả định kho mặc định là 7
                    };

                    // Lưu khách hàng vào cơ sở dữ liệu
                    var customerInsertRs = await Repository.InsertAsync(customer);

                    var user = await _userRegistrationManager.RegisterAsync(
                        username,
                        "",
                        username + "@mail.com",
                        username,
                        defaultPassword,
                        true // Assumed email address is always confirmed. Change this if you want to implement email confirmation.
                    );
                    // Gán quyền customer cho user
                    await _userManager.SetRolesAsync(user, new[] { CustomerRoleName });
                    await _userManager.ChangePasswordAsync(user, defaultPassword);
                    await Repository.GetDbContext().Database.ExecuteSqlRawAsync(
                        "EXEC SP_Customers_LinkToAccount  @CustomerId, @Username",
                        new SqlParameter("@CustomerId", customer.Id),
                        new SqlParameter("@Username", username)
                    );

                }
                catch (Exception ex)
                {
                    // Ghi log lỗi nếu có vấn đề xảy ra
                    Logger.Error($"Failed to create customer for username: {username}", ex);
                }
            }
        }

        public async Task<List<CustomerDto>> GetFull()
        {
            var data = await Repository.GetAllAsync();
            if (data == null)
            {
                throw new UserFriendlyException($"Customer is empty");
            }

            return ObjectMapper.Map<List<CustomerDto>>(data);

        }


        public async Task<List<CustomerFinancialInfoDto>> GetCustomerListWithFinancialAsync()
        {
            var warehouseId = _pbtAppSession.WarehouseId;
            var data = await ConnectDb.GetListAsync<CustomerFinancialInfoDto>(
                "SP_Customers_GetAllWithFinance",
                System.Data.CommandType.StoredProcedure,
                new[]
                {
                    new SqlParameter("@WarehouseId", warehouseId)
                }
            );

            return data;
        }


        public async Task<List<CustomerDto>> GetCustomerListByCurrentUserWarehouseByAsync()
        {
            var data = await Repository.GetAllAsync();

            // get current user warehouse id
            var warehouseId = _pbtAppSession.WarehouseId;

            if (_roles.Contains(RoleConstants.warehouseVN) && !_roles.Contains(RoleConstants.admin))
            {
                data = data.Where(x => x.WarehouseId == warehouseId);
            }

            return ObjectMapper.Map<List<CustomerDto>>(data);
        }

        [HttpGet]
        public async Task<PagedResultDto<CustomerDto>> GetFullFilter([FromQuery] string filter,
            [FromQuery] int maxResultCount = 10, [FromQuery] int skipCount = 0)
        {
            var query = (await Repository.GetAllAsync())
                .Where(x => string.IsNullOrEmpty(filter) || x.FullName.ToLower().Contains(filter.ToLower()));

            var totalCount = query.Count();

            if (maxResultCount > 0)
            {
                query = query.Skip(skipCount).Take(maxResultCount);
            }
            else
            {
                query = query.Skip(skipCount);
            }

            var items = query.ToList();

            return new PagedResultDto<CustomerDto>
            {
                Items = ObjectMapper.Map<List<CustomerDto>>(items),
                TotalCount = totalCount
            };
        }

        public async Task<List<CustomerDto>> SearchCustomersAsync(string keyword)
        {

            var customers = await Repository.GetAllListAsync();
            var result = customers.Where(u => string.IsNullOrEmpty(keyword) ||
                                              u.FullName.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                                              u.Email.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                                              u.PhoneNumber.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                .Where(u => u.IsAgent)
                .Select(u => new CustomerDto
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    Username = u.Username,
                }).ToList();

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="customerDto"></param>
        /// <returns></returns>

        public async Task<CustomerDto> CreateCustomerByRegistration(CreateUpdateCustomerDto customerDto)
        {
            var existsPrefix = await Repository.GetAll().Where(u => u.BagPrefix == customerDto.BagPrefix)
                .FirstOrDefaultAsync();
            if (!string.IsNullOrEmpty(customerDto.BagPrefix) && existsPrefix != null)
                throw new UserFriendlyException("Mã bag đã tồn tại");

            var customer = await base.CreateAsync(customerDto);

            return customer;
        }

        public async Task<CustomerDto> GetByPhoneNumber(string PhoneNumber)
        {
            var query = await Repository.GetAllAsync();
            query = query.Where(u => u.PhoneNumber.Equals(PhoneNumber, StringComparison.OrdinalIgnoreCase));
            var customer = query.FirstOrDefault();
            return ObjectMapper.Map<CustomerDto>(customer);
        }

        public async Task<CustomerDto> GetByUsernameOrPhone(string keyword)
        {
            var query = await Repository.GetAllAsync();
            query = query.Where(u => u.PhoneNumber == keyword
                                     || u.Username.ToUpper() == keyword.ToUpper()
            );
            var customer = query.FirstOrDefault();
            return ObjectMapper.Map<CustomerDto>(customer);
        }

        public async override Task<CustomerDto> UpdateAsync(CustomerDto input)
        {
            var existsPrefix = await Repository.GetAll().Where(u => u.BagPrefix == input.BagPrefix && u.Id != input.Id)
                .FirstOrDefaultAsync();
            if (!string.IsNullOrEmpty(input.BagPrefix) && existsPrefix != null)
                throw new UserFriendlyException("Mã bag đã tồn tại");
            var customer = await Repository.GetAsync(input.Id);

            customer.FullName = input.FullName;
            customer.Email = input.Email;
            customer.PhoneNumber = input.PhoneNumber;
            customer.Address = input.Address;
            customer.IsAgent = input.IsAgent;
            customer.AgentLevel = input.AgentLevel;
            customer.Notes = input.Notes;
            customer.WarehouseId = input.WarehouseId;
            customer.InsurancePercentage = input.InsurancePercentage;

            await Repository.UpdateAsync(customer);
            return ObjectMapper.Map<CustomerDto>(customer);

        }

        public async Task<List<OptionItemDto>> GetCustomerBySaleOrParentForSelectAsync(string q)
        {
            try
            {
                var userId = _pbtAppSession.UserId;
                var customerId = _pbtAppSession.CustomerId;
                var query = await Repository.GetAllAsync();
                query = query.Where(u => u.SaleId == userId ||
                                         u.ParentId == customerId || u.Id == customerId
                );
                if (!string.IsNullOrEmpty(q))
                {
                    query = query.Where(u =>
                        u.Username.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                        u.PhoneNumber.Contains(q, StringComparison.OrdinalIgnoreCase));
                }

                return query.Select(u => new OptionItemDto
                {
                    id = u.Id.ToString(),
                    text = u.Username
                }).ToList();
            }
            catch (Exception)
            {
                return new List<OptionItemDto>();
            }
        }


        public async Task<List<OptionItemDto>> GetAllForSelectBySaleAsync(string q)
        {
            try
            {

                if (_roles.Contains(RoleConstants.admin) || _roles.Contains(RoleConstants.saleadmin) ||
                    _roles.Contains(RoleConstants.sale))
                {

                    var query = (await Repository.GetAllAsync());

                    if (!string.IsNullOrEmpty(q))
                    {
                        query = query.Where(u =>
                        u.FullName.Contains(q) ||
                        u.Username.Contains(q));
                    }

                    return query.Select(u => new OptionItemDto
                    {
                        id = u.Id.ToString(),
                        text = u.Username
                    }).ToList();
                }

            }
            catch (Exception ex)
            {

            }

            return new List<OptionItemDto>();
        }

        public async Task<List<OptionItemDto>> GetAllForSelectAsync(string q = "")
        {
            try
            {
                var query = (await Repository.GetAllAsync());

                if (!string.IsNullOrEmpty(q))
                {
                    query = query.Where(u =>
                        u.Username.Contains(q));
                }

                return await query.Select(u => new OptionItemDto
                {
                    id = u.Id.ToString(),
                    text = u.Username
                }).ToListAsync();


            }
            catch (Exception ex)
            {

            }

            return new List<OptionItemDto>();
        }


        public async Task<bool> UpdatePasswordAsync(ChangePasswordDto input)
        {
            try
            {

                var customer = await Repository.GetAsync(input.CustomerId);
                if (customer == null)
                {
                    throw new UserFriendlyException("Customer not found.");
                }

                var user = await _userManager.Users.FirstOrDefaultAsync(u => u.CustomerId == customer.Id);
                if (user == null)
                {
                    throw new UserFriendlyException("User not found.");
                }

                var result = await _userManager.ChangePasswordAsync(user, input.Password);
                if (!result.Succeeded)
                {
                    var messages = "";
                    var errors = result.Errors;
                    if (errors.FirstOrDefault(x => x.Code == "PasswordRequiresNonAlphanumeric") != null)
                    {
                        messages += "Mật khẩu phải chứa ký tự đặc biệt. ";
                    }
                    if (errors.FirstOrDefault(x => x.Code == "PasswordRequiresLower") != null)
                    {
                        messages += "\n Mật khẩu phải chứa ký tự thường. ";
                    }
                    if (errors.FirstOrDefault(x => x.Code == "PasswordRequiresUpper") != null)
                    {
                        messages += "\n Mật khẩu phải chứa ký tự in hoa. ";
                    }
                    throw new UserFriendlyException(messages);
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException(ex.Message);
            }
        }

        public async Task<CustomerBalanceWithDebtDto> GetByCustomerWalletBalanceAsync(long customerId)
        {
            var customer = await Repository.GetAsync(customerId);
            if (customer == null)
            {
                throw new UserFriendlyException("Customer not found.");
            }

            return new CustomerBalanceWithDebtDto
            {
                CustomerId = customer.Id,
                AvailableCreditLimit = customer.MaxDebt - customer.CurrentDebt,
                CurrentAmount = customer.CurrentAmount >= 0 ? customer.CurrentAmount : 0,
                CurrentDebt = customer.CurrentAmount > 0 ? 0 : Math.Abs(customer.CurrentAmount),
                MaxDebt = customer.MaxDebt,
                CustomerName = customer.FullName,
                Username = customer.Username

            };
        }

        public async Task<bool> UpdateMaxDebtAsync(UpdateMaxDebtDto input)
        {
            try
            {
                // Lấy thông tin khách hàng từ repository
                var customer = await Repository.GetAsync(input.CustomerId);
                if (customer == null)
                {
                    throw new UserFriendlyException("Customer not found.");
                }

                // Cập nhật công nợ tối đa
                customer.MaxDebt = input.MaxDebt;

                // Lưu thay đổi vào database
                await Repository.UpdateAsync(customer);

                return true;
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException("An error occurred while updating the maximum debt.", ex);
            }
        }

        /// <summary>
        /// Lấy thông tin khách hàng theo ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="UserFriendlyException"></exception>
        [AllowAnonymous]
        [AbpAllowAnonymous]
        public async Task<CustomerDto> GetCustomerById(long id)
        {
            var customer = (await Repository.GetAllIncludingAsync(x => x.Warehouse)).FirstOrDefault(u => u.Id == id);
            if (customer == null)
            {
                throw new UserFriendlyException("Customer not found.");
            }

            return ObjectMapper.Map<CustomerDto>(customer);
        }

        //get current customer login

        public async Task<CustomerDto> GetLoginCustomerAsync()
        {
            var userId = _abpSession.UserId.Value;
            var query = await Repository.GetAllAsync();
            query = query.Where(u => u.UserId.HasValue && u.UserId == userId);
            var customer = query.FirstOrDefault();
            return ObjectMapper.Map<CustomerDto>(customer);
        }

        public async Task<List<CustomerDto>> GetCustomersByCurrentUserForSelectOrderViewAsync(string query)
        {
            var currentUserId = AbpSession.UserId;
            try
            {
                var permissionCase = 1;
                var customerDtoIds = new List<CustomerIdDto>();
                // admin và sale admin sẽ nhìn thấy tất cả kiện hàng
                if (await PermissionChecker.IsGrantedAsync(PermissionNames.Role_Admin) || await PermissionChecker.IsGrantedAsync(PermissionNames.Role_SaleAdmin))
                {
                    // query = query.Where(x => x.CustomerId == _pbtAppSession.CustomerId);
                    permissionCase = 1;
                    Logger.Info("Admin hoặc Sale Admin truy cập vào danh sách kiện hàng.");
                }
                // sale chỉ nhìn thấy kiện hàng của khách hàng do mình quản lý
                else if (PermissionChecker.IsGranted(PermissionNames.Role_Sale))
                {
                    permissionCase = 2;
                    Logger.Info($"Sale truy cập vào danh sách kiện hàng. SaleId: {_pbtAppSession.CustomerId}");
                    customerDtoIds = await ConnectDb.GetListAsync<CustomerIdDto>(
                   "SP_Customers_GetIdsBySaleId",
                   CommandType.StoredProcedure,
                    new[]
                   {
                    new SqlParameter("@SaleId", currentUserId)
                   });
                }
                else if (PermissionChecker.IsGranted(PermissionNames.Role_Customer))
                {
                    permissionCase = 3;
                    Logger.Info($"Customer truy cập vào danh sách kiện hàng. CustomerId: {_pbtAppSession.CustomerId}");
                    customerDtoIds = await ConnectDb.GetListAsync<CustomerIdDto>(
                        "SP_Customers_GetIdsByParentId",
                        CommandType.StoredProcedure,
                        new[]
                        {
                        new SqlParameter("@CustomerId", _pbtAppSession.CustomerId)
                        }
                    );
                }
                else
                {
                    permissionCase = 4;
                }

                var customerIds = new List<long>();
                if (customerDtoIds != null)
                {
                    customerIds = customerDtoIds.Select(u => u.CustomerId).ToList();
                }


                // Tạo query để lấy danh sách kiện hàng
                var data = (await Repository.GetAllAsync())
                    .Where(x =>
                       (permissionCase == 1) || // admin và sale admin nhìn thấy tất cả
                       ((permissionCase == 2 || permissionCase == 3) && customerIds.Contains(x.Id)) // sale nhìn thấy kiện hàng của khách hàng do mình quản lý
                    )
                    .ToList();


                return ObjectMapper.Map<List<CustomerDto>>(data);

            }
            catch (Exception ex)
            {

                throw;
            }
        }

    
        public async Task<List<CustomerDto>> GetCustomersByCurrentUserAsync()
        {
            try
            {
                var permissionCaseCheckResult = GetPermissionCheckerWithCustomerIds();
                if (permissionCaseCheckResult.PermissionCase <= 0)
                {
                    return new List<CustomerDto>();
                }
                var customerIdsStr = string.Join(",", permissionCaseCheckResult.CustomerIds);

                var data = await ConnectDb.GetListAsync<CustomerDto>(
                    "SP_Customers_GetByPermissionCaseAndCustomerIds",
                    CommandType.StoredProcedure,
                    new[]
                    {
                        new SqlParameter("@permissionCase", permissionCaseCheckResult.PermissionCase),
                        new SqlParameter("@customerIds", customerIdsStr)
                    }
                );
                return data;
            }
            catch (Exception ex)
            {
                Logger.Error("Lỗi khi lấy danh sách khách hàng theo người dùng hiện tại", ex);

                return new List<CustomerDto>();
            }
        }

        public async Task<List<OptionItemDto>> GetCustomersBagWithPartnerAsync(BagWithPartnerRequestDto input)
        {
            var userId = _abpSession.UserId.Value;
            var roles = _roles.Select(r => r.ToLower()).ToList();
            var warehouseId = _pbtAppSession.WarehouseId;

            var permissionCase = 1;
            if (roles.Contains(RoleConstants.admin) || roles.Contains(RoleConstants.saleadmin) || PermissionChecker.IsGranted(PermissionNames.Function_ViewAllCustomer))
            {
                // Nếu là admin thì lấy tất cả khách hàng
                permissionCase = 1;
            }
            else if (roles.Contains(RoleConstants.sale))
            {
                // Nếu là sale thì lấy khách hàng được gán cho mình và các khách hàng con của họ
                permissionCase = 2;
            }
            else if (roles.Contains(RoleConstants.customer))
            {
                // Nếu là customer thì lấy chính mình và các khách hàng con
                permissionCase = 3;
            }
            var parameters = new[]
            {
                new SqlParameter("@UserId", userId),
                new SqlParameter("@permissionCase", permissionCase),
                new SqlParameter("@WarehouseId", (object?)warehouseId ?? DBNull.Value),
                new SqlParameter("@ExportDateFrom", (object?)input.StartExportDate ?? DBNull.Value),
                new SqlParameter("@ExportDateTo", (object?)input.EndExportDate ?? DBNull.Value),
                new SqlParameter("@ImportDateFrom", (object?)input.StartImportDate ?? DBNull.Value),
                new SqlParameter("@ImportDateTo", (object?)input.EndImportDate ?? DBNull.Value),
                new SqlParameter("@ExportDateVNFrom", (object?)input.StartExportVNDate ?? DBNull.Value),
                new SqlParameter("@ExportDateVNTo", (object?)input.EndExportVNDate ?? DBNull.Value),
                new SqlParameter("@Keyword", (object?)input.Keyword ?? DBNull.Value),
            };

            var customers = await ConnectDb.GetListAsync<CustomerDto>(
                "SP_Customers_GetBySaleImportExportView",
                CommandType.StoredProcedure,
                parameters
            );

            return customers.Select(u => new OptionItemDto()
            {
                id = u.Id.ToString(),
                text = u.Username
            }).ToList();
        }

        public async Task<List<OptionItemDto>> GetCustomersImportExportViewAsync(ImportExportWithBagRequestDto input)
        {
            var userId = _abpSession.UserId.Value;
            var roles = _roles.Select(r => r.ToLower()).ToList();
            var warehouseId = _pbtAppSession.WarehouseId;

            var permissionCase = 1;
            if (roles.Contains(RoleConstants.admin) || roles.Contains(RoleConstants.saleadmin) || PermissionChecker.IsGranted(PermissionNames.Function_ViewAllCustomer))
            {
                // Nếu là admin thì lấy tất cả khách hàng
                permissionCase = 1;
            }
            else if (roles.Contains(RoleConstants.sale))
            {
                // Nếu là sale thì lấy khách hàng được gán cho mình và các khách hàng con của họ
                permissionCase = 2;
            }
            else if (roles.Contains(RoleConstants.customer))
            {
                // Nếu là customer thì lấy chính mình và các khách hàng con
                permissionCase = 3;
            }
            var parameters = new[]
            {
                new SqlParameter("@UserId", userId),
                new SqlParameter("@permissionCase", permissionCase),
                new SqlParameter("@WarehouseId", (object?)warehouseId ?? DBNull.Value),
                new SqlParameter("@ExportDateFrom", (object?)input.StartExportDate ?? DBNull.Value),
                new SqlParameter("@ExportDateTo", (object?)input.EndExportDate ?? DBNull.Value),
                new SqlParameter("@ImportDateFrom", (object?)input.StartImportDate ?? DBNull.Value),
                new SqlParameter("@ImportDateTo", (object?)input.EndImportDate ?? DBNull.Value),
                new SqlParameter("@ExportDateVNFrom", (object?)input.StartExportVNDate ?? DBNull.Value),
                new SqlParameter("@ExportDateVNTo", (object?)input.EndExportVNDate ?? DBNull.Value),
                new SqlParameter("@Keyword", (object?)input.Keyword ?? DBNull.Value),
            };

            var customers = await ConnectDb.GetListAsync<CustomerDto>(
                "SP_Customers_GetBySaleImportExportView",
                CommandType.StoredProcedure,
                parameters
            );

            return customers.Select(u => new OptionItemDto()
            {
                id = u.Id.ToString(),
                text = u.Username
            }).ToList();
        }

        public async Task<long?> GetCurrentCustomerId()
        {
            var userId = _abpSession.UserId.Value;
            var cookieCurrentUserIdKey = $"CurrentCustomerId_{userId}";
            var cookieCurrentAmountKey = $"CurrentCurrentAmount_{userId}";
            var cookieCurrentDebtKey = $"CurrentCurrentDebt_{userId}";
            var cookieCurrentWarehouseKey = $"CurrentCurrentWarehouseId_{userId}";
            // Lấy giá trị từ cookie
            var customerIdStr = _cookieService.GetCookie(cookieCurrentUserIdKey);
            if (!string.IsNullOrEmpty(customerIdStr) && long.TryParse(customerIdStr, out var customerIdFromCookie))
            {
                return customerIdFromCookie;
            }

            // Nếu không có trong cookie, lấy trong DB
            var customer = await Repository.FirstOrDefaultAsync(u => u.UserId == userId);
            //var customer = customers.Where(c => c.UserId.HasValue && c.UserId == userId).FirstOrDefault();
            if (customer == null)
            {
                return null;
            }

            _cookieService.SetCookie(cookieCurrentWarehouseKey, customer.WarehouseId.ToString());
            _cookieService.SetCookie(cookieCurrentUserIdKey, customer.Id.ToString());
            _cookieService.SetCookie(cookieCurrentAmountKey, customer.CurrentAmount.ToString());
            _cookieService.SetCookie(cookieCurrentDebtKey, customer.CurrentDebt.ToString());
            return customer.Id;
        }

        public async Task<CustomerDto> GetCustomerByUserNameWithCacheAsync(string customerName)
        {
            // Kiểm tra cache trước
            var cacheKey = $"Customer_{customerName}";
            var cachedCustomer = _cacheService.GetCacheValue<CustomerDto>(cacheKey);
            if (cachedCustomer != null)
            {
                return cachedCustomer;
            }

            // Nếu không có trong cache, lấy từ database
            var customer = await Repository.FirstOrDefaultAsync(u => u.Username == customerName);
            if (customer == null)
            {
                return null;
            }

            // Lưu vào cache
            var dto = ObjectMapper.Map<CustomerDto>(customer);
            _cacheService.SetCacheValue(cacheKey, dto);

            return dto;
        }

        /// <summary>
        /// Lấy tỷ lệ bảo hiểm của khách hàng
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        /// <exception cref="UserFriendlyException"></exception>
        public async Task<decimal> GetInsurancePercentage(long customerId)
        {
            try
            {
                // Lấy giá trị bảo hiểm từ database
                var customer = await Repository.FirstOrDefaultAsync(u => u.Id == customerId);
                if (customer != null)
                {
                    if (customer.InsurancePercentage > 0)
                    {
                        return (decimal)customer.InsurancePercentage;
                    }

                    // Lấy giá trị bảo hiểm từ cấu hình
                    var insurancePercentageConfig =
                        await _configurationSettingAppService.GetValueAsync("insurance_percentage");
                    if (decimal.TryParse(insurancePercentageConfig, out var percentage))
                    {
                        return percentage;
                    }
                    else
                    {
                        throw new UserFriendlyException("Giá trị bảo hiểm không hợp lệ.");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException("Lỗi khi lấy giá trị bảo hiểm.", ex);
            }
            return 0;
        }

        /// <summary>
        /// Xóa ParentId của khách hàng
        /// </summary>
        /// <param name="customerId"></param>
        /// <exception cref="UserFriendlyException"></exception>
        public async Task RemoveParentIdAsync(long id)
        {
            try
            {
                var customer = await Repository.GetAsync(id);
                if (customer == null)
                {
                    throw new UserFriendlyException("Customer not found.");
                }
                // Xóa ParentId
                customer.ParentId = 0;
                // Cập nhật vào database
                await Repository.UpdateAsync(customer);
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException("An error occurred while removing the parent ID.", ex);
            }
        }

        // lock customer
        public async Task<bool> LockCustomerAsync(long customerId)
        {
            try
            {
                var customer = await Repository.GetAsync(customerId);
                if (customer == null)
                {
                    throw new UserFriendlyException("Customer not found.");
                }

                // Đặt trạng thái khóa
                //customer.IsLocked = true;
                await Repository.UpdateAsync(customer);
                return true;
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException("An error occurred while locking the customer.", ex);
            }
        }

        //public override async Task DeleteAsync(EntityDto<long> input)
        //{
        //    // Kiểm tra xem khách hàng có tồn tại không

        //    var user = await _userManager.Users.FirstOrDefaultAsync(u => u.CustomerId == input.Id);
        //    if (user != null)
        //    {
        //        // Nếu có, xóa tài khoản người dùng liên kết với khách hàng
        //        await _userManager.DeleteAsync(user);
        //    }
        //    var customer = await Repository.FirstOrDefaultAsync(input.Id);
        //    return  base.DeleteAsync(input);
        //}


        public async Task DeleteCustomerAsync(EntityDto<long> input)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.CustomerId == input.Id);
            if (user != null)
            {
                // Nếu có, xóa tài khoản người dùng liên kết với khách hàng

                await _userManager.DeleteAsync(user);
            }
            base.DeleteAsync(input);
        }

        public async Task LockUserAccount(long userId)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user != null)
            {
                user.IsActive = !user.IsActive;
                await _userManager.UpdateAsync(user);
            }
            // logout user if isActive = false
        }

        public async Task<IActionResult> UpdateAmount(UpdateAmountDto input)
        {
            //  if (await _userManager.CheckPasswordAsync(user, input.CurrentPassword))



            try
            {
                if (_abpSession.UserId == null)
                {
                    return new BadRequestObjectResult(new { message = "User not logged in." });
                }

                var currentUser = await _userManager.GetUserByIdAsync(_abpSession.GetUserId());
                if (currentUser == null)
                {
                    return new BadRequestObjectResult(new { message = "User not found." });
                }

                var pinCheck = input.SpecialPIN;
                if (pinCheck != currentUser.SpecialPIN)
                {
                    return new BadRequestObjectResult(new { message = "Invalid Special PIN." });
                }

                var customer = await Repository.GetAsync(input.CustomerId);
                if (customer == null)
                {
                    return new BadRequestObjectResult(new { message = "Customer not found." });
                }
                customer.CurrentAmount = input.NewAmount;

                var customerTransaction = new CustomerTransaction
                {
                    CustomerId = input.CustomerId,
                    Amount = input.NewAmount,
                    TransactionType = input.NewAmount > 0 ? (int)CustomerTransactionUpdateTypeEnum.AddWallet : (int)CustomerTransactionUpdateTypeEnum.SubtractWallet,
                    BalanceAfterTransaction = customer.CurrentAmount,
                    Description = "",
                };
                await _customerTransactionRepository.InsertAsync(customerTransaction);

                await Repository.UpdateAsync(customer);

            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(new { message = ex.Message });
            }

            return new OkObjectResult(new
            {
                success = true,
                message = "Thành công"

            });
        }


        public async Task<List<long>> GetCustomerIdsByDateRange(
            DateTime? exportDateFrom,
            DateTime? exportDateTo,
            DateTime? importDateFrom,
            DateTime? importDateTo,
            DateTime? exportDateVNFrom,
            DateTime? exportDateVNTo)
        {
            var parameters = new[]
            {
            new SqlParameter("@ExportDateFrom", (object?)exportDateFrom ?? DBNull.Value),
            new SqlParameter("@ExportDateTo", (object?)exportDateTo ?? DBNull.Value),
            new SqlParameter("@ImportDateFrom", (object?)importDateFrom ?? DBNull.Value),
            new SqlParameter("@ImportDateTo", (object?)importDateTo ?? DBNull.Value),
            new SqlParameter("@ExportDateVNFrom", (object?)exportDateVNFrom ?? DBNull.Value),
            new SqlParameter("@ExportDateVNTo", (object?)exportDateVNTo ?? DBNull.Value)
        };

            var list = await ConnectDb.GetListAsync<CustomerIdDto>(
                "sp_Package_GetCustomerIds_ByExportImportDate",
                CommandType.StoredProcedure,
                parameters);

            // Chuyển sang List<long>
            return list.ConvertAll(x => x.CustomerId);
        }


        /// <summary>
        /// 1. Admin
        /// 2. Sale Admin
        /// 3. Sale
        /// 4. Customer
        /// 5. WarehouseCN
        /// 6. WarehouseVN
        /// 7. SaleCustom
        /// </summary>
        /// <returns></returns>
        private (int PermissionCase, List<long> CustomerIds) GetPermissionCheckerWithCustomerIds()
        {
            try
            {
                var currentUserId = AbpSession.UserId;
                string cacheKey = $"PermissionChecker_User_{AbpSession.UserId}";
                // Kiểm tra cache
                var cachedResult = _cacheService.GetCacheValue<(int HasPermission, List<long> CustomerIds)>(cacheKey);
                if (cachedResult != default)
                {
                    return cachedResult; // Trả về kết quả từ cache nếu đã có
                }
                var permissionCase = -1;
                var currentUserWarehouseId = _pbtAppSession.WarehouseId;

                var customerDtoIds = new List<CustomerIdDto>();

                // admin thì nhìn thấy tất cả kiện hàng
                if (PermissionChecker.IsGranted(PermissionNames.Role_Admin))
                {
                    permissionCase = 1;
                    Logger.Info("Admin xem được toàn bộ khách hàng");
                }

                // Sale admin nhìn thấy tất cả các kiện hàng của mình, của sale dưới quyền và của khách hàng do mình quản lý
                else if (PermissionChecker.IsGranted(PermissionNames.Role_SaleAdmin))
                {
                    permissionCase = 2;
                    Logger.Info($"Sale Admin xem được các khách hàng của mình và cấp dưới của mình. SaleAdminId: {_pbtAppSession.CustomerId}");

                    customerDtoIds = ConnectDb.GetList<CustomerIdDto>("SP_Customers_GetIdsBySaleAdminUserId", CommandType.StoredProcedure,
                    new[]
                    {
                        new SqlParameter("@SaleAdminUserId", currentUserId)
                    });
                }

                // sale chỉ nhìn thấy kiện hàng của khách hàng do mình quản lý
                else if (PermissionChecker.IsGranted(PermissionNames.Role_Sale))
                {
                    permissionCase = 3;
                    Logger.Info($"Sale truy cập vào danh sách khách hàng của mình. SaleId: {_pbtAppSession.CustomerId}");
                    customerDtoIds = ConnectDb.GetList<CustomerIdDto>("SP_Customers_GetIdsBySaleId", CommandType.StoredProcedure,
                    new[]
                   {
                    new SqlParameter("@SaleId", currentUserId)
                   });
                }
                // customer chỉ nhìn thấy kiện hàng của mình
                else if (PermissionChecker.IsGranted(PermissionNames.Role_Customer))
                {
                    permissionCase = 4;
                    Logger.Info($"Customer truy cập vào danh sách khách hàng của mình và chính mình. CustomerId: {_pbtAppSession.CustomerId}");
                    customerDtoIds = ConnectDb.GetList<CustomerIdDto>(
                        "SP_Customers_GetIdsByParentId",
                        CommandType.StoredProcedure,
                        new[]
                        {
                        new SqlParameter("@CustomerId", _pbtAppSession.CustomerId)
                        }
                    );
                }
                else if (PermissionChecker.IsGranted(PermissionNames.Role_WarehouseCN))
                {
                    Logger.Info($"Warehouse CN truy cập vào danh sách kiện hàng. WarehouseId: {currentUserWarehouseId}");
                    permissionCase = 5;
                }
                else if (PermissionChecker.IsGranted(PermissionNames.Role_WarehouseVN))
                {
                    Logger.Info($"Warehouse VN truy cập vào danh sách kiện hàng. WarehouseId: {currentUserWarehouseId}");
                    permissionCase = 6;
                }

                else if (PermissionChecker.IsGranted(PermissionNames.Role_SaleCustom))
                {
                    permissionCase = 7;
                    Logger.Info($"Sale Customer truy cập vào danh sách khách hàng của mình và chính mình. CustomerId: {_pbtAppSession.CustomerId}");
                    customerDtoIds = ConnectDb.GetList<CustomerIdDto>("SP_Customers_GetIdsBySaleId", CommandType.StoredProcedure,
                    new[]
                    {
                        new SqlParameter("@SaleId", currentUserId)
                    });
                }
                else
                {
                    permissionCase = -1;
                    Logger.Warn($"Người dùng không có quyền truy cập danh sách kiện hàng. UserId: {currentUserId}");
                }

                cachedResult = (permissionCase, customerDtoIds.Select(u => u.CustomerId).Distinct().ToList());
                // Lưu vào cache
                _cacheService.SetCacheValue(cacheKey, cachedResult, TimeSpan.FromHours(8));

                return cachedResult;

            }
            catch (Exception ex)
            {
                Logger.Error("Gặp lỗi khi kiểm tra quyền lấy thông tin khách hàng theo người dùng", ex);
            }
            return (-1, new List<long>());
        }

    }
}
