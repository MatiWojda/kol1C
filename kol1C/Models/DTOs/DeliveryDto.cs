namespace kol1C.Models.DTOs;
public class DeliveryProductDto
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Amount { get; set; }
}

public class DeliveryDto
{
    public DateTime Date { get; set; }
    public CustomerDto Customer { get; set; } = new CustomerDto();
    public DriverDto Driver { get; set; } = new DriverDto();
    public List<DeliveryProductDto> Products { get; set; } = [];
}

public class CustomerDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
}

public class DriverDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
}