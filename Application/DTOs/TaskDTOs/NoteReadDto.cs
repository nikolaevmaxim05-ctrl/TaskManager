using TaskManager.Domain.ValueObjects;

namespace TaskManager.Application.DTOs;

public class NoteReadDto
{
    public Guid Id { get; set; }
    public string HeadOfNote { get; set; }
    public string BodyOfNote { get; set; }
    public DateTime DateOfDeadLine { get; set; }
    public NoteStatus Status { get; set; }
    public DateTime DateOfCreate { get; set; }
    public List<string> Images { get; set; }
}