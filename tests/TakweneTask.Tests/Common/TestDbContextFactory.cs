using Microsoft.EntityFrameworkCore;
using TakweneTask.Application.Common.Interfaces;
using TakweneTask.Infrastructure.Persistence;

namespace TakweneTask.Tests.Common;

public static class TestDbContextFactory
{
    public static AppDbContext Create(string? databaseName = null, bool ensureCreated = false)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: databaseName ?? Guid.NewGuid().ToString())
            .Options;

        var context = new AppDbContext(options);
        if (ensureCreated)
        {
            context.Database.EnsureCreated();
        }

        return context;
    }

    public static IUnitOfWork CreateUnitOfWork(AppDbContext? context = null)
    {
        var db = context ?? Create();
        return new UnitOfWork(db);
    }
}
