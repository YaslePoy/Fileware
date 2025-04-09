using FilewareApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FilewareApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HistoryController(FilewareDbContext db) : Controller
{
    [HttpGet]
    public ActionResult<IReadOnlyList<HistoryPoint>> GetHistoryAfterId(int id, int count)
    {
        List<HistoryPoint> mapLinked(List<HistoryPoint> list)
        {
            foreach (var point in list)
            {
                if (point.Type == (int)HistoryPointType.Message)
                {
                    point.Linked = db.Messages.FirstOrDefault(i => i.Id == point.LinkedId);
                }
                else
                {
                    point.Linked = db.FileData.FromSql($"""
                                                        SELECT f.Id, null as Data, f.FileType, f.LastChange, f.LoadTime, f.Name, f.Size, f.Version, f.Preview
                                                        FROM FileData AS f
                                                        WHERE f.Id = {point.LinkedId}
                                                        """).ToList()
                        .First();
                }
            }

            return list;
        }

        if (id == -1)
            return Ok(mapLinked(db.HistoryPoints.OrderByDescending(i => i.Id).Take(count).ToList()));
        return Ok(mapLinked(db.HistoryPoints.Where(i => i.Id < id).OrderByDescending(i => i.Id).Take(count).ToList()));
    }
}