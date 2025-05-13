using kol1C.Models.DTOs;

namespace kol1C.Services;

public interface IDbService
{
    Task<DeliveryDto> GetDeliveryInfoAsync(int deliveryId);
    Task AddNewDeliveryAsync(CreateDeliveryRequest deliveryRequest);
}