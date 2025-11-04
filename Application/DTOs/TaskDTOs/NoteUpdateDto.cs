using TaskManager.Domain.ValueObjects;

namespace TaskManager.Application.DTOs;

public class NoteUpdateDto
{
    public string HeadOfNote { get; set; }
    public string BodyOfNote { get; set; }
    public DateTime DateOfDeadLine { get; set; }
    public NoteStatus Status { get; set; }
    public List<IFormFile> Images { get; set; }
    public List<string> ExistingImages { get; set; } 
}
