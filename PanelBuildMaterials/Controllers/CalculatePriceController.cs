using Microsoft.AspNetCore.Mvc;
using PanelBuildMaterials.Models;
using PanelBuildMaterials.Utilities;

namespace PanelBuildMaterials.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CalculatePriceController : ControllerBase
    {
        private readonly OrderService _orderService;

        public CalculatePriceController(OrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet("CalculatePrice")]
        public async Task<IActionResult> CalculatePrice(int? materialId, int? serviceId, int? count)
        {
            try
            {
                var orderDetail = new BuildingMaterialsServicesOrder
                {
                    BuildingMaterialId = materialId ?? 0,
                    ServiceId = serviceId ?? 0,
                    CountBuildingMaterial = count > 0 ? count : null
                };

                var orderPrice = await _orderService.CalculateOrderPriceAsync(orderDetail);

                return Ok(new { orderPrice });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
