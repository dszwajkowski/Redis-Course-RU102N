using Redis.OM;
using Redis.OM.Modeling;
using section5._2;

var provider = new RedisConnectionProvider("redis://localhost:6379");

// clear indexes
provider.Connection.DropIndexAndAssociatedRecords(typeof(Sale));
provider.Connection.DropIndexAndAssociatedRecords(typeof(Employee));

// create indexes
await provider.Connection.CreateIndexAsync(typeof(Sale));
await provider.Connection.CreateIndexAsync(typeof(Employee));

// insert employee
var employees = provider.RedisCollection<Employee>();
var employee = new Employee
{
    Name = "Steve",
    Address = new Address
    {
        StreetAddress = "Main Street",
        PostalCode = "34739",
        Location = new GeoLoc(-81.006, 27.872)
    },
    Sales = new List<string>()
};
var key = await employees.InsertAsync(employee);

Console.WriteLine($"Employee id: {employee.Id}");
Console.WriteLine($"Key name: {key}");

// insert sale
var sale = new Sale
{
    Id = Guid.NewGuid().ToString(),
    Address = new Address
    {
        StreetAddress = "Pinewood Ave",
        PostalCode = "10001",
        Location = new GeoLoc(-73.991, 40.753)
    },
    EmployeeId = employee.Id,
    Total = 5000,
};

key = await provider.Connection.SetAsync(sale, TimeSpan.FromMinutes(5));
Console.WriteLine($"Sale id: {sale.Id}");
Console.WriteLine($"Key name: {key}");