﻿using Redis.OM;
using Redis.OM.Modeling;
using section5._4;

var provider = new RedisConnectionProvider("redis://localhost:6379");

provider.Connection.DropIndexAndAssociatedRecords(typeof(Sale));
provider.Connection.DropIndexAndAssociatedRecords(typeof(Employee));

await provider.Connection.CreateIndexAsync(typeof(Sale));
await provider.Connection.CreateIndexAsync(typeof(Employee));

var employees = provider.RedisCollection<Employee>();

var alice = new Employee
{
    Name = "Alice",
    Age = 45,
    Address = new Address { StreetAddress = "Elm Street", Location = new GeoLoc(-81.957, 27.058), PostalCode = "34269" },
    Sales = new List<string>()
};
var bob = new Employee
{
    Name = "Bob",
    Age = 60,
    Address = new Address() { StreetAddress = "Bleecker Street", Location = new GeoLoc(-74.003, 40.732), PostalCode = "10014" },
    Sales = new List<string>()
};

await employees.InsertAsync(alice);
var bobKey = await employees.InsertAsync(bob);

// updating single document
var sales = provider.RedisCollection<Sale>();
var saleInsertTasks = new List<Task<string>>();
var random = new Random();

for (var i = 0; i < 500; i++)
{
    saleInsertTasks.Add(sales.InsertAsync(new Sale
    {
        Total = random.Next(1000, 30000),
        EmployeeId = bob.Id
    }));
}

await Task.WhenAll(saleInsertTasks);

bob.Sales.AddRange(saleInsertTasks.Select(x => x.Result.Split(":")[1]));

await employees.UpdateAsync(bob);

var bobFromDb = employees.FindById(bob.Id!);
Console.WriteLine($"Bob has: {bobFromDb!.Sales!.Count} sales");

// updating multiple documents
await foreach (var employee in employees)
{
    Console.WriteLine($"{employee.Name} old age: {employee.Age}");
    employee.Age++;
}
employees.Save();

var aliceFromDb = await employees.FirstAsync(x => x.Name == "Alice");
Console.WriteLine($"Alice's new age: {string.Join(", ", aliceFromDb.Age)}");

// deleting documents
await employees.DeleteAsync(alice);
Console.WriteLine($"Does Alice exists in db: {await employees.AnyAsync(x => x.Name == "Alice")}");

await provider.Connection.UnlinkAsync(bobKey);
Console.WriteLine($"Does Bob's exists in db: {await employees.AnyAsync(x => x.Name == "Bob")}");
