using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WarehouseREST.DTOs;
using WarehouseREST.Models;
using WarehouseREST.Repo;

namespace WarehouseREST.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WarehouseController : ControllerBase
    {

        private readonly IWarehouse _warehouse;

        public WarehouseController(IWarehouse warehouse) 
        {
            _warehouse = warehouse;
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
                await _warehouse.AddProductAsync(product);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

    }
}
