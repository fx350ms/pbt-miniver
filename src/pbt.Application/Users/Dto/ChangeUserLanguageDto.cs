using System.ComponentModel.DataAnnotations;

namespace pbt.Users.Dto
{
    public class ChangeUserLanguageDto
    {
        [Required]
        public string LanguageName { get; set; }
    }
}