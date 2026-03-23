using Abp.Domain.Entities; 

namespace pbt.Entities
{
    public class ComplaintReason : Entity<int>
    {
        public string Name { get; set; } // Tên lý do khiếu nại
        public string Description { get; set; } // Mô tả chi tiết
    }
}
