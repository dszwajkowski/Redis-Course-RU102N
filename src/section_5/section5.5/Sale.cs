using Redis.OM.Modeling;

namespace section5._5;

[Document(StorageType = StorageType.Json)]
public class Sale
{
    [RedisIdField]
    [Indexed]
    public string? Id { get; set; }
    [Indexed(Aggregatable = true)]
    public string? EmployeeId { get; set; }
    [Indexed(Aggregatable = true)]
    public int Total { get; set; }
    [Indexed(CascadeDepth = 2)]
    public Address? Address { get; set; }
}