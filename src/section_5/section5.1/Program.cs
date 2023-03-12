using Redis.OM;
using section5._1;

var provider = new RedisConnectionProvider("redis://localhost:6379");


// create indexes
await provider.Connection.CreateIndexAsync(typeof(Sale));
await provider.Connection.CreateIndexAsync(typeof(Employee));