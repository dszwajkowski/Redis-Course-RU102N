using Redis.OM;
using Redis.OM.Modeling;
using section5._3;

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
    Address = new Address { StreetAddress = "Elm Street", Location = new GeoLoc(-81.957, 27.058), PostalCode = "34269" }
};

var bob = new Employee
{
    Name = "Bob",
    Age = 60,
    Address = new Address() { StreetAddress = "Bleecker Street", Location = new GeoLoc(-74.003, 40.732), PostalCode = "10014" }
};

var charlie = new Employee
{
    Name = "Charlie",
    Age = 26,
    Address = new Address() { StreetAddress = "Ocean Boulevard", Location = new GeoLoc(-121.869, 36.604), PostalCode = "93940" }
};

var dan = new Employee
{
    Name = "Dan",
    Age = 42,
    Address = new Address() { StreetAddress = "Baker Street", Location = new GeoLoc(-0.158, 51.523), PostalCode = "NW1 6XE" }
};

var yves = new Employee
{
    Name = "Yves",
    Age = 19,
    Address = new Address() { StreetAddress = "Rue de Rivoli", Location = new GeoLoc(2.361, 48.863), PostalCode = "75003" }
};

await employees.InsertAsync(bob);
await employees.InsertAsync(alice);
await employees.InsertAsync(charlie);
await employees.InsertAsync(dan);
await employees.InsertAsync(yves);

// find the first employee in Redis named "Bob",
var employeeNamedBob = await employees.FirstAsync(x => x.Name == "Bob");
Console.WriteLine($"Bob's age: {employeeNamedBob.Age}, address: {employeeNamedBob.Address?.StreetAddress}, {employeeNamedBob.Address?.PostalCode} ({employeeNamedBob.Address?.Location}).");

// age query
var employeesUnderForty = await employees.Where(x => x.Age < 40).ToListAsync();
Console.WriteLine($"Employees under 40: {string.Join(", ", employeesUnderForty.Select(x => x.Name))}.");

// geo query
var employeesWithin500Km = await employees
    .GeoFilter(x => x.Address!.Location, -75.159, 39.963, 500, GeoLocDistanceUnit.Kilometers)
    .Select(x => x.Name!)
    .ToListAsync();

Console.WriteLine($"Employees within a 500 km radius of Philadelphia: {string.Join(", ", employeesWithin500Km)}.");

// sorting
var employeesAlphabeticalOrderAsc = await employees.OrderBy(x => x.Name)
    .Select(x => x.Name!)
    .ToListAsync();
Console.WriteLine($"Employees ascending alphabetical order: {string.Join(", ", employeesAlphabeticalOrderAsc)}.");

var employeesAlphabeticalOrderDesc = await employees.OrderByDescending(x => x.Name)
    .Select(x => x.Name!)
    .ToListAsync();
Console.WriteLine($"Employees descending alphabetical order: {string.Join(", ", employeesAlphabeticalOrderDesc)}.");