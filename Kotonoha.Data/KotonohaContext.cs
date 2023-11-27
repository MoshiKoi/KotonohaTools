namespace Kotonoha.Data;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public class KotonohaContext : DbContext
{
    public DbSet<Entry> Entries { get; set; } = default!;
    public DbSet<Subentry> Subentries { get; set; } = default!;
    public DbSet<RelatedEntry> RelatedEntries { get; set; } = default!;
    public DbSet<Gloss> Glosses { get; set; } = default!;
    public DbSet<EntryForm> Forms { get; set; } = default!;
    public DbSet<EntryReading> Readings { get; set; } = default!;
    public DbSet<Citation> Citations { get; set; } = default!;

    public KotonohaContext(DbContextOptions<KotonohaContext> options) : base(options) { }
}

class KotonohaContextDesignTimeFactory : IDesignTimeDbContextFactory<KotonohaContext>
{
    public KotonohaContext CreateDbContext(string[] args)
    {
        var connectionStringBuilder = new SqliteConnectionStringBuilder()
        {
            DataSource = "dummy.db",
        };

        var optionsBuilder = new DbContextOptionsBuilder<KotonohaContext>();
        optionsBuilder.UseSqlite(connectionStringBuilder.ConnectionString);

        return new KotonohaContext(optionsBuilder.Options);
    }
}