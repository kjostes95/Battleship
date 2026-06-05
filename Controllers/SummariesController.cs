using Battleship.Data;
using Battleship.Models;
using Microsoft.AspNetCore.Mvc;

namespace Battleship.Controllers;

[ApiController]
[Route("summaries")]
public class SummariesController : ControllerBase
{
    private readonly BattleshipContext _dbContext;

    public SummariesController(BattleshipContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public ActionResult<IEnumerable<GameSummary>> GetCompletedSummaries()
    {
        return Ok(_dbContext.GameSummaries.OrderByDescending(summary => summary.CompletedAt).ToList());
    }
}
