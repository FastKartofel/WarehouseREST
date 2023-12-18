using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WarehouseREST.DTOs;
using WarehouseREST.Repo;

namespace WarehouseREST.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Warehouses2Controller : ControllerBase
    {
        private readonly IWarehouse _warehouseService;

        public Warehouses2Controller(IWarehouse warehouseService)
        {
            _warehouseService = warehouseService;
        }

        [HttpPost]
        public async Task<ActionResult> AddProductAsync([FromBody] ProductDTO product)
        {
            if (product == null)
            {
                return BadRequest("Product data is required.");
            }

            try
            {
                await _warehouseService.AddProductViaStoredProcedureAsync(product);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
