using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using Kotonoha.Data;

namespace Kotonoha.Tools;

static class ContextUtils
{
    public static KotonohaContext CreateKotonohaContext(string path)
    {
        var connectionStringBuilder = new SqliteConnectionStringBuilder()
        {
            DataSource = path,
            Mode = SqliteOpenMode.ReadWriteCreate,
        };

        var optionsBuilder = new DbContextOptionsBuilder<KotonohaContext>()
            .UseSqlite(connectionStringBuilder.ConnectionString);

        var contextFactory = new KotonohaContext(optionsBuilder.Options);

        return contextFactory;
    }
}