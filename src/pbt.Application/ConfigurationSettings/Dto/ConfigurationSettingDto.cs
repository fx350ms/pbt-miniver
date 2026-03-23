using Abp.Application.Services.Dto;
using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pbt.ConfigurationSettings.Dto
{
    public class ConfigurationSettingDto : EntityDto<long>
    {

      
        [Required]
        [MaxLength(256)]
        public string Key { get; set; }

        /// <summary>
        /// The value of the configuration setting
        /// </summary>

        public string Value { get; set; }
    }
}
