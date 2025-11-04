using System.ComponentModel.DataAnnotations.Schema;
using BCrypt.Net;

namespace TaskManager.Domain.ValueObjects
{
    public class AuthorizationParams
    {
        public string EMail
        { get; set; }
        public string PasswordHash { get; set; }
        public AuthorizationParams(string email, string passwordHash)
        {
            EMail = email;
            PasswordHash = passwordHash;
        }
        public AuthorizationParams()
        {
                
        }
    }
}
