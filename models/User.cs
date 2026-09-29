using System.ComponentModel.DataAnnotations;

public class User
{
    public int Id { get; set; }

    [MaxLength(50)]
    public string Name { get; set; } = "";
    [MaxLength(200)]
    public string Email { get; set; } = "";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    //связываю с моделью комментария 1:M (автор комментариев)
    public List<Comment> Comments { get; set; } = new();
}