using pbt.FundAccounts.Dto;
using pbt.Transactions.Dto;

namespace pbt.Web.Models.Transactions
{
    public class TransactionDetailViewModel
    {
        //public long Id { get; set; }
        //public string FundAccountName { get; set; }
        //public decimal FundAccountBalance { get; set; }
        //public string Currency { get; set; }
        //public int TransactionType { get; set; }
        //public int TransactionDirection { get; set; }
        //public decimal Amount { get; set; }
        //public string RefCode { get; set; }
        //public string OrderId { get; set; }
        //public string RecipientPayerName { get; set; }
        //public decimal TotalDebt { get; set; }
        //public decimal MaxDebt { get; set; }
        //public decimal CurrentAmount { get; set; }
        //public string TransactionContent { get; set; }
        //public string Notes { get; set; }
        public TransactionDto Transaction { get; set; }
        public FundAccountDto FundAccount { get; set; } // Thông tin quỹ tài khoản


    }
 
}