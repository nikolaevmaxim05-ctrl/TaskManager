using TaskManager.Application.DTOs;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Interfaces;

public interface INoteService
{
    public Task<List<NoteReadDto>> GetAllMyNoteAsync (HttpContext context);
    public Task<NoteReadDto> GetNoteByIdAsync(HttpContext context, Guid noteId);
    public Task DeleteNoteByIdAsync(HttpContext context, Guid noteId);
    public Task UpdateNoteAsync(HttpContext context, NoteUpdateDto note, Guid noteId);
    public Task CreateNoteAsync(HttpContext context, NoteCreateDto note);
}