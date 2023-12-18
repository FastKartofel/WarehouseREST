using WarehouseREST.DTOs;

namespace WarehouseREST.Repo
{
    public interface IWarehouse
    {
        Task AddProductAsync(ProductDTO productDto);

        Task AddProductViaStoredProcedureAsync(ProductDTO productDTO);
    }
}
