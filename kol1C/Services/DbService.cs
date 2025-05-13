using System.Data.Common;
using kol1C.Exceptions;
using kol1C.Models.DTOs;
using Microsoft.Data.SqlClient;

namespace kol1C.Services;

public class DbService : IDbService
{
    private readonly string _connectionString;
    public DbService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Default") ?? string.Empty;
    }
    
    public async Task<DeliveryDto> GetDeliveryInfoAsync(int deliveryId)
    {
        var query =
            @"SELECT d.date, c.first_name, c.last_name, c.date_of_birth, dr.first_name, dr.last_name, dr.licence_number, p.name, p.price, pd.amount
            FROM Delivery d
            JOIN Customer c ON d.customer_id = c.customer_id
            JOIN Driver dr ON d.driver_id = dr.driver_id
            JOIN Product_Delivery pd ON d.delivery_id = pd.delivery_id
            JOIN Product p ON pd.product_id = p.product_id
            WHERE d.delivery_id = 1;";
        
        await using SqlConnection connection = new SqlConnection(_connectionString);
        await using SqlCommand command = new SqlCommand();
        
        command.Connection = connection;
        command.CommandText = query;
        await connection.OpenAsync();
        
        command.Parameters.AddWithValue("@appointmentId", deliveryId);
        var reader = await command.ExecuteReaderAsync();
        
        DeliveryDto? delivery = null;
        
        while (await reader.ReadAsync())
        {
            if (delivery is null)
            {
                delivery = new DeliveryDto()
                {
                    Date = reader.GetDateTime(0),
                    Customer = new CustomerDto()
                    {
                        FirstName = reader.GetString(1),
                        LastName = reader.GetString(2),
                        DateOfBirth = reader.GetDateTime(3)
                    },
                    Driver = new DriverDto()
                    {
                        FirstName = reader.GetString(4),
                        LastName = reader.GetString(5),
                        LicenseNumber = reader.GetString(6)
                    },
                    Products = new List<DeliveryProductDto>(),
                };
            }
            
            delivery.Products.Add(new DeliveryProductDto()
            {
                Name = reader.GetString(7),
                Price = reader.GetDecimal(8),
                Amount = reader.GetInt32(9)
            });
            
        }       
        
        if (delivery is null)
        {
            throw new NotFoundException("No delivery found for specified ID");
        }
        
        return delivery;
    }

    public async Task AddNewDeliveryAsync(CreateDeliveryRequest deliveryRequest)
    {
        await using SqlConnection connection = new SqlConnection(_connectionString);
        await using SqlCommand command = new SqlCommand();
        
        command.Connection = connection;
        await connection.OpenAsync();
        
        DbTransaction transaction = await connection.BeginTransactionAsync();
        command.Transaction = transaction as SqlTransaction;

        try
        {
            command.Parameters.Clear();
            command.CommandText = "SELECT 1 FROM Customer WHERE customer_id = @IdCustomer;";
            command.Parameters.AddWithValue("@IdCustomer", deliveryRequest.CustomerId);
                
            var customerIdRes = await command.ExecuteScalarAsync();
            if(customerIdRes is null)
                throw new NotFoundException($"Customer with ID - {deliveryRequest.CustomerId} - not found.");
            
            command.Parameters.Clear();
            command.CommandText = "SELECT driver_id FROM Driver WHERE licence_number = @licenceNumber;";
            command.Parameters.AddWithValue("@licenceNumber", deliveryRequest.LicenceNumber);
            var driverIdRes = await command.ExecuteScalarAsync();
            if (driverIdRes is null)
                throw new NotFoundException($"Doctor with PWZ - {deliveryRequest.LicenceNumber} - not found.");
            var driverId = (int)driverIdRes;
            
            command.Parameters.Clear();
            command.CommandText = 
                @"INSERT INTO Delivery
            VALUES(@IdDelivery, @CustomerId, @DriverId, GETDATE());";

            command.Parameters.AddWithValue("@IdDelivery", deliveryRequest.DeliveryId);
            command.Parameters.AddWithValue("@CustomerId", deliveryRequest.CustomerId);
            command.Parameters.AddWithValue("@DriverId", driverId);
            
            try
            {
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                throw new ConflictException("Appointment with the same ID already exists.");
            }
            

            foreach (var product in deliveryRequest.Products)
            {
                command.Parameters.Clear();
                command.CommandText = "SELECT product_id FROM Product WHERE name = @ProductName;";
                command.Parameters.AddWithValue("@ProductName", product.Name);
                
                var serviceId = await command.ExecuteScalarAsync();
                if(serviceId is null)
                    throw new NotFoundException($"Product with name: - {product.Name} - not found.");
                
                command.Parameters.Clear();
                command.CommandText = 
                    @"INSERT INTO product_delivery
                        VALUES(@IdProduct, @IdDelivery, @Amount);";
        
                command.Parameters.AddWithValue("@IdDelivery", deliveryRequest.DeliveryId);
                command.Parameters.AddWithValue("@IdProduct", serviceId);
                command.Parameters.AddWithValue("@Amount", product.Amount);
                
                await command.ExecuteNonQueryAsync();
            }
            
            await transaction.CommitAsync();
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            throw;
        }
        

    }
}