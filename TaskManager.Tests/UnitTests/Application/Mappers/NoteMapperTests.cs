using TaskManager.Application.DTOs;
using TaskManager.Application.Mappers;
using Xunit;

namespace TaskManager.TaskManager.Tests.Application.Mappers;

public class NoteMapperTests
{
    [Fact]
    public void NoteMapperToDomain_IsChangedId()
    {
        var dto = new NoteCreateDto();

        var note = dto.ToDomain(Guid.NewGuid());
        
        Assert.True(note.Id != Guid.Empty);
    }

}   