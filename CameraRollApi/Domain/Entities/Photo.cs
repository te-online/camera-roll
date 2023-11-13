namespace Domain.Entities;

public class Photo
{
  public Guid Id { get; set; } = Guid.NewGuid();
  public bool IsPublic { get; set; } = false;
  public string? Title { get; set; }
  public string? Description { get; set; }
  public string? S3Url { get; set; }
  public string? AuthorId { get; set; }
  public DateTime UpdatedAt { get; set; } = DateTime.Now;
  public DateTime CreatedAt { get; set; } = DateTime.Now;
}