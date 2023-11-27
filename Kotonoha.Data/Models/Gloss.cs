namespace Kotonoha.Data;

public class Gloss
{
    public int GlossId { get; set; }
    public bool Primary { get; set; } = false;
    public string GlossLanguage { get; set; } = "eng";
    public required string Content { get; set; }
}