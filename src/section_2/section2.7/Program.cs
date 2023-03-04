using StackExchange.Redis;

ConfigurationOptions connectionConfiguration = new()
{
    EndPoints = new EndPointCollection { "localhost:6379" }
};
var multiplexer = await ConnectionMultiplexer.ConnectAsync(connectionConfiguration);
var db = multiplexer.GetDatabase();

// prepared script
var scriptText = @"
        local id = redis.call('incr', @id_key)
        local key = 'key:' .. id
        redis.call('set', key, @value)
        return key";

var script = LuaScript.Prepare(scriptText);

var key1 = await db.ScriptEvaluateAsync(script, new { id_key = (RedisKey)"autoIncrement", value = "A String Value" });
var key2 = await db.ScriptEvaluateAsync(script, new { id_key = (RedisKey)"autoIncrement", value = "Another String Value" });

Console.WriteLine($"Key1: {key1}");
Console.WriteLine($"Key2: {key2}");

// non-prepared script
var nonPreparedScript = @"
        local id = redis.call('incr', KEYS[1])
        local key = 'key:' .. id
        redis.call('set', key, ARGV[1])
        return key";

var key3 = await db.ScriptEvaluateAsync(nonPreparedScript, new RedisKey[] { "autoIncrement" }, new RedisValue[] { "Yet another string value" });
Console.WriteLine($"Key3: {key3}");
