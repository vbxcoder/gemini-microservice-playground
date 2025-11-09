using System.ComponentModel.DataAnnotations;

namespace AuthenticationService.Models
{
    public class User
    {
        [Key]
        public int IdUser { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
    }
}
