using System;

namespace pbt.Web.Models.Transactions
{
    public class CharingRequest
    {
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; } = DateTime.Now;
        public string Description { get; set; }
        public string ReferenceCode { get; set; } // Mã tham chiếu
        public string Source { get; set; } // Nguồn truy cập (Id thiết bị điện thoại, IP của máy truy cập)
        public int SourceType { get; set; } // IP, Serial, Máy nội bộ ....
        public string Sign {  get; set; }   
    }
}
