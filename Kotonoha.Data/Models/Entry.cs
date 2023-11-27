using System.ComponentModel.DataAnnotations.Schema;

namespace Kotonoha.Data;

public class Entry
{
    public int EntryId { get; set; }

    [InverseProperty("Entry")]
    public required List<EntryForm> Forms { get; set; }

    [InverseProperty("Entry")]
    public required List<EntryReading> Readings { get; set; }

    [InverseProperty("Entry")]
    public required List<Subentry> Subentries { get; set; }
}