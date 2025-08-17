namespace Finora.Messages
{
    public abstract class EntityBaseDTO
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
