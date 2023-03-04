using StackExchange.Redis;

ConfigurationOptions connectionConfiguration = new()
{
    EndPoints = new EndPointCollection { "localhost:6379" }

};
var multiplexer = await ConnectionMultiplexer.ConnectAsync(connectionConfiguration);
var db = multiplexer.GetDatabase();

var userAgeSet = "users:age";
var userLastAccessSet = "users:lastAccess";
var userHighScoreSet = "users:highScores";
var userMostRecentlyActive = "users:mostRecentlyActive";
var namesSet = "names";

await db.KeyDeleteAsync(new RedisKey[] { userAgeSet, userLastAccessSet, userHighScoreSet, namesSet, userMostRecentlyActive });

await db.SortedSetAddAsync(userAgeSet,
    new SortedSetEntry[]
    {
        new("User:1", 20),
        new("User:2", 23),
        new("User:3", 18),
        new("User:4", 35),
        new("User:5", 55),
        new("User:6", 62)
    });

await db.SortedSetAddAsync(userLastAccessSet,
    new SortedSetEntry[]
    {
        new("User:1", 1648483867),
        new("User:2", 1658074397),
        new("User:3", 1659132660),
        new("User:4", 1652082765),
        new("User:5", 1658087415),
        new("User:6", 1656530099)
    });

await db.SortedSetAddAsync(userHighScoreSet,
    new SortedSetEntry[]
    {
        new("User:1", 10),
        new("User:2", 55),
        new("User:3", 36),
        new("User:4", 25),
        new("User:5", 21),
        new("User:6", 44)
    });

await db.SortedSetAddAsync(namesSet,
    new SortedSetEntry[]
    {
        new("John", 0),
        new("Fred", 0),
        new("Bob", 0),
        new("Susan", 0),
        new("Alice", 0),
        new("Tom", 0)
    });

// check score
Console.WriteLine($"User:5 high score: {await db.SortedSetScoreAsync(userHighScoreSet, "User:5")}.");

// check rank
Console.WriteLine($"User:3 rank: {await db.SortedSetRankAsync(userHighScoreSet, "User:3", Order.Descending)}.");

// range by rank
var topThreeHighscores = await db.SortedSetRangeByRankAsync(userHighScoreSet, 0, 2, Order.Descending);
Console.WriteLine($"Top three high scores: {string.Join(", ", topThreeHighscores)}.");

// range by score
var eighteenToThirty = await db.SortedSetRangeByScoreAsync(userAgeSet, 18, 30);
Console.WriteLine($"Users aged 18-30: {string.Join(", ", eighteenToThirty)}.");

// lexicographic range
var namesAlphabeticOrder = await db.SortedSetRangeByValueAsync(namesSet);
Console.WriteLine($"Names sorted alphabetically: {string.Join(", ", namesAlphabeticOrder)}.");

// lexicographic range + only names between B-S
var namesBetweenBandS = await db.SortedSetRangeByValueAsync(namesSet, "B", "T", Exclude.Stop);
Console.WriteLine($"Names sorted alphabetically between B-S: {string.Join(", ", namesBetweenBandS)}.");

// combing sets
db.SortedSetRangeAndStore(userLastAccessSet, userMostRecentlyActive, 0, 2, order: Order.Descending);
var rankOrderMostRecentlyActive = await db.SortedSetCombineWithScoresAsync(SetOperation.Intersect, new RedisKey[] { userHighScoreSet, userMostRecentlyActive }, new double[] { 1, 0 });
rankOrderMostRecentlyActive = rankOrderMostRecentlyActive.Reverse().ToArray();
Console.WriteLine($"Most recently active high scores: {string.Join(", ", rankOrderMostRecentlyActive)}.");