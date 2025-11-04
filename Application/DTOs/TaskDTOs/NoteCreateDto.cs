namespace TaskManager.Application.DTOs;

public class NoteCreateDto
{
    public string HeadOfNote { get; set; }
    public string BodyOfNote { get; set; }
    public DateTime DateOfDeadLine { get; set; }
    public List<IFormFile> Images { get; set; }
}