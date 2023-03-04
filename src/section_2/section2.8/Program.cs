using StackExchange.Redis;

ConfigurationOptions connectionConfiguration = new()
{
    EndPoints = new EndPointCollection { "localhost:6379" }
};
var multiplexer = await ConnectionMultiplexer.ConnectAsync(connectionConfiguration);
var db = multiplexer.GetDatabase();

var transaction = db.CreateTransaction();

transaction.HashSetAsync("Person:1", new HashEntry[]
{
    new ("name", "Saul"),
    new ("age", 30),
    new ("postalCode", "00-001")
});
transaction.SortedSetAddAsync("person:name:Saul", "Person:1", 0);
transaction.SortedSetAddAsync("person:postalCode:00-001", "Person:1", 0);
transaction.SortedSetAddAsync("person:age", "Person:1", 30);
bool result = transaction.Execute();
Console.WriteLine($"Transaction 1 result: {result}.");

// transaction condition
transaction.AddCondition(Condition.HashEqual("Person:1", "age", 30));
transaction.HashIncrementAsync("person:1", "age");
transaction.SortedSetIncrementAsync("person:age", "person:1", 1);
result = transaction.Execute();
Console.WriteLine($"Transaction 2 result: {result}.");

// failed transaction condition
transaction.AddCondition(Condition.HashEqual("person:1", "age", 55));
transaction.HashIncrementAsync("person:1", "age");
transaction.SortedSetIncrementAsync("person:age", "person:1", 1);
result = transaction.Execute();
Console.WriteLine($"Transaction 3 result: {result}.");