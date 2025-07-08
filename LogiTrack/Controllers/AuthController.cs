using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LogiTrack;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[ProducesResponseType(StatusCodes.Status200OK)]
public class AuthController : ControllerBase
{
    private readonly LogiTrackContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AuthController(LogiTrackContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _context = context;
        _userManager = userManager;
        _signInManager = signInManager;
    }


    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Login([FromBody] Microsoft.AspNetCore.Identity.Data.LoginRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
        {
            return BadRequest("Invalid login request.");
        }

        // Here you would typically validate the user credentials against a database
        // For simplicity, we are returning a dummy response
        if (request.Email == "admin@example.com" && request.Password == "password")
        {
            return Ok(new { Token = "dummy-jwt-token" });
        }

        return BadRequest("Invalid username or password.");
    }

    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Register([FromBody] Microsoft.AspNetCore.Identity.Data.RegisterRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
        {
            return BadRequest("Invalid registration request.");
        }

        // Here you would typically save the user to a database
        // For simplicity, we are returning a dummy response
        return CreatedAtAction(nameof(Register), new { Email = request.Email }, new { Message = "User registered successfully." });
    }
}
