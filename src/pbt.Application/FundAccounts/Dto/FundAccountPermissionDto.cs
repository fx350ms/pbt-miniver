using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace pbt.FundAccounts.Dto
{
    public class FundAccountPermissionDto : EntityDto<int>
    {
        public int FundAccountId { get; set; } // Reference to the FundAccount
        public long UserId { get; set; } // Reference to the User

        public string FundAccountName { get; set; } // Optional: For display purposes
        public string UserName { get; set; } // Optional: For display purposes
    }

    public class CreateFundAccountPermissionDto
    {
        [Required]
        public int FundAccountId { get; set; }

        [Required]
        public long UserId { get; set; }
    }
    
}