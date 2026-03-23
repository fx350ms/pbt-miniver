using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace pbt.Entities
{
    public class FundAccountPermission : Entity<int>
    {
        public int FundAccountId { get; set; } // Reference to the FundAccount
        public long UserId { get; set; } // Reference to the User

    }
}