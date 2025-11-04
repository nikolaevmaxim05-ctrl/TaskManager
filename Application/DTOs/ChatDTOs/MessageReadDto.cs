namespace TaskManager.Application.DTOs.ChatDTOs;

public class MessageReadDto
{
    public Guid Id { get; set; }
    public Guid Sender { get; set; }
    public string Body { get; set; }
    public DateTime SendTime { get; set; }
}