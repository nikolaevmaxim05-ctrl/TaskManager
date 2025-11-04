using TaskManager.Application.DTOs;
using TaskManager.Domain.Entities;
using TaskManager.Domain.ValueObjects;

namespace TaskManager.Application.Mappers;

public static class NoteMapper
{
    public static Note ToDomain(this NoteCreateDto dto, Guid userId)
    {
        var images = new List<string>();
        if (dto.Images != null)
        {
            images = dto.Images.Select(u => u.FileName).ToList();
        }
        return new Note
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            BodyOfNote = dto.BodyOfNote,
            HeadOfNote = dto.HeadOfNote,
            DateOfDeadLine = dto.DateOfDeadLine,
            Status = NoteStatus.InProgress,
            Images = images
        };
    }

    public static void ApplyUpdate(this Note note, NoteUpdateDto dto, List<string> addedImg)
    {
        note.HeadOfNote = dto.HeadOfNote;
        note.BodyOfNote = dto.BodyOfNote;
        note.DateOfDeadLine = dto.DateOfDeadLine;
        note.Status = dto.Status;

        // ✅ вместо пересоздания — обновляем существующую коллекцию
        note.Images.Clear();

        if (dto.ExistingImages != null)
            note.Images.AddRange(dto.ExistingImages);

        if (addedImg != null)
            note.Images.AddRange(addedImg);
    }


    public static NoteReadDto ToReadDto(this Note note) 
        => new()
    {
        Id = note.Id,
        BodyOfNote = note.BodyOfNote,
        HeadOfNote = note.HeadOfNote,
        DateOfCreate = note.DateOfCreate,
        DateOfDeadLine = note.DateOfDeadLine,
        Status = note.Status,
        Images = note.Images
    };
}