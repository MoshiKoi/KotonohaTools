using System.ComponentModel.DataAnnotations.Schema;

namespace Kotonoha.Data;

public class Subentry
{
    public int SubentryId { get; set; }
    public Entry Entry { get; set; } = default!;
    public required string PartOfSpeech { get; set; }
    public List<Gloss> Glosses { get; set; } = new();
    public required Citation Source { get; set; }
    public string? Info { get; set; }

    [InverseProperty("Parent")]
    public List<RelatedEntry> Related { get; set; } = new();
}