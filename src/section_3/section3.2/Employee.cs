namespace section3._2;

public class Employee
{
    public int EmployeeId { get; set; }
    public string Name { get; set; } = null!;
    public List<Sale> Sales { get; } = new();
}
