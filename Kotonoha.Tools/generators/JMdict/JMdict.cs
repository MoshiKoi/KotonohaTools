using System.Xml;

using Kotonoha.Data;

namespace Kotonoha.Tools;

static class JMdict
{
    public static async Task ParseAsync(FileInfo inFile, FileInfo outFile)
    {
        using var context = ContextUtils.CreateKotonohaContext(outFile.FullName);
        await context.Database.EnsureCreatedAsync();

        var citation = new Citation
        {
            Name = "JMdict",
            Description = """Sourced from JMdict under a <a href="https://www.edrdg.org/edrdg/licence.html">Creative Commons Attribution-ShareAlike Licence (V4.0)</a>"""
        };

        context.Citations.Add(citation);

        var nameTable = new NameTable();
        var entryNt = nameTable.Add("entry");
        var glossNt = nameTable.Add("gloss");
        var senseNt = nameTable.Add("sense");
        var posNt = nameTable.Add("pos");
        var kebNt = nameTable.Add("keb");
        var readingElementNt = nameTable.Add("r_ele");
        var rebNt = nameTable.Add("reb");
        var restrictNt = nameTable.Add("re_restr");

        var readerSettings = new XmlReaderSettings()
        {
            Async = true,
            DtdProcessing = DtdProcessing.Parse,
            MaxCharactersFromEntities = 0,
            NameTable = nameTable
        };

        using var fileStream = File.OpenRead(inFile.FullName);
        using var reader = XmlReader.Create(fileStream, readerSettings);

        bool isGloss = false;
        bool isKeb = false;
        bool isReb = false;
        bool isPos = false;
        bool isRestrict = false;

        List<Subentry> subentries = new();
        List<string> forms = new();
        List<EntryReading> readings = new();
        List<string> glosses = new();
        string? currentPos = default;
        string? restrict = default;

        int entryCount = 0;
        int subentryCount = 0;
        while (await reader.ReadAsync())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    if (ReferenceEquals(reader.LocalName, glossNt)) { isGloss = true; break; }
                    if (ReferenceEquals(reader.LocalName, kebNt)) { isKeb = true; break; }
                    if (ReferenceEquals(reader.LocalName, rebNt)) { isReb = true; break; }
                    if (ReferenceEquals(reader.LocalName, posNt)) { isPos = true; break; }
                    if (ReferenceEquals(reader.LocalName, restrictNt)) { isRestrict = true; break; }
                    break;

                case XmlNodeType.EndElement:
                    if (ReferenceEquals(reader.LocalName, entryNt))
                    {
                        var entry = new Entry
                        {
                            Subentries = subentries,
                            Forms = forms.Select(x => new EntryForm { Form = x }).ToList(),
                            Readings = readings,
                        };

                        context.Add(entry);
                        subentryCount += subentries.Count;

                        subentries = new();
                        forms = new();
                        readings = new();
                        ++entryCount;

                        if (entryCount % 3000 == 0)
                        {
                            Console.WriteLine($"{entryCount} entries parsed (+{subentryCount} subentries)");
                            subentryCount = 0;
                            if (entryCount % 27000 == 0)
                            {
                                Console.WriteLine($"Saving...");
                                await context.SaveChangesAsync();
                            }
                        }
                        break;
                    }
                    if (ReferenceEquals(reader.LocalName, senseNt))
                    {
                        if (currentPos is null) { throw new Exception($"Part of speech is null"); }

                        subentries.Add(new Subentry
                        {
                            PartOfSpeech = currentPos,
                            Glosses = glosses.Select(x => new Gloss { Content = x }).ToList(),
                            Source = citation,
                        });

                        glosses.Clear();
                    }
                    if (ReferenceEquals(reader.LocalName, glossNt)) { isGloss = false; break; }
                    if (ReferenceEquals(reader.LocalName, kebNt)) { isKeb = false; break; }
                    if (ReferenceEquals(reader.LocalName, rebNt)) { isReb = false; break; }
                    if (ReferenceEquals(reader.LocalName, posNt)) { isPos = false; break; }
                    if (ReferenceEquals(reader.LocalName, restrictNt)) { isRestrict = false; break; }
                    break;

                case XmlNodeType.Text:
                    if (isKeb) { forms.Add(reader.Value); break; }
                    if (isReb)
                    {
                        readings.Add(new EntryReading
                        {
                            Reading = reader.Value,
                            Restrict = restrict
                        });
                        break;
                    }
                    if (isRestrict) { restrict = reader.Value; break; }
                    if (isPos) { currentPos = reader.Value; break; }
                    if (isGloss) { glosses.Add(reader.Value); break; }
                    break;

                default:
                    continue;
            }
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"{entryCount} entries parsed");
    }
}