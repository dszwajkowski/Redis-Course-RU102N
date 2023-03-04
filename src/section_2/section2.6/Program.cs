using StackExchange.Redis;

ConfigurationOptions connectionConfiguration = new()
{
    EndPoints = new EndPointCollection { "localhost:6379" }
};
var multiplexer = await ConnectionMultiplexer.ConnectAsync(connectionConfiguration);
var db = multiplexer.GetDatabase();

var sensor1 = "Sensor:1";
var sensor2 = "Sensor:2";

// clear keys
await db.KeyDeleteAsync(new RedisKey[] { sensor1, sensor2 });

// adding to stream
var random = new Random();
Task.Run(async () =>
{
    double s1Temp = 19;
    double s1Humid = 41;
    double s2Temp = 25;
    double s2Humid = 80;
    int i = 0;
    while (true)
    {
        await db.StreamAddAsync(sensor1, new[]
        {
            new NameValueEntry("temp", s1Temp),
            new NameValueEntry("humidity", s1Humid)
        });

        await db.StreamAddAsync(sensor2, new[]
        {
            new NameValueEntry("temp", s2Temp),
            new NameValueEntry("humidity", s2Humid)
        });

        await Task.Delay(1000);

        i++;
        if (i % 3 == 0)
        {
            s1Temp = Math.Max(s1Temp + random.Next(1) - 1, -273.15);
            s1Humid = Math.Max(Math.Min(s1Humid + random.Next(3) - 2, 100), 0);
            s2Temp = Math.Max(s2Temp + random.Next(1) - 1, -273.15);
            s2Humid = Math.Max(Math.Min(s2Humid + random.Next(3) - 2, 100), 0);
        }
    }
});

// reading from stream
Task.Run(async () =>
{
    var positions = new Dictionary<string, StreamPosition>
    {
        { sensor1, new StreamPosition(sensor1, "0-0") },
        { sensor2, new StreamPosition(sensor2, "0-0") }
    };

    while (true)
    {
        var readResults = await db.StreamReadAsync(positions.Values.ToArray(), countPerStream: 1);
        if (!readResults.Any(x => x.Entries.Any()))
        {
            await Task.Delay(1000);
            continue;
        }
        foreach (var stream in readResults)
        {
            foreach (var entry in stream.Entries)
            {
                Console.WriteLine($"{stream.Key} - {entry.Id}: {string.Join(", ", entry.Values)}");
                positions[stream.Key!] = new StreamPosition(stream.Key, entry.Id);
            }
        }
    }
});

// creating  and reading from consumer group
await db.StreamCreateConsumerGroupAsync(sensor1, "average", "0-0");
await db.StreamCreateConsumerGroupAsync(sensor2, "average", "0-0");

Task.Run(async () =>
{
    var tempTotals = new Dictionary<string, double> { { sensor1, 0 }, { sensor2, 0 } };

    var messageCountTotals = new Dictionary<string, long> { { sensor1, 0 }, { sensor2, 0 } };
    var consumerName = "consumer:1";
    var positions = new Dictionary<string, StreamPosition>
    {
        { sensor1, new StreamPosition(sensor1, ">") },
        { sensor2, new StreamPosition(sensor2, ">") }
    };

    while (true)
    {
        var result = await db.StreamReadGroupAsync(positions.Values.ToArray(), "average", consumerName, countPerStream: 1);
        if (!result.Any(x => x.Entries.Any()))
        {
            await Task.Delay(1000);
            continue;
        }

        foreach (var stream in result)
        {
            foreach (var entry in stream.Entries)
            {
                var temp = (int)entry.Values.First(x => x.Name == "temp").Value;
                messageCountTotals[stream.Key!]++;
                tempTotals[stream.Key!] += temp;
                var avg = tempTotals[stream.Key!] / messageCountTotals[stream.Key!];
                Console.WriteLine($"{stream.Key} average temperature = {avg}");
                await db.StreamAcknowledgeAsync(stream.Key, "average", entry.Id);
            }
        }
    }
});

Console.ReadKey();