using StackExchange.Redis;
using System.Threading.Channels;

ConfigurationOptions connectionConfiguration = new()
{
    EndPoints = new EndPointCollection { "localhost:6379" }
};
var multiplexer = await ConnectionMultiplexer.ConnectAsync(connectionConfiguration);
var db = multiplexer.GetDatabase();

// create subscriber
var subscriber = multiplexer.GetSubscriber();
var cancellationTokenSource = new CancellationTokenSource();
var cancellationToken = cancellationTokenSource.Token;

// sequential process 
var channel = await subscriber.SubscribeAsync("test-channel");
channel.OnMessage(msg => { Console.WriteLine($"Sequentially received: {msg.Message} on channel: {msg.Channel}"); });

// concurrent process 
await subscriber.SubscribeAsync("test-channel", 
    (channel, value) => { Console.WriteLine($"Received: {value} on channel: {channel}"); });

// create producer
var sendTask = Task.Run(async () =>
{
    var i = 0;
    while (!cancellationToken.IsCancellationRequested)
    {
        await db.PublishAsync("test-channel", $"Message no. {i++}.");
        await Task.Delay(1000);
    }
});

// subscribe to pattern
{
    await subscriber.SubscribeAsync("Pattern:*",
        (channel, value) => { Console.WriteLine($"Received pattern: {value} on channel: {channel}"); });
}

var sendPatternTask = Task.Run(async () =>
{
    var i = 0;
    while (!cancellationToken.IsCancellationRequested)
    {
        await db.PublishAsync($"Pattern:{Guid.NewGuid()}", i++);
        await Task.Delay(1000);
    }
});

Console.ReadKey();

await channel.UnsubscribeAsync();
Console.WriteLine("Unsubscribed single channel");
Console.ReadKey();

await subscriber.UnsubscribeAsync("test-channel");
Console.WriteLine("Unsubscribed subscriber from test-channel");
Console.ReadKey();

await subscriber.UnsubscribeAllAsync();
Console.WriteLine("Unsubscribed all");
Console.ReadKey();