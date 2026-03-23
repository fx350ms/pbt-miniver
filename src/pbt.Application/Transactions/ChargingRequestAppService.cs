using Abp.Application.Services.Dto;
using Abp.Application.Services;
using Abp.Domain.Repositories;
using Microsoft.Extensions.Configuration;
using pbt.Transactions.Dto;
using pbt.Security;
using System.Threading.Tasks;
using pbt.Entities;

namespace pbt.Transactions
{
    //[AbpMvcAuthorize(PermissionNames.Pages_Users)]
    public class ChargingRequestAppService : AsyncCrudAppService<ChargingRequest, ChargingRequestDto, long, PagedResultRequestDto, ChargingRequestDto, ChargingRequestDto>, IChargingRequestAppService
    {
        private readonly IConfiguration _configuration;
        private readonly IRepository<CharingSource, int> _charingSourceRepository;

        public ChargingRequestAppService(IRepository<ChargingRequest, long> repository, IConfiguration configuration, IRepository<CharingSource, int> charingSourceRepository)
            : base(repository)
        {
            _configuration = configuration;
            _charingSourceRepository = charingSourceRepository;
        }

        public async Task<ChargingRequestDto> ProcessAsync(ChargingRequestDto input)
        {
            // Lưu Log

            // Retrieve SecurityKey from appsettings.json
            var securityKey = _configuration["Authentication:JwtBearer:SecurityKey"];

            // Concatenate input data with SecurityKey
            var dataToHash = $"{input.Amount}{input.TransactionDate}{input.Description}{input.ReferenceCode}{input.Source}{input.SourceType}{securityKey}";

            // Compute MD5 hash
            var computedSign = StringEncode.MD5(dataToHash);

            // Verify the sign
            if (computedSign != input.Sign)
            {
                throw new Abp.UI.UserFriendlyException("Invalid sign");
            }

            // Kiểm tra thiết bị
            var charingSource = await _charingSourceRepository.FirstOrDefaultAsync(cs => cs.Data == input.Source && cs.SourceType == input.SourceType);
            if (charingSource == null)
            {
                throw new Abp.UI.UserFriendlyException("Invalid device source or source type");
            }

            // Thêm vào DB
            var createdRequest = await base.CreateAsync(input);

            // Thêm vào transaction (Implement transaction logic here)


            // Cộng tiền (Implement money addition logic here)

            return createdRequest;
        }
    }
}