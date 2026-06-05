namespace Battleship.Dtos;

public sealed class CreateGameResponse
{
    public string GameId { get; set; } = null!;
    public int BoardSize { get; set; }
}
