using Kotonoha.Data;

namespace Kotonoha.Tools;

static class UniDic
{
    public static async Task Parse(FileInfo lexFile, FileInfo licenceFile, FileInfo outFile)
    {

        using var context = ContextUtils.CreateKotonohaContext(outFile.FullName);
        await context.Database.EnsureCreatedAsync();

        var citation = new Citation
        {
            Name = "UniDic",
            Description = await File.ReadAllTextAsync(licenceFile.FullName)
        };

        context.Citations.Add(citation);

        using var reader = new StreamReader(lexFile.FullName);

        while (await reader.ReadLineAsync() is string line)
        {
            var parts = line.Split(',')
                .Select(part => part.Trim())
                .ToArray();

            // https://clrd.ninjal.ac.jp/unidic/faq.html

            var term = parts[0];
            var pos1 = parts[4];
            var pos2 = parts[5];
            var pos3 = parts[6];
            var pos4 = parts[7];
            var cType = parts[8];
            var cForm = parts[9];
            var lForm = parts[10];
            var lemma = parts[11];
            var orth = parts[12];
            var pron = parts[13];
            var orthBase = parts[14];
            var pronBase = parts[15];
            var goshu = parts[16];
            var iType = parts[17];
            var iForm = parts[18];
            var fType = parts[19];
            var fForm = parts[20];

            var pitchAccent = parts[28];

            Console.WriteLine($"{term} ({lemma}): {pitchAccent}");
        }
    }
}