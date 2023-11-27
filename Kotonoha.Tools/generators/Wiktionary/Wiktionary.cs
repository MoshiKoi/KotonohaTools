using System.Xml;
using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using Kotonoha.Data;

namespace Kotonoha.Tools;

partial class Wiktionary
{
    private static WiktionaryPagesContext CreateWiktionaryPagesContext(string path)
    {
        var connectionStringBuilder = new SqliteConnectionStringBuilder()
        {
            Mode = SqliteOpenMode.ReadWriteCreate,
            DataSource = path
        };

        var contextOptions = new DbContextOptionsBuilder<WiktionaryPagesContext>()
            .UseSqlite(connectionStringBuilder.ConnectionString)
            .Options;

        return new WiktionaryPagesContext(contextOptions);
    }

    public static async Task EnWiktionary2(FileInfo wiki1db, FileInfo outFile)
    {
        using var context = ContextUtils.CreateKotonohaContext(outFile.FullName);

        await context.Database.EnsureCreatedAsync();

        var citation = new Citation
        {
            Name = "Wiktionary",
            Description = """Sourced from Wiktionary under a <a href="https://en.wiktionary.org/wiki/Wiktionary:Text_of_Creative_Commons_Attribution-ShareAlike_3.0_Unported_License">Creative Commons Attribution-ShareAlike Licence</a>"""
        };

        context.Citations.Add(citation);
        await context.SaveChangesAsync();

        using var wikiContext = CreateWiktionaryPagesContext(wiki1db.FullName);

        foreach (var page in wikiContext.Pages)
        {
            foreach (var match in JapaneseNounRegex().Matches(page.Content).AsEnumerable())
            {
                var reading = match.Groups[1].Value;
                var glosses = (IEnumerable<string>)match.Groups[2].Captures
                     .Select(capture => ParseGloss(capture.Value))
                     .Where(gloss => gloss != null);

                context.Entries.Add(new Entry
                {
                    Forms = new() { new EntryForm { Form = page.Title } },
                    Readings = new() { new EntryReading { Reading = reading } },
                    Subentries = glosses.Select(gloss => new Subentry
                    {
                        Source = citation,
                        PartOfSpeech = "noun",
                        Glosses = new() { new Gloss { Content = gloss } }
                    }).ToList()
                });
            }
        }

        await context.SaveChangesAsync();
    }

    public static Task EnWiktionary1(FileInfo inFile, FileInfo outFile) => Wiktionary1(inFile, outFile, EnParse);

