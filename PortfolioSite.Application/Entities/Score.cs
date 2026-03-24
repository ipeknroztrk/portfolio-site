public class Score
{
    public int Id { get; set; }
    public string PlayerName { get; set; } = "";
    public int Points { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}