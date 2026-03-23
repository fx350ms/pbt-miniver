using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace pbt.Dictionary.Dto
{
    public class CreateUpdateDictionaryDto : EntityDto<int>
    {
        [Required]
        public string NameVi { get; set; }

        [Required]
        public string NameCn { get; set; }

        public string Description { get; set; }
    }
}
