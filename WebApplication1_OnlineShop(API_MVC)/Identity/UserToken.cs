using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1_API_MVC_.Identity
{
    public class UserToken
    {
        [Key]
        public int Id { get; set; }
        [Column(TypeName = "nvarchar(max)")]
        public string Token { get; set; }
        public string UserId { get; set; }
        public DateTime Expiration { get; set; }
        public bool IsRevoked { get; set; }

        public ApplicationUser User { get; set; }
    }

}
