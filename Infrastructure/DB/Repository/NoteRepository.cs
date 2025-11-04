using Microsoft.EntityFrameworkCore;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;

namespace TaskManager.Infrastructure.DB.Repository
{
    public class NoteRepository : INoteRepository
    {
        private readonly UserContext _context;
        private readonly ILogger<NoteRepository> _logger;

        public NoteRepository(UserContext context, ILogger<NoteRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> Create(Note entity)
        {
            _logger.LogInformation("Попытка создать задачу {entity}", entity.Id);
            
            await _context.Notes.AddAsync(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Note?> Read(Guid id)
        {
            _logger.LogInformation("Попытка считать с бд задачу по id {id}", id);
            
            return await _context.Notes.FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<Note>> ReadAllByUserId(Guid userId)
        {
            _logger.LogInformation("Попытка считать с бд весь NotePool по id пользователя {userId}", userId);
            
            return await _context.Notes
                .Where(t => t.UserId == userId)   // предполагаем, что в Note есть связь UserId
                .ToListAsync();
        }

        public async Task<IEnumerable<Note>> ReadAll()
        {
            _logger.LogInformation("Попытка считать все задачи с бд");
            
            return await _context.Notes.ToListAsync();
        }
        public async Task<bool> Update(Note updatedNote)
        {
            _logger.LogInformation("Попытка обновить задачу {updatedNote}", updatedNote.Id);
            
            var oldTask = await Read(updatedNote.Id);
            oldTask.DateOfDeadLine = updatedNote.DateOfDeadLine;
            oldTask.HeadOfNote = updatedNote.HeadOfNote;
            oldTask.BodyOfNote = updatedNote.BodyOfNote;
            oldTask.Status = updatedNote.Status;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> Delete(Note note)
        {
            _logger.LogInformation("Попытка удалить задачу {note}", note.Id);
            
            _context.Notes.Remove(note);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}