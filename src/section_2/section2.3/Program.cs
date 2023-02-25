using StackExchange.Redis;

ConfigurationOptions connectionConfiguration = new()
{
    EndPoints = new EndPointCollection { "localhost:6379" }

};
var multiplexer = await ConnectionMultiplexer.ConnectAsync(connectionConfiguration);
var db = multiplexer.GetDatabase();

string allOrdersSetKey = "orders";
string newOrdersSetKey = "orders:new";
string shippedOrdersSetKey = "orders:shipped";
string closedOrdersSetKey = "orders:closed";

// clear sets
await db.KeyDeleteAsync(allOrdersSetKey);
var orderStateKeys = new RedisKey[]
{
    newOrdersSetKey,
    shippedOrdersSetKey,
    closedOrdersSetKey
};
await db.KeyDeleteAsync(orderStateKeys);

// populate sets
await db.SetAddAsync(newOrdersSetKey, new RedisValue[] {"Order:1", "Order:2"});
await db.SetAddAsync(shippedOrdersSetKey, new RedisValue[] {"Order:3", "Order:4"});
await db.SetAddAsync(closedOrdersSetKey, new RedisValue[] {"Order:5", "Order:6"});

// combine sets
await db.SetCombineAndStoreAsync(SetOperation.Union, allOrdersSetKey, orderStateKeys);

// check if item is in set
Console.WriteLine($"Is Order6 closed?: {await db.SetContainsAsync(closedOrdersSetKey, "Order:6")}");

// enumerate entire set
Console.WriteLine($"All orders: {string.Join(", ", await db.SetMembersAsync(allOrdersSetKey))}");

// enumerate entire set in chunks
List<RedisValue> orders = new();
await foreach (var order in db.SetScanAsync(allOrdersSetKey))
{
    orders.Add(order);
}
Console.WriteLine($"All orders (scan): {string.Join(", ", orders)}");

// moving item between sets
bool moveItemResult = await db.SetMoveAsync(shippedOrdersSetKey, closedOrdersSetKey, "Order:3");
Console.WriteLine($"Closing \"Order:3\" result: {moveItemResult}.");
