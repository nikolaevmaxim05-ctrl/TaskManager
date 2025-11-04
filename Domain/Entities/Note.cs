using TaskManager.Domain.ValueObjects;

namespace TaskManager.Domain.Entities
{
    public class Note : BaseEntity
    {
        public Guid UserId { get; set; }
        /// <summary>
        /// статус задачи
        /// </summary>
        public NoteStatus Status { get; set; }
        /// <summary>
        /// Заголовок задачи
        /// </summary>
        public string HeadOfNote { get; set; }
        /// <summary>
        /// тело задачи
        /// </summary>
        public string BodyOfNote { get; set; }
        /// <summary>
        /// дата создания задачи
        /// </summary>
        public readonly DateTime DateOfCreate;
        /// <summary>
        /// дата дэд лайна
        /// </summary>
        public DateTime DateOfDeadLine { get; set; }
        
        public List<string> Images { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="noteStatus">статус задачи</param>
        /// <param name="headOfNote">Заголовок задачи</param>
        /// <param name="bodyOfNote">тело задачи</param>
        /// <param name="dateOfCreate">дата создания задачи</param>
        /// <param name="dateOfDeadLine">дата дэд лайна</param>
        public Note(Guid userId,NoteStatus noteStatus, string headOfNote, string bodyOfNote, DateTime dateOfCreate, DateTime dateOfDeadLine, List<string> images)
        {
            UserId = userId;
            Id = Guid.NewGuid();
            Status = noteStatus;
            HeadOfNote = headOfNote;
            BodyOfNote = bodyOfNote;    
            DateOfCreate = dateOfCreate;    
            DateOfDeadLine = dateOfDeadLine;
            Images = images;
        }

        public Note()
        {
                
        }
    }
}
