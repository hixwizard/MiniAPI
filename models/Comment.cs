using System.ComponentModel.DataAnnotations;

public class Comment
{
    public int Id { get; set; }

    [MaxLength(500)]
    public string Content { get; set; } = "";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ── Связь 1:M с постом ──
    public int PostId { get; set; }
    public Post Post { get; set; } = null!;

    // авторство комментария
    public int UserId { get; set; }
    public User User { get; set; } = null!;
}