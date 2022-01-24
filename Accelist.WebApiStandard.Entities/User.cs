using System.ComponentModel.DataAnnotations;

namespace Accelist.WebApiStandard.Entities
{
    public class User
    {
        [Key]
        public string? UserID { set; get; }

        [Required]
        public string? UserName { set; get; }

        [Required]
        public string? FullName { set; get; }

        [Required]
        public string? Password { set; get; }
    }
}