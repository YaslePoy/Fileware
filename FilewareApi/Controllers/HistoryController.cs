using FilewareApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace FilewareApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HistoryController(FilewareDbContext db) : Controller
{
    [HttpGet]
    public ActionResult<IReadOnlyList<HistoryPoint>> GetHistory()
    {
        return Ok(db.HistoryPoints.ToList());
    }

    [HttpGet("{day}")]
    public ActionResult<IReadOnlyList<HistoryPoint>> GetByDate(DateTime day)
    {
        var nextDay = day.AddDays(1);
        return Ok(db.HistoryPoints.Where(i => i.Time >= day && i.Time < nextDay));
    }
}