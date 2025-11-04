namespace TaskManager.Application.DTOs.ChatDTOs;

public class ChatReadDto
{
    public Guid Id { get; set; }
    public List<Guid> Members { get; set; } = new List<Guid>();
    public List<MessageReadDto> Messages { get; set; } = new List<MessageReadDto>();
}