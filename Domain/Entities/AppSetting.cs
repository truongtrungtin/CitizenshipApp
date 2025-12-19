namespace Domain.Entities;

public class AppSetting
{
    public int Id { get; set; }
    public string Key { get; set; } = default!;
    public string Value { get; set; } = default!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
