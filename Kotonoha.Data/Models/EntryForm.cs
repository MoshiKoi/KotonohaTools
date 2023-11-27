namespace Kotonoha.Data;

public class EntryForm
{
    public int EntryFormId { get; set; }
    public required string Form { get; set; }
    public Entry Entry { get; set; } = default!;
}