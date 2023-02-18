using System.Diagnostics;
using StackExchange.Redis;

ConfigurationOptions connectionConfiguration = new()
{
    EndPoints = new EndPointCollection { "localhost:6379" },
};
var multiplexer = await ConnectionMultiplexer.ConnectAsync(connectionConfiguration);
var db = multiplexer.GetDatabase();

var sw = Stopwatch.StartNew();

// unpipelined commands execution
for (int i = 0; i < 1000; i++)
{
    await db.PingAsync();
}
sw.Stop();
Console.WriteLine($"Executing unpipelined commands took {sw.ElapsedMilliseconds} ms.");

// implicitly pipelined commands execution
List<Task<TimeSpan>> tasks = new();
sw.Restart();
for (int i = 0; i < 1000; i++)
{
    tasks.Add(db.PingAsync());
}
await Task.WhenAll(tasks);
sw.Stop();
Console.WriteLine($"Executing implicitly pipelined commands took {sw.ElapsedMilliseconds} ms.");

// explicitly pipelined commands execution
tasks.Clear();
var batch = db.CreateBatch();
sw.Restart();
for (int i = 0; i < 1000; i++)
{
    tasks.Add(batch.PingAsync());
}
batch.Execute();
sw.Stop();
Console.WriteLine($"Executing explicitly pipelined commands took {sw.ElapsedMilliseconds} ms.");