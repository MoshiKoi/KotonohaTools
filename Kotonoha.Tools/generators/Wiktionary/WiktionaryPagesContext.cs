using Microsoft.EntityFrameworkCore;

namespace Kotonoha.Tools;

class Page
{
    public int PageId { get; set; }
    public required string Title { get; set; }
    public required string Content { get; set; }
}

class WiktionaryPagesContext : DbContext
{
    public DbSet<Page> Pages { get; set; } = default!;

    public WiktionaryPagesContext(DbContextOptions options) : base(options) { }
}