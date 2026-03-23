using Abp.Domain.Entities;
using System.Collections.Generic;


namespace pbt.Entities
{
    //Nhóm ngành hàng hóa
    public class ProductGroupType : Entity<int>
    {
        public string Name { get; set; }     // Ví dụ: Hàng phổ thông, Hóa chất, Hàng fake
        public string Note { get; set; }     // Ghi chú bổ sung (nếu có)
        public bool IsDefault { get; set; }

    }
}
