namespace TaskManager.Application.DTOs.ChatDTOs;

public class MessageCreateDto
{
    public Guid Sender { get; set; }
    public string Body { get; set; }
}