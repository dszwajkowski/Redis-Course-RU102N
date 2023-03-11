
using NRedisTimeSeries;
using NRedisTimeSeries.Commands.Enums;
using NRedisTimeSeries.DataTypes;
using StackExchange.Redis;


ConfigurationOptions connectionConfiguration = new()
{
    EndPoints = { "localhost:6379" }
};
var multiplexer = await ConnectionMultiplexer.ConnectAsync(connectionConfiguration);
var db = multiplexer.GetDatabase();

// clear keys.
db.KeyDelete(new RedisKey[] {"sensor", "sensor:Max", "sensor:Avg", "sensor:Min"});

// create time series
await db.TimeSeriesCreateAsync("sensor", 60000, new List<TimeSeriesLabel> { new("id", "sensor-1") });

var aggregations = new TsAggregation[] { TsAggregation.Avg, TsAggregation.Min, TsAggregation.Max };
foreach (var agg in aggregations)
{
    await db.TimeSeriesCreateAsync($"sensor:{agg}", 60000, new List<TimeSeriesLabel> { new("type", agg.ToString()), new("aggregation-for", "sensor-1") });
    await (db.TimeSeriesCreateRuleAsync("sensor", new TimeSeriesRule($"sensor:{agg}", 5000, agg)));
}

// produce time series data
var producerTask = Task.Run(async () =>
{
    while (true)
    {
        await db.TimeSeriesAddAsync("sensor", "*", Random.Shared.Next(50));
        await Task.Delay(1000);
        
    }
});

// retrieve the most recent data from the time series
var consumerTask = Task.Run(async () => {
    while (true)
    {
        await Task.Delay(1000);
        var result = await db.TimeSeriesGetAsync("sensor");
        Console.WriteLine($"consumerTask: {result.Time.Value}: {result.Val}");
    }
});

// query multiple time series with the label. 
var aggregationConsumerTask = Task.Run(async () =>
{
    while (true)
    {
        await Task.Delay(5000);
        var results = await db.TimeSeriesMGetAsync(new List<string>() { "aggregation-for=sensor-1" }, true);
        foreach (var result in results)
        {
            Console.WriteLine($"aggregationConsumerTask: {result.labels.First(x => x.Key == "type").Value}: {result.value.Val}");
        }
    }
});

Console.ReadKey();
