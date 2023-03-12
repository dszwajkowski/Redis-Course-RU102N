using Redis.OM.Modeling;

namespace section5._1;

[Document(StorageType = StorageType.Json, Prefixes = new[] { "Employee" }, IndexName = "employees")]
public class Employee
{
    [RedisIdField]
    [Indexed]
    public string? Id { get; set; }
    [Indexed]
    public List<string>? Sales { get; set; }
    [Indexed(JsonPath = "$.Location")]
    [Indexed(JsonPath = "$.PostalCode")]
    public Address? Address { get; set; }
    [Indexed]
    public string? Name { get; set; }
}