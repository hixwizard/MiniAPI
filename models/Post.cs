using System.ComponentModel.DataAnnotations;

public class Post
{
    public int Id { get; set; }

    [MaxLength(100)]
    public string Title { get; set; } = "";

    [MaxLength(500)]
    public string Content { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    //связываю с моделью комментария 1:M
    public List<Comment> Comments { get; set; } = new();
}