using Avtotest.Web.Validation;
using System.ComponentModel.DataAnnotations;
using PhoneAttribute = Avtotest.Web.Validation.PhoneAttribute;

namespace Avtotest.Web.Models
{
    public class User
    {
        public int Index { get; set; }
        [Required]
        [StringLength(20,ErrorMessage ="MaxLenth must not be more than 20")]
        public string? Name { get; set; }

        [Phone]
        public string? Phone { get; set; }
        [Password(7)]
        public string? Password { get; set; }
        public string? Image { get; set; }
        public IFormFile? ImageFile { get; set; }
    }
}
