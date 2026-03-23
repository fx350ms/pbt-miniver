using Abp.Domain.Entities.Auditing;
using System.ComponentModel.DataAnnotations;

namespace pbt.Entities
{

    public class Dictionary :  FullAuditedEntity<int>
    {
        [Required]
        public string NameVi { get; set; }

        [Required]
        public string NameCn { get; set; }

        public string Description { get; set; }
    }

}
