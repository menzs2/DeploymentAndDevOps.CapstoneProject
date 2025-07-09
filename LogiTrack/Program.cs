using LogiTrack;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDbContext<LogiTrackContext>(options =>
    options.UseSqlite("Data Source=logitrack.db"));

// Configure Identity with default options
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
.AddEntityFrameworkStores<LogiTrackContext>();

// Configure jwt authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Secret"]))
        };
    });

builder.Services.AddAuthorization();

// Register the OrderService as a scoped service
builder.Services.AddScoped<OrderService>();

// Register the AuthService as a scoped service and include UserManager, SignInManager, and RoleManager
// to handle user authentication and authorization
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<UserManager<ApplicationUser>>();
builder.Services.AddScoped<SignInManager<ApplicationUser>>();
builder.Services.AddScoped<RoleManager<IdentityRole>>();

// Register InMemoryCache
builder.Services.AddMemoryCache();

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