    private static async Task Wiktionary1(FileInfo inFile, FileInfo outFile, Func<string, string?> ContentParser)
    {
        using (var context = CreateWiktionaryPagesContext(outFile.FullName))
        {
            await context.Database.EnsureCreatedAsync();
        }

        var nameTable = new NameTable();
        var pageNt = nameTable.Add("page");
        var revisionNt = nameTable.Add("revision");
        var titleNt = nameTable.Add("title");
        var textNt = nameTable.Add("text");

        var readerSettings = new XmlReaderSettings()
        {
            Async = true,
            NameTable = nameTable
        };

        bool isPage = false;
        bool isTitle = false;
        bool isRevision = false;
        bool isRevisionContent = false;
        bool isLatestRevision = false;
        string title = "";

        using var fileStream = File.OpenRead(inFile.FullName);
        using var reader = XmlReader.Create(fileStream, readerSettings);

        var entryCount = 0;
        var entryDelta = 0;
        var pageCount = 0;
        while (await reader.ReadAsync())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    if (ReferenceEquals(reader.LocalName, pageNt)) { isPage = true; isLatestRevision = true; ++pageCount; }
                    else if (isPage && ReferenceEquals(reader.LocalName, titleNt)) { isTitle = true; }
                    else if (isPage && isLatestRevision && ReferenceEquals(reader.LocalName, revisionNt)) { isRevision = true; }
                    else if (isRevision && ReferenceEquals(reader.LocalName, textNt)) { isRevisionContent = true; }
                    break;

                case XmlNodeType.EndElement:
                    if (ReferenceEquals(reader.LocalName, pageNt)) { isPage = false; }
                    else if (ReferenceEquals(reader.LocalName, titleNt)) { isTitle = false; }
                    else if (ReferenceEquals(reader.LocalName, revisionNt)) { isRevision = false; isLatestRevision = false; }
                    else if (ReferenceEquals(reader.LocalName, textNt)) { isRevisionContent = false; }
                    break;

                case XmlNodeType.Text:
                    var value = await reader.GetValueAsync();

                    if (isTitle) { title = value; }
                    else if (isRevisionContent && isLatestRevision)
                    {
                        var content = ContentParser(value);
                        if (content != null)
                        {
                            using var context = CreateWiktionaryPagesContext(outFile.FullName);
                            context.Pages.Add(new Page
                            {
                                Title = title,
                                Content = content
                            });
                            await context.SaveChangesAsync();
                            ++entryDelta;
                        }

                        if (++pageCount % 3000 == 0)
                        {
                            Console.WriteLine($"{pageCount} pages parsed (+{entryDelta} entries)");
                            entryCount += entryDelta;
                            entryDelta = 0;
                        }
                    }
                    break;
                default:
                    break;
            }
        }
        entryCount += entryDelta;
        Console.WriteLine($"{pageCount} pages parsed, {entryCount} total entries");
        Console.WriteLine("Finished parsing raw pages");
    }

    // Get the entire Japanese section from a wiktionary page
    [GeneratedRegex("\n==Japanese==\n.+?\n==[^=]+?==\n", RegexOptions.Singleline)]
    private static partial Regex JapaneseWikiRegex();

    // Read the noun section
    [GeneratedRegex(@"{{ja-noun\|(.+?)}}\n\n(?:# (.+?)\n|.+?\n)+")]
    private static partial Regex JapaneseNounRegex();

    // Read the definitions
    [GeneratedRegex(@"(?<=^\#).+$", RegexOptions.Multiline)]
    private static partial Regex JapaneseGlossRegex();

    private static string? EnParse(string content)
        => JapaneseWikiRegex().Match(content) is { Success: true, Value: var value }
            ? value
            : null;

    [GeneratedRegex(@"\[\[(?:[^|]+?\|)?(.+?)\]\]")]
    private static partial Regex WikilinkRegex();

    [GeneratedRegex(@"{{([^|]+?)(?:\|(?:([^|]+?)=([^|]+?)|([^|]+?)))*}}")]
    private static partial Regex TemplateRegex();

    private static readonly Dictionary<string, Func<IEnumerable<string>, Dictionary<string, string>, string>> templates = new()
    {
        { "lb", (labels, _) => $"({labels.Last()})" }
    };

    // Handles Wikitext and so on - if it doesn't know how to parse it, it should return null
    private static string? ParseGloss(string content)
    {
        var result = WikilinkRegex().Replace(content, match => match.Groups[1].Value);
        try
        {
            result = TemplateRegex().Replace(result, match =>
            {
                if (templates.TryGetValue(match.Groups[1].Value, out var fn))
                {
                    var dict = Enumerable.Zip(
                        match.Groups[2].Captures.Select(capture => capture.Value),
                        match.Groups[3].Captures.Select(capture => capture.Value))
                        .ToDictionary(x => x.First, x => x.Second);

                    return fn(match.Groups[4].Captures.Select(capture => capture.Value), dict);
                }
                else
                    return match.Value;
            });
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
        }

        // We don't want to have random formatting chars in the result
        if (result.All(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c) || char.IsPunctuation(c)))
        {
            return result;
        }

        Console.WriteLine($"Failed to parse gloss:\n\tContent: {content}\n\tResult: {result}");
        return null;
    }
}