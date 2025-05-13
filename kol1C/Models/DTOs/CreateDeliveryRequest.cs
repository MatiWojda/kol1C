using System.ComponentModel.DataAnnotations;

namespace kol1C.Models.DTOs;

public class CreateDeliveryRequest
{
    public int DeliveryId { get; set; }
    public int CustomerId { get; set; }
    public string LicenceNumber { get; set; } = string.Empty;
    public List<CreateDeliverProductItem> Products { get; set; } =  new List<CreateDeliverProductItem>();
}

public class CreateDeliverProductItem
{
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}