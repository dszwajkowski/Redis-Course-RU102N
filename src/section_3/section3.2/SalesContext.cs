using Microsoft.EntityFrameworkCore;

namespace section3._2;

public class SalesContext : DbContext
{
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Sale> Sales { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options) =>
        options.UseInMemoryDatabase("Sales.db");
}