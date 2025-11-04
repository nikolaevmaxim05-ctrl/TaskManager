using TaskManager.Application.DTOs.ChatDTOs;
using TaskManager.Domain.Entities;
using TaskManager.Domain.ValueObjects;

namespace TaskManager.Application.Mappers;

public static class ChatMapper
{
    public static Chat ToDomain(this ChatCreateDto dto, List<User> members)
        => new()
        {
            Id = Guid.NewGuid(),
            Members = members,
            Messages = new List<Message>()
        };

    public static ChatReadDto ToReadDto(this Chat chat)
    {
        var dto = new ChatReadDto();

        chat.Members.ForEach(user => dto.Members.Add(user.Id));

        dto.Messages = new List<MessageReadDto>();
        

        foreach (var i in chat.Messages)
        {
             dto.Messages.Add(i.ToReadDto());
        }

        dto.Id = chat.Id;
        
        return dto;
    }
}