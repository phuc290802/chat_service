using ChatApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class DbTestController : ControllerBase
{
    private readonly AppDbContext _db;
    public DbTestController(AppDbContext db) => _db = db;

    [HttpGet]
    public IActionResult Test() => Ok(new { Connected = _db.Database.CanConnect() });
}
