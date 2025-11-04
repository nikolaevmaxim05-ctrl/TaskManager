namespace TaskManager.Domain.Entities
{
    /// <summary>
    /// базовый класс для всез сущностей, который позволяет им объеденятся по свойству иметь собсвенный id
    /// </summary>
    public abstract class BaseEntity
    {
        /// <summary>
        /// id любой сущности
        /// </summary>
        public Guid Id { get; set; }
    }
}
