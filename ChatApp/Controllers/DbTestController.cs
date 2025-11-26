using ChatApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

[ApiController]
[Route("api/[controller]")]
public class DbTestController : ControllerBase
{
    private readonly AppDbContext _db;
    public DbTestController(AppDbContext db) => _db = db;

    [HttpGet]
    public IActionResult Test() => Ok(new { Connected = _db.Database.CanConnect() });

    [Authorize]
    [HttpGet("me")]
    public IActionResult GetMe()
    {
        var subClaim = User.FindFirst(JwtRegisteredClaimNames.Sub);
        var userId = subClaim?.Value;

        // Log ra console
        Console.WriteLine($"Sub claim: {subClaim}");
        Console.WriteLine($"UserId: {userId}");

        return Ok(new { userId });
    }



}
