using StackExchange.Redis;

ConfigurationOptions connectionConfiguration = new()
{
    EndPoints = new EndPointCollection { "localhost:6379" }
};
var multiplexer = await ConnectionMultiplexer.ConnectAsync(connectionConfiguration);
var db = multiplexer.GetDatabase();

var user = "User:1";

// clear keys
await db.KeyDeleteAsync(new RedisKey[] { user });

// create hash
await db.HashSetAsync(user, new HashEntry[]
{
    new("firstName","Saul"),
    new("age", 20),
    new("email","saul@example.com")
});


// increment hash
var ageAfterIncrement = await db.HashIncrementAsync(user, "age");
// get value from hash
var userFirstName = await db.HashGetAsync(user, "firstName");
Console.WriteLine($"{userFirstName} is {ageAfterIncrement} years old.");

// get all fields from hash

// first solution
var getAllFields = await db.HashGetAllAsync(user);
Console.WriteLine($"User's fields (get all): {string.Join(", ", getAllFields)}.");

// second solution
var scanAllFields = db.HashScanAsync(user);
var scannedFields = new List<HashEntry>();
await foreach (var field in scanAllFields)
{
    scannedFields.Add(field);
}
Console.WriteLine($"User's fields (scan): {string.Join(", ", scannedFields)}.");
