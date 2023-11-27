using System.CommandLine;

namespace Kotonoha.Tools;

partial class Program
{
    static async Task<int> Main(string[] args)
    {
        var dbArg = new Argument<FileInfo>("database", "database file");

        var jmdictArg = new Argument<FileInfo>("file", "jmdict xml file");
        var jmdictCommand = new Command("jmdict") { jmdictArg, dbArg };

        jmdictCommand.SetHandler(JMdict.ParseAsync, jmdictArg, dbArg);

        var wiktionaryArg = new Argument<FileInfo>("file", "wiktionary xml dump file");
        var wiktionaryDbArg = new Argument<FileInfo>("db", "wiktionary pages db");

        var wiktionaryCommand = new Command("enwiktionary1") { wiktionaryArg, wiktionaryDbArg };
        var wiktionary2Command = new Command("enwiktionary2") { wiktionaryDbArg, dbArg };

        wiktionaryCommand.SetHandler(Wiktionary.EnWiktionary1, wiktionaryArg, dbArg);
        wiktionary2Command.SetHandler(Wiktionary.EnWiktionary2, wiktionaryDbArg, dbArg);

        var unidicLexArg = new Argument<FileInfo>("file", "unidic lex csv file");
        var unidicLicenseArg = new Argument<FileInfo>("license file", "unidic license file"); 
        var unidicCommand = new Command("unidic") { unidicLexArg, unidicLicenseArg, dbArg };

        unidicCommand.SetHandler(UniDic.Parse, unidicLexArg, unidicLicenseArg, dbArg);

        var rootCommand = new RootCommand("Kotonoha resource generator")
        {
            jmdictCommand,
            wiktionaryCommand,
            wiktionary2Command,
            unidicCommand,
        };

        return await rootCommand.InvokeAsync(args);
    }
}