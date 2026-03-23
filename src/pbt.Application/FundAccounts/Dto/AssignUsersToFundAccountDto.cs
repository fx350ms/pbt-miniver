using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace pbt.FundAccounts.Dto
{
    public class AssignUsersToFundAccountDto
    {
        [Required]
        public int FundAccountId { get; set; } // ID of the Fund Account

        [Required]
        public List<long> UserIds { get; set; } // List of User IDs to assign
    }
}