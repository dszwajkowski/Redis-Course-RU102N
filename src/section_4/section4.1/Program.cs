using StackExchange.Redis;

ConfigurationOptions connectionConfiguration = new()
{
    EndPoints = new EndPointCollection { "localhost:6379" }
};
var multiplexer = await ConnectionMultiplexer.ConnectAsync(connectionConfiguration);
var db = multiplexer.GetDatabase();

// clear keys
await db.KeyDeleteAsync("bf");
await db.KeyDeleteAsync("topk");

/* BLOOM FILTER */

char[] delimiterChars = { ' ', ',', '.', ':', '\t', '\n', '—', '?', '"', ';', '!', '’', '\r', '\'', '(', ')', '”' };
var textFromFile = await File.ReadAllTextAsync(@".\data\moby_dick.txt");
var words = textFromFile.Split(delimiterChars)
    .Where(x => !string.IsNullOrWhiteSpace(x))
    .ToArray();

var bloomList = words.Aggregate(new List<object> { "bf" }, (list, word) =>
{
    list.Add(word);
    return list;
});

// reserve space for bloom filter
// first argument - acceptable error rate, second - estimated size of set
await db.ExecuteAsync("BF.RESERVE", "bf", 0.01, 20000); 
// add everything in one shot
await db.ExecuteAsync("BF.MADD", bloomList, CommandFlags.FireAndForget);


// check if word "damn" exists in bloom filter
var doesDamnExists = await db.ExecuteAsync("BF.EXISTS", "bf", "damn");

// print return type
Console.WriteLine($"Result type: {doesDamnExists.Type}.");

// print result (BF.EXISTS returns int)
Console.WriteLine($"Does \"damn\" exists: {((int) doesDamnExists == 1 ? "yes" : "no")}");

/* TOP-K */

// reserve heavy hitter
await db.ExecuteAsync("TOPK.RESERVE", "topk", 10, 20, 10, .925);

// organize the words into a list where each word is followed by the number of occurrences
var topKList = words.Aggregate(new Dictionary<string, int>(), (dict, word) =>
{
    if (!dict.ContainsKey(word))
    {
        dict.Add(word, 0);
    }

    dict[word]++;
    return dict;
}).Aggregate(new List<object> { "topk" }, (list, kvp) =>
{
    list.Add(kvp.Key);
    list.Add(kvp.Value);
    return list;
});

// add everything to Top-K
await db.ExecuteAsync("TOPK.INCRBY", topKList, CommandFlags.FireAndForget);

// enumerate Top-K
var topk = await db.ExecuteAsync("TOPK.LIST", "topk");
var top10WithCounts = (await db.ExecuteAsync("TOPK.LIST", "topk", "WITHCOUNT")).ToDictionary().Select(x => $"{x.Key}: {x.Value}");
Console.WriteLine($"Top 10 with counts: {string.Join(", ", top10WithCounts)}");