using kol1C.Exceptions;
using kol1C.Models.DTOs;
using kol1C.Services;
using Microsoft.AspNetCore.Mvc;

namespace kol1C.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeliveriesController : ControllerBase
    {
        private readonly IDbService _dbService;
        public DeliveriesController(IDbService dbService)
        {
            _dbService = dbService;
        }

        [HttpGet("{id}", Name = "GetDeliveryInfo")]
        public async Task<IActionResult> GetDeliveryInfo(int id)
        {
            try
            {
                var res = await _dbService.GetDeliveryInfoAsync(id);
                return Ok(res);
            }
            catch (NotFoundException e)
            {
                return NotFound(e.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddNewDelivery(CreateDeliveryRequest deliveryRequest)
        {
            if (!deliveryRequest.Products.Any())
            {
                return BadRequest("At least one item is required.");
            }

            try
            {
                await _dbService.AddNewDeliveryAsync(deliveryRequest);
            }
            catch (ConflictException e)
            {
                return Conflict(e.Message);
            }
            catch (NotFoundException e)
            {
                return NotFound(e.Message);
            }
            
            return CreatedAtRoute(
                routeName: "GetDeliveryInfo",
                routeValues: new { id = deliveryRequest.DeliveryId },
                value: null
            );
        }    
    }
}