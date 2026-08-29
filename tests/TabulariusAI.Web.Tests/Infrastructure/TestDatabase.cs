using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TabulariusAI.Web.Data;

namespace TabulariusAI.Web.Tests.Infrastructure;

/// <summary>Provides an isolated relational SQLite database for integration tests.</summary>
public sealed class TestDatabase : IAsyncDisposable
{
    private readonly SqliteConnection connection;

    /// <summary>Initializes a new isolated in-memory SQLite database and creates the application schema.</summary>
    public TestDatabase()
    {
        connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        var options = new DbContextOptionsBuilder<TabulariusDbContext>().UseSqlite(connection).Options;
        Context = new TabulariusDbContext(options);
        Context.Database.EnsureCreated();
    }

    /// <summary>Gets the database context associated with this test database.</summary>
    public TabulariusDbContext Context { get; }

    /// <summary>Disposes the database context and its underlying SQLite connection.</summary>
    public async ValueTask DisposeAsync()
    {
        await Context.DisposeAsync();
        await connection.DisposeAsync();
    }
}
