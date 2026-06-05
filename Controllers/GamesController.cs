using Battleship.Data;
using Battleship.Domain;
using Battleship.Dtos;
using Battleship.Models;
using Microsoft.AspNetCore.Mvc;

namespace Battleship.Controllers;

[ApiController]
[Route("games")]
public class GamesController : ControllerBase
{
    private static readonly Dictionary<string, Game> ActiveGames = new();
    private static readonly object LockObject = new();
    private readonly BattleshipContext _dbContext;
    private readonly IRandomProvider _randomProvider;

    public GamesController(BattleshipContext dbContext, IRandomProvider randomProvider)
    {
        _dbContext = dbContext;
        _randomProvider = randomProvider;
    }

    [HttpPost]
    public ActionResult<CreateGameResponse> CreateGame([FromQuery] int boardSize = 10)
    {
        if (boardSize < 5 || boardSize > 20)
        {
            return BadRequest("Board size must be between 5 and 20.");
        }

        var gameId = Guid.NewGuid().ToString("N");
        var game = new Game(gameId, boardSize, _randomProvider);

        lock (LockObject)
        {
            ActiveGames[gameId] = game;
        }

        return CreatedAtAction(nameof(GetGameState), new { id = gameId }, new CreateGameResponse
        {
            GameId = gameId,
            BoardSize = boardSize
        });
    }

    [HttpPost("{id}/shots")]
    public ActionResult<ShotResponse> FireShot(string id, [FromBody] FireShotRequest request)
    {
        if (request is null)
        {
            return BadRequest("Request body is required.");
        }

        Game? game;
        lock (LockObject)
        {
            ActiveGames.TryGetValue(id, out game);
        }

        if (game is null)
        {
            return NotFound();
        }

        if (game.IsWon)
        {
            return BadRequest("The game is already finished.");
        }

        var position = new BoardPosition(request.X, request.Y);
        if (!game.Board.IsInBounds(position))
        {
            return BadRequest("Shot coordinates are out of bounds.");
        }

        var result = game.Board.Fire(position);
        var isWon = game.IsWon;

        if (isWon)
        {
            PersistCompletedGame(game);
        }

        return Ok(new ShotResponse
        {
            Outcome = result.Outcome.ToString().ToLowerInvariant(),
            ShipName = result.ShipName,
            ShotsFired = game.Board.ShotsFired,
            ShipsRemaining = game.Board.ShipsRemaining,
            IsWon = isWon
        });
    }

    [HttpGet("{id}")]
    public ActionResult<GameStateResponse> GetGameState(string id)
    {
        Game? game;
        lock (LockObject)
        {
            ActiveGames.TryGetValue(id, out game);
        }

        if (game is null)
        {
            return NotFound();
        }

        var response = new GameStateResponse
        {
            GameId = game.Id,
            BoardSize = game.Board.Size,
            ShotsFired = game.Board.ShotsFired,
            ShipsRemaining = game.Board.ShipsRemaining,
            IsWon = game.IsWon,
            Shots = game.Board.Shots.Select(pair => new ShotHistoryItem
            {
                X = pair.Key.X,
                Y = pair.Key.Y,
                Outcome = pair.Value.Outcome.ToString().ToLowerInvariant(),
                ShipName = pair.Value.ShipName
            }).ToList()
        };

        return Ok(response);
    }

    private void PersistCompletedGame(Game game)
    {
        if (_dbContext.GameSummaries.Any(summary => summary.GameId == game.Id))
        {
            return;
        }

        _dbContext.GameSummaries.Add(new GameSummary
        {
            GameId = game.Id,
            BoardSize = game.Board.Size,
            ShipCount = game.Board.Ships.Count,
            TotalShots = game.Board.ShotsFired,
            CompletedAt = DateTime.UtcNow
        });

        _dbContext.SaveChanges();
    }
}
