using LogiTrack;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDbContext<LogiTrackContext>(options =>
    options.UseSqlite("Data Source=logitrack.db"));

// Add Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    // Enable Swagger UI in development
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
// using (var context = new LogiTrackContext())
// {
//     context.Database.EnsureCreated();
//     context.InventoryItems.ExecuteDelete(); // Clear existing items for testing
//     context.Orders.ExecuteDelete(); // Clear existing orders for testing
//                                     // Add test inventory item if none exist

//     context.InventoryItems.Add(new InventoryItem
//     {
//         Name = "Pallet Jack",
//         Quantity = 12,
//         Location = "Warehouse A"
//     });
//     context.InventoryItems.Add(new InventoryItem
//     {
//         Name = "Forklift",
//         Quantity = 5,
//         Location = "Warehouse B"
//     });
//     context.InventoryItems.Add(new InventoryItem
//     {
//         Name = "Hand Truck",
//         Quantity = 20,
//         Location = "Warehouse C"
//     });
//     context.SaveChanges();

//     var order = context.Orders.Add(new Order
//     {
//         CustomerName = "John Doe",
//         DatePlaced = DateTime.UtcNow,
//         Status = "Pending",
//         OrderItems = new List<OrderItem>()
//     });
//     order.Entity.AddItem(context.InventoryItems.First());
//     context.SaveChanges();


//     // Retrieve and print inventory to confirm
//     var items = context.InventoryItems.ToList();


//     foreach (var item in items)
//     {
//         Console.WriteLine(item.DisplayInfo()); // Should print: Item: Pallet Jack | Quantity: 12 | Location: Warehouse A
//     }
//     Console.WriteLine("Order Summary:");
//     Console.WriteLine(context.Orders.FirstOrDefault()?.GetOrderSummary() ?? "No orders found.");
// }
