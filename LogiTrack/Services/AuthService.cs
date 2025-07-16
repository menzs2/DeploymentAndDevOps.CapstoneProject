using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace LogiTrack;

public class AuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfigurationSection _configuration;

    public AuthService(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IConfigurationSection configuration)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
        _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public async Task<IdentityResult> RegisterUserAsync(ApplicationUser user, string password, string? role = null)
    {
        if (user == null || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(user.Email))
        {
            throw new ArgumentException("User and password must be provided.");
        }
        if (await _userManager.FindByEmailAsync(user.Email) != null)
        {
            throw new InvalidOperationException("User with this email already exists.");
        }
        //hash the password and create the user
        user.UserName = user.Email; // Ensure UserName is set to Email for consistency
        user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, password);
        
        var result = await _userManager.CreateAsync(user);
        if (result.Succeeded)
        {
            // Optionally assign a default role
            if (string.IsNullOrEmpty(role))
            {
                role = "User"; // Default to User role if none specified
            }
            if (await _roleManager.RoleExistsAsync(role))
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    //return an error if the role does not exist
                    role = "Guest"; // Default to Guest if role does not exist);
                }
                await _userManager.AddToRoleAsync(user, role);
                return result;
            }
            // If no role is specified, assign a default role
            if (!await _roleManager.RoleExistsAsync("User"))
            {
                await _roleManager.CreateAsync(new IdentityRole("User"));
            }
            await _userManager.AddToRoleAsync(user, "User");
        }

        return result;
    }

    public async Task<SignInResult> LoginAsync(string email, string password)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            throw new ArgumentException("Email and password must be provided.");
        }

        return await _signInManager.PasswordSignInAsync(email, password, isPersistent: true, lockoutOnFailure: false);
    }


    public async Task LogoutAsync()
    {
        await _signInManager.SignOutAsync();
    }

    // Add methods for login, registration, etc. here
    public async Task<string> GenerateJwtTokenAsync(ApplicationUser user)
    {

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
            new Claim(ClaimTypes.Role, (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? "User") // Default to User role if none assigned
        };
        var secret = _configuration["Secret"];
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)); // Use a secure key from config
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Issuer"],
            audience: _configuration["Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(30),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    internal async Task<ApplicationUser?> GetUserByEmailAsync(string email)
    {
        return await _userManager.FindByEmailAsync(email);
    }
    
    internal async Task InsertRole(string roleName)
    {
        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            await _roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }
}
