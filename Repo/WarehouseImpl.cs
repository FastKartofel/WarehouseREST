using Microsoft.Data.SqlClient;
using System.Data;
using WarehouseREST.DTOs;

namespace WarehouseREST.Repo
{
    public class WarehouseImpl : IWarehouse
    {
        private readonly IConfiguration _configuration;

        public WarehouseImpl(IConfiguration configuration) 
        {   
            _configuration = configuration;
        }

        public async Task AddProductAsync(ProductDTO productDTO) 
        {

            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
               await connection.OpenAsync();
                SqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;
                        command.Transaction = transaction;

                        command.CommandText = "SELECT COUNT(*) FROM Product WHERE IdProduct = @ProductId";
                        SqlParameter sqlParameter = command.Parameters.AddWithValue("@ProductId", productDTO.ProductId);

                        var result = (int)await command.ExecuteNonQueryAsync();

                        if (result == 0)
                        {
                            transaction.Rollback();

                        }


                        command.Parameters.Clear();
                        command.CommandText = "SELECT COUNT(*) FROM ProductDTO WHERE IdWholesaler = @WholesalerId";
                        command.Parameters.AddWithValue("@WholesalerId", productDTO.WholesalerId);

                        result = (int)await command.ExecuteNonQueryAsync();

                        if (result == 0)
                        {
                            transaction.Rollback();

                        }


                        if (productDTO.Amount <= 0)
                        {
                            transaction.Rollback();
                            throw new Exception("Amount must be greater than 0");
                        }


                        command.Parameters.Clear();
                        command.CommandText = "SELECT COUNT(*) FROM ProductDTO WHERE IdProduct = @ProductId AND Amount = @Amount AND CreatedAt < @CreatedAt";
                        command.Parameters.AddWithValue("@ProductId", productDTO.ProductId);
                        command.Parameters.AddWithValue("@Amount", productDTO.Amount);
                        command.Parameters.AddWithValue("@CreatedAt", productDTO.CreatedAt);

                        result = (int)await command.ExecuteNonQueryAsync();

                        if (result == 0)
                        {
                            transaction.Rollback();

                        }

                        command.Parameters.Clear();
                        command.CommandText = "SELECT COUNT(*) FROM Product_Warehouse WHERE IdOrder = (SELECT TOP 1 IdOrder FROM [Order] WHERE IdProduct = @ProductId AND Amount = @Amount AND CreatedAt < @CreatedAt)";
                        result = (int)await command.ExecuteNonQueryAsync();
                        if (result > 0)
                        {
                            transaction.Rollback();
                            throw new Exception("Order has already been processed");
                        }


                        command.Parameters.Clear();
                        command.CommandText = "UPDATE Order SET FullfilledAt = @FullfilledAt WHERE IdOrder = (SELECT TOP 1 IdOrder FROM [Order] WHERE IdProduct = @ProductId AND Amount = @Amount AND CreatedAt < @CreatedAt)";
                        command.Parameters.AddWithValue("@FullfilledAt", DateTime.Now);
                        command.ExecuteNonQuery();


                        command.Parameters.Clear();
                        command.CommandText = "INSERT INTO Product_Warehouse (IdProduct, IdWarehouse, Amount, IdOrder) VALUES (@ProductId, @WarehouseId, @Amount, (SELECT TOP 1 IdOrder FROM [Order] WHERE IdProduct = @ProductId AND Amount = @Amount AND CreatedAt < @CreatedAt))";
                        command.Parameters.AddWithValue("@WarehouseId", 1);
                        command.ExecuteNonQuery();

                        command.Parameters.Clear();
                        command.CommandText = "UPDATE Warehouse SET Stock = Stock + @Amount WHERE IdWarehouse = @WarehouseId";
                        command.ExecuteNonQuery();

                        transaction.Commit();

                    }

                }
                catch (Exception ex)
                {

                    transaction.Rollback();

                }
                await connection.CloseAsync();

            }

        }

        public async Task AddProductViaStoredProcedureAsync(ProductDTO productDTO)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (SqlCommand command = new SqlCommand("AddProductToWarehouse", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@IdProduct", productDTO.ProductId);
                    command.Parameters.AddWithValue("@IdWarehouse", productDTO.WarehouseId);
                    command.Parameters.AddWithValue("@Amount", productDTO.Amount);
                    command.Parameters.AddWithValue("@CreatedAt", productDTO.CreatedAt);

                    try
                    {
                        await command.ExecuteNonQueryAsync();
                    }
                    catch (SqlException ex)
                    {
                        throw new Exception("SQL Error: " + ex.Message);
                    }
                }
            }
        }

    }
}
