namespace TaskManager.Domain.Entities;

public class UserConfirmation : BaseEntity
{
    public string Email { get; set; } 
    public string Token { get; set; } 
    public DateTime Expiration { get; set; }
}