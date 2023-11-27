namespace Kotonoha.Data;

public class EntryReading
{
    public int EntryReadingId { get; set; }
    public required string Reading { get; set; }
    public Entry Entry { get; set; } = default!;
    public string? Restrict { get; set; }
}