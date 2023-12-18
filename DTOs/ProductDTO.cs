using System.ComponentModel.DataAnnotations;

namespace WarehouseREST.DTOs
{
    public class ProductDTO
    {
        [Range(0, 10, ErrorMessage = "Amount must be in range 0 - 10")]
        public int Amount { get; set; }

        public int ProductId { get; set; }

        public int WholesalerId { get; set; }

        public int WarehouseId { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
