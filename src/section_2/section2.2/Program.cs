using StackExchange.Redis;

ConfigurationOptions connectionConfiguration = new()
{
    EndPoints = new EndPointCollection { "localhost:6379" },
};
var multiplexer = await ConnectionMultiplexer.ConnectAsync(connectionConfiguration);
var db = multiplexer.GetDatabase();

// left pushing
var firstListKey = new RedisKey("firstList");
// clear previously added items
await db.KeyDeleteAsync(firstListKey);
await db.ListLeftPushAsync(firstListKey, new RedisValue[] {"String1", "String2", "String3", "String4", "String5"});
Console.WriteLine($"First string in list 1: {await db.ListGetByIndexAsync(firstListKey, 0)}.");
Console.WriteLine($"Last string in list 1: {await db.ListGetByIndexAsync(firstListKey, -1)}.");

// right pushing
var secondListKey = new RedisKey("secondList");
// clear previously added items
await db.KeyDeleteAsync(secondListKey);
await db.ListRightPushAsync(secondListKey, new RedisValue[] {"String6", "String7", "String8","String9", "String10"});
Console.WriteLine($"First string in list 2: {await db.ListGetByIndexAsync(secondListKey, 0)}.");
Console.WriteLine($"Last string in list 2: {await db.ListGetByIndexAsync(secondListKey, -1)}.");

// enumerating list
Console.WriteLine($"All items from list 2: {string.Join(", ", await db.ListRangeAsync(secondListKey))}.");
Console.WriteLine($"Items from list 2 without first and last item: {string.Join(", ", await db.ListRangeAsync(secondListKey, 1, -2))}.");

// move item between lists
await db.ListMoveAsync(firstListKey, secondListKey, ListSide.Left, ListSide.Left);
Console.WriteLine($"Items from list 1 after moving: {string.Join(", ", await db.ListRangeAsync(firstListKey))}.");
Console.WriteLine($"Items from list 2 after moving: {string.Join(", ", await db.ListRangeAsync(secondListKey))}.");

// using list as FIFO queue
await db.ListLeftPushAsync(firstListKey, "String5");
Console.WriteLine("Adding \"String5\" to queue.");
var popedItem = await db.ListRightPopAsync(firstListKey);
Console.WriteLine($"Removing from queue. Removed item: {popedItem}.");

// using list as LIFO stack
await db.ListLeftPushAsync(firstListKey, "String6");
Console.WriteLine("Adding \"String6\" to queue.");
popedItem = await db.ListLeftPopAsync(firstListKey);
Console.WriteLine($"Removing from queue. Removed item: {popedItem}.");

// search
Console.WriteLine($"String3 index in list 1: {await db.ListPositionAsync(firstListKey, "String3")}.");

// list size
Console.WriteLine($"List 1 size: {await db.ListLengthAsync(firstListKey)}");
