# Battleship

## Overview

This repository contains a .NET 10 Battleship game backend with a thin static frontend. The backend is implemented as an ASP.NET Core Web API with a separate xUnit test project.

## Architecture

### Layering

- `Controllers/` contains the API entry points.
  - `GamesController` handles game creation, firing shots, and retrieving game state.
  - `SummariesController` exposes completed game summaries for leaderboard data.
- `Domain/` contains core game logic and rules.
  - `Board`, `Ship`, `BoardPosition`, `Game`, and shot resolution logic live here.
- `Data/` contains persistence infrastructure.
  - `BattleshipContext` is the EF Core DbContext used to store completed game summaries in SQLite.
- `Dtos/` contains request/response shapes shared by the API.
- `Models/` contains persisted domain models such as `GameSummary`.
- `wwwroot/` contains the static frontend assets.

### Where state lives

- Active game state is held in memory inside the API while a game is in progress.
  - This keeps fast game interaction simple and avoids serializing the full board state on each shot.
- Completed game metadata is persisted to SQLite using EF Core.
  - Only summary data is stored permanently, so finished games can be shown in a leaderboard.

## Randomness and Testability

Random ship placement is abstracted behind `IRandomProvider`.

- `RandomProvider` is used in production to generate unpredictable board layouts.
- Tests use a deterministic mock implementation (`MockRandomProvider`) so board placement can be repeated reliably.

This makes randomness testable by:

- isolating random number generation behind an interface,
- injecting a deterministic provider into the game board constructor during tests,
- verifying board placement, shot behavior, and win conditions against stable outcomes.

## Tradeoffs and Incomplete Areas

### Tradeoffs

- In-memory active game state is simple, but not horizontally scalable.
  - If the app restarts, in-progress games are lost.
  - For a full production service, game sessions would need durable storage or a distributed cache.
- Only completed game summaries are persisted.
  - This keeps storage small, but it means there is no replay/history for individual games.
- The frontend is intentionally thin and static.
  - It is good for quick demos and manual play, but not a full client-side app.

### Incomplete / Future Improvements

- Persisting full game state so users can resume interrupted games.
- Adding player accounts or session IDs for multi-user tracking.
- More robust edge-case handling for malformed requests and invalid shots.
- Better frontend UX and board rendering for mobile devices.
- More extensive test coverage for controller endpoints and error paths.
- Replace the in-memory active-state store with a scalable session store or database.

## Build and Run

```bash
cd Battleship
dotnet build
dotnet test Battleship.Tests/Battleship.Tests.csproj
dotnet run
```

Then open the frontend at `http://localhost:5000`.

## Notes

- The project uses `net10.0`.
- `Battleship.Tests` is a separate xUnit project.
- `.gitignore` excludes build artifacts, binaries, and local database files.
- **Tests**: `Battleship.Tests` contains deterministic xUnit tests that use `MockRandomProvider` for repeatable board placement.
