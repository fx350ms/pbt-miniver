using Abp.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace pbt.Entities
{
    public class ConfigurationSetting : Entity<long>
    {
        /// </summary>
        [Required]
        [MaxLength(256)]
        public string Key { get; set; }

        /// <summary>
        /// The value of the configuration setting
        /// </summary>

        public string Value { get; set; }
    }
}
