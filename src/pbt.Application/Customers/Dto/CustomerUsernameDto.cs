using Abp.Application.Services.Dto;

namespace pbt.Customers.Dto
{
    public class CustomerUsernameDto : EntityDto<long>
    {
        public string UserName { get; set; }
    }
}
