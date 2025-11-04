using TaskManager.Domain.ValueObjects;

namespace TaskManager.Domain.Entities
{
    /// <summary>
    /// обычный пользователь
    /// </summary>
    public class User : BaseEntity
    {
        public UserStats UserStats { get; set; }
        public AuthorizationParams AuthorizationParams { get; set; }
        public List<Note> NotePool { get; set; } = new List<Note>();
        public FriendList FriendList { get; set; } = new FriendList();
        public List<Notification> Notifications { get; set; } = new List<Notification>();
        public List<Chat> Chats { get; set; } = new List<Chat>();
        public User(AuthorizationParams authorizationParams) 
        {
            AuthorizationParams = authorizationParams;
            Id = Guid.NewGuid();
        }
        public User(AuthorizationParams authorizationParams, List<Note> notePool, UserStats userStats, FriendList friendList, List<Notification> notifications, List<Chat> chats)
        {
            Chats = chats;
            AuthorizationParams = authorizationParams;
            Id = Guid.NewGuid();
            NotePool = notePool;
            UserStats = userStats;
            FriendList = friendList;
            Notifications = notifications;
        }
        public User()
        {
            
        }
        public override int GetHashCode()
        {
            return AuthorizationParams.GetHashCode();
        }
        /// <summary>
        /// сравниваем переменные User по автроизационным параметрам
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object? obj)
        {
            User? user = obj as User;    
            if (user == null) return false;
            return user.AuthorizationParams.Equals(AuthorizationParams);
        }
    }
}
