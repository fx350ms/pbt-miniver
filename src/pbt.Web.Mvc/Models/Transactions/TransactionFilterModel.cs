using pbt.FundAccounts.Dto;
using System.Collections.Generic;

namespace pbt.Web.Models.Transactions
{
    public class TransactionFilterModel
    {
        public List<FundAccountDto> FundAccounts { get; set; } = new List<FundAccountDto>();
    }
}
