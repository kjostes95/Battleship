const boardContainer = document.getElementById('board-container');
const statusEl = document.getElementById('status');
const noticeEl = document.getElementById('notice');
const summaryEl = document.getElementById('summary');
const leaderboardContainer = document.getElementById('leaderboard-container');
const newGameButton = document.getElementById('new-game');
const refreshLeaderboardButton = document.getElementById('refresh-leaderboard');

let gameId = null;
let boardSize = 10;
let boardState = {};
let isGameOver = false;

function initialize() {
    if (newGameButton) {
        newGameButton.addEventListener('click', createGame);
    }

    if (refreshLeaderboardButton) {
        refreshLeaderboardButton.addEventListener('click', async () => {
            statusEl.textContent = 'Refreshing leaderboard...';
            await loadLeaderboard();
            statusEl.textContent = 'Leaderboard refreshed.';
        });
    } else {
        console.warn('Refresh leaderboard button not found');
    }

    createGame();
    loadLeaderboard();
}

window.addEventListener('DOMContentLoaded', initialize);

async function createGame() {
    statusEl.textContent = 'Creating game...';
    const response = await fetch('/games', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' }
    });

    if (!response.ok) {
        statusEl.textContent = 'Unable to create game.';
        return;
    }

    const result = await response.json();
    gameId = result.gameId;
    boardSize = result.boardSize;
    boardState = {};
    isGameOver = false;
    statusEl.textContent = `Game ID: ${gameId}`;
    summaryEl.textContent = '';
    noticeEl.textContent = '';
    renderBoard();
}

function renderBoard() {
    boardContainer.innerHTML = '';
    const grid = document.createElement('div');
    grid.className = 'board';
    grid.style.gridTemplateColumns = `repeat(${boardSize}, minmax(34px, 1fr))`;

    for (let y = 0; y < boardSize; y++) {
        for (let x = 0; x < boardSize; x++) {
            const cell = document.createElement('button');
            cell.type = 'button';
            cell.className = 'cell';
            cell.dataset.x = x;
            cell.dataset.y = y;
            const key = `${x},${y}`;
            const shot = boardState[key];
            if (shot) {
                cell.classList.add(shot.outcome);
                cell.textContent = shot.outcome === 'miss' ? '•' : 'X';
                cell.disabled = true;
            }
            cell.addEventListener('click', () => fireShot(x, y));
            grid.appendChild(cell);
        }
    }

    boardContainer.appendChild(grid);
}

async function loadLeaderboard() {
    if (!leaderboardContainer) {
        console.error('Leaderboard container not found');
        return;
    }

    leaderboardContainer.innerHTML = '<p>Loading leaderboard...</p>';

    try {
        const response = await fetch('/summaries');
        if (!response.ok) {
            const message = `Error loading leaderboard: ${response.status} ${response.statusText}`;
            leaderboardContainer.innerHTML = `<p>${message}</p>`;
            statusEl.textContent = message;
            return;
        }

        const leaderboard = await response.json();
        renderLeaderboard(leaderboard);
    } catch (error) {
        const message = `Error loading leaderboard: ${error.message}`;
        leaderboardContainer.innerHTML = `<p>${message}</p>`;
        statusEl.textContent = message;
    }
}

function renderLeaderboard(items) {
    if (!Array.isArray(items) || items.length === 0) {
        leaderboardContainer.innerHTML = '<p>No completed games yet.</p>';
        return;
    }

    const table = document.createElement('table');
    table.className = 'leaderboard';

    const thead = document.createElement('thead');
    thead.innerHTML = `
        <tr>
            <th>Game ID</th>
            <th>Board</th>
            <th>Ships</th>
            <th>Shots</th>
            <th>Completed At</th>
        </tr>
    `;
    table.appendChild(thead);

    const tbody = document.createElement('tbody');
    items.forEach(item => {
        const row = document.createElement('tr');
        row.innerHTML = `
            <td>${item.gameId}</td>
            <td>${item.boardSize}</td>
            <td>${item.shipCount}</td>
            <td>${item.totalShots}</td>
            <td>${new Date(item.completedAt).toLocaleString()}</td>
        `;
        tbody.appendChild(row);
    });

    table.appendChild(tbody);
    leaderboardContainer.innerHTML = '<h2>Leaderboard</h2>';
    leaderboardContainer.appendChild(table);
}

async function fireShot(x, y) {    if (!gameId) {
        statusEl.textContent = 'Create a game first.';
        return;
    }

    const previousStatus = statusEl.textContent;
    statusEl.textContent = `Firing shot at (${x}, ${y})...`;

    const response = await fetch(`/games/${gameId}/shots`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ x, y })
    });

    if (!response.ok) {
        const errorText = await response.text();
        if (isGameOver) {
            noticeEl.textContent = 'Rejected, game has ended';
            statusEl.textContent = previousStatus;
        } else {
            noticeEl.textContent = '';
            statusEl.textContent = `Error: ${errorText}`;
        }
        return;
    }

    noticeEl.textContent = '';
    const result = await response.json();
    const key = `${x},${y}`;
    boardState[key] = {
        outcome: result.outcome,
        shipName: result.shipName
    };

    if (result.outcome === 'sunk' && result.shipName) {
        for (const shotKey of Object.keys(boardState)) {
            const shot = boardState[shotKey];
            if (shot.shipName === result.shipName) {
                shot.outcome = 'sunk';
            }
        }
    }

    if (result.isWon) {
        isGameOver = true;
        summaryEl.textContent = 'You sank the fleet! You win!!!';
        await loadLeaderboard();
    }
    renderBoard();

    let display = `${result.outcome.toUpperCase()} at (${x}, ${y})`;
    if (result.shipName) {
        display += ` — ${result.shipName}`;
    }
    display += `. Shots: ${result.shotsFired}, Ships left: ${result.shipsRemaining}`;
    statusEl.textContent = display;
}

createGame();
loadLeaderboard();
