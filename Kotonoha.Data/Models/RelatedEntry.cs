namespace Kotonoha.Data;

public class RelatedEntry
{
    public int RelatedEntryId { get; set; }
    public required Subentry Parent { get; set; }
    public required Entry Other { get; set; }
    public Subentry? OtherSubentry { get; set; }
}