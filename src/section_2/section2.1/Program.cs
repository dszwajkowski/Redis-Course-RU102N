using StackExchange.Redis;

ConfigurationOptions connectionConfiguration = new()
{
    EndPoints = new EndPointCollection { "localhost:6379" },
};
var multiplexer = await ConnectionMultiplexer.ConnectAsync(connectionConfiguration);
var db = multiplexer.GetDatabase();

var stringKey = new RedisKey("stringKey");
await db.StringSetAsync(stringKey, "Redis");
await db.StringAppendAsync(stringKey, " Course");
var stringFromDb = await db.StringGetAsync(stringKey);
Console.WriteLine("Full string: " + stringFromDb);

var numericStringKey = new RedisKey("numericStringKey");
await db.StringSetAsync(numericStringKey, 68);
await db.StringIncrementAsync(numericStringKey);
var numericStringFromDb = await db.StringGetAsync(numericStringKey);
Console.WriteLine("Numeric string: " + numericStringFromDb);

var conditionalKey = new RedisKey("conditionalKey");
await db.StringSetAsync(conditionalKey, "This string has been set", when: When.NotExists);
await db.StringSetAsync(conditionalKey, "This string won't be set", when: When.NotExists);
var conditionalStringFromDb = await db.StringGetAsync(conditionalKey);
Console.WriteLine(conditionalStringFromDb);




