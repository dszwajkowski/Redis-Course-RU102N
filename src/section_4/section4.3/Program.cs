using NRedisGraph;
using StackExchange.Redis;

ConfigurationOptions connectionConfiguration = new()
{
    EndPoints = new EndPointCollection { "localhost:6379" }
};
var multiplexer = await ConnectionMultiplexer.ConnectAsync(connectionConfiguration);
var db = multiplexer.GetDatabase();

// clear keys
await db.KeyDeleteAsync("pets");

var graph = new RedisGraph(db);

// create graph
var createBobResult = await graph.QueryAsync("pets", "CREATE(:human{name:'Bob',age:32})");
Console.WriteLine($"Nodes Created:{createBobResult.Statistics.NodesCreated}.");
Console.WriteLine($"Properties Set:{createBobResult.Statistics.PropertiesSet}.");
Console.WriteLine($"Labels Created:{createBobResult.Statistics.LabelsAdded}.");
Console.WriteLine($"Operation took:{createBobResult.Statistics.QueryInternalExecutionTime}.");

await graph.QueryAsync("pets", "CREATE(:human{name:'Alice',age:30})");
await graph.QueryAsync("pets", "CREATE(:pet{name:'Honey',age:5,species:'canine',breed:'Greyhound'})");

// create relationships
await graph.QueryAsync("pets", "MATCH(a:human),(p:pet) WHERE(a.name='Bob' and p.name='Honey') CREATE (a)-[:OWNS]->(p)");
await graph.QueryAsync("pets", "MATCH(a:human),(p:pet) WHERE(a.name='Alice' and p.name='Honey') CREATE (a)-[:WALKS]->(p)");
await graph.QueryAsync("pets", "MATCH(a:human),(p:pet) WHERE(a.name='Bob' and p.name='Honey') CREATE (a)-[:WALKS]->(p)");

// query graph

// all owners of honey
var ownersOfHoney = await graph.QueryAsync("pets", "MATCH(a:human),(p:pet) where (a)-[:OWNS]->(p) and p.name='Honey' return a");
Console.WriteLine($"Owners of Honey: {string.Join(", ", ownersOfHoney)}.");

// all people walking with Honey
var peopleWalkingWithHoney = await graph.QueryAsync("pets", "MATCH(a:human),(p:pet) where (a)-[:WALKS]->(p) and p.name='Honey' return a");
Console.WriteLine($"People walking with Honey: {string.Join(", ", peopleWalkingWithHoney.Select(x => (Node) x.Values.First()).Select(x => x.PropertyMap["name"].Value))}");

var dogsOwnedByBob = await graph.QueryAsync("pets", "MATCH(a:human),(p:pet) where (a)-[:OWNS]->(p) and p.species='canine' and a.name='Bob' return p");
Console.WriteLine($"Bob's dogs: {string.Join(", ", dogsOwnedByBob.Select(x => (Node)x.Values.First()).Select(x => x.PropertyMap["name"].Value))}");
