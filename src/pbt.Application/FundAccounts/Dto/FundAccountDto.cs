using Abp.Application.Services.Dto;

namespace pbt.FundAccounts.Dto
{
    public class FundAccountDto : FullAuditedEntityDto<int>
    {
        public int AccountType { get; set; } // Tiền mặt/Ngân hàng
        public string AccountName { get; set; }
        public string Currency { get; set; }
        public string BankName { get; set; }
        public string AccountNumber { get; set; }
        public string AccountHolder { get; set; }
        public string Notes { get; set; }
        public bool IsActived { get; set; }
        public decimal TotalAmount { get; set; } // Số dư của quỹ
    }
}