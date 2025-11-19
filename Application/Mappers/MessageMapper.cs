using TaskManager.Application.DTOs.ChatDTOs;
using TaskManager.Domain.Entities;
using TaskManager.Domain.ValueObjects;

namespace TaskManager.Application.Mappers;

public static class MessageMapper
{
    public static Message ToDomain(this MessageCreateDto dto, User sender, Chat chat)
        => new()
        {
            Id = Guid.NewGuid(),
            Sender = sender,
            Body = dto.Body,
            SendTime = DateTime.UtcNow,
            Chat = chat,
            ChatId = chat.Id
        };

    public static MessageReadDto ToReadDto(this Message message) 
        => new() 
        { 
            Id = message.Id,
            Body = message.Body, 
            SendTime = message.SendTime, 
            Sender = message.Sender.Id 
        };
    public static void ApplyUpdate (this Message message, MessageUpdateDto dto)
    {
        message.Body = dto.Body;
    }
}