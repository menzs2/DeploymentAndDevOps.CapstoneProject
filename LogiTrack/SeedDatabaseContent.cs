using LogiTrack.Models;
namespace LogiTrack;

public class SeedDatabaseContent
{
    private readonly OrderService _service;
    private readonly AuthService _authService;
    private readonly LogiTrackContext _context;
    public SeedDatabaseContent(OrderService service, AuthService authService, LogiTrackContext context)
    {
        _service = service;
        _authService = authService;
        _context = context;
    }

    public void Seed()
    {
        _context.Database.EnsureCreated();
        //seed roles if not already present
        _authService.InsertRole("Admin").Wait();
        _authService.InsertRole("User").Wait();
        _authService.InsertRole("Manager").Wait();
        _authService.InsertRole("Guest").Wait();


        // Seed users if not already present
         _authService.RegisterUserAsync(new ApplicationUser
            {
                Email = "joachim.murat@example.com",
                UserName = "joachim.murat@example.com",
            }, "Password123!", "Admin").Wait();
         _authService.RegisterUserAsync(new ApplicationUser
            {
                Email = "michel.neyh@example.com",
                UserName = "michel.neyh@example.com",
            }, "Password456!", "User").Wait();
        // Seed inventory items if not already present

        if (!_context.InventoryItems.Any())
        {
            _context.InventoryItems.AddRange(new List<InventoryItem>{
                new InventoryItem { Name = "Pallet Jack", Quantity = 12, Location = "Warehouse A" },
                new InventoryItem { Name = "Forklift", Quantity = 5, Location = "Warehouse B" },
                new InventoryItem { Name = "Hand Truck", Quantity = 20, Location = "Warehouse C" },
                new InventoryItem { Name = "Dolly", Quantity = 15, Location = "Warehouse D" },
                new InventoryItem { Name = "Conveyor Belt", Quantity = 8, Location = "Warehouse E" },
                new InventoryItem { Name = "Stacker", Quantity = 10, Location = "Warehouse F" },
                new InventoryItem { Name = "Crane", Quantity = 3, Location = "Warehouse G" } }
            );
            _context.SaveChanges();
        }

        if (!_context.Orders.Any())
        {
            _service.InsertOrder(new Order
            {
                CustomerName = "John Doe",
                DatePlaced = DateTime.UtcNow,
                OrderItems = new List<OrderItem>
                {
                    new OrderItem { InventoryItemId = 1, OrderedQuantity = 2 },
                    new OrderItem { InventoryItemId = 2, OrderedQuantity = 1 }
                }
            }).Wait();
            _service.InsertOrder(new Order
            {
                CustomerName = "Jane Smith",
                DatePlaced = DateTime.UtcNow,
                OrderItems = new List<OrderItem>
                {
                    new OrderItem { InventoryItemId = 3, OrderedQuantity = 5 },
                    new OrderItem { InventoryItemId = 4, OrderedQuantity = 3 }
                }
            }).Wait();
        }
    }
}
