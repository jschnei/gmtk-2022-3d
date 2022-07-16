using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DieState {
    const bool VERBOSE = false;

    public int posX = 0;
    public int posY = 0;

    // faces stored as [top, bottom, front, back, left, right]
    const int D_TOP = 0;
    const int D_BOTTOM = 1;
    const int D_FRONT = 2;
    const int D_BACK = 3;
    const int D_LEFT = 4;
    const int D_RIGHT = 5;
    public int[] faces = {3, 4, 6, 1, 2, 5};
    public bool[] powered = {false, false, false, false, false, false, false};

    public void LogState() {
        if(VERBOSE) Debug.Log("x=" + posX + ", y=" + posY + ", top=" + faces[D_TOP]);
    }

    public void SetPosition(int x, int y) {
        posX = x;
        posY = y;

        LogState();
    }

    public void RollUp() {
        int tmp_top = faces[D_TOP];
        faces[D_TOP] = faces[D_FRONT];
        faces[D_FRONT] = faces[D_BOTTOM];
        faces[D_BOTTOM] = faces[D_BACK];
        faces[D_BACK] = tmp_top;

        posY--;

        LogState();
    }

    public void RollDown() {
        int tmp_top = faces[D_TOP];
        faces[D_TOP] = faces[D_BACK];
        faces[D_BACK] = faces[D_BOTTOM];
        faces[D_BOTTOM] = faces[D_FRONT];
        faces[D_FRONT] = tmp_top;

        posY++;

        LogState();
    }

    public void RollLeft() {
        int tmp_top = faces[D_TOP];
        faces[D_TOP] = faces[D_RIGHT];
        faces[D_RIGHT] = faces[D_BOTTOM];
        faces[D_BOTTOM] = faces[D_LEFT];
        faces[D_LEFT] = tmp_top;

        posX--;

        LogState();
    }

    public void RollRight() {
        int tmp_top = faces[D_TOP];
        faces[D_TOP] = faces[D_LEFT];
        faces[D_LEFT] = faces[D_BOTTOM];
        faces[D_BOTTOM] = faces[D_RIGHT];
        faces[D_RIGHT] = tmp_top;

        posX++;

        LogState();
    }

    public int GetBottom() {
        return faces[D_BOTTOM];
    }

    public int GetTop() {
        return faces[D_TOP];
    }

    public void PowerupFace(int face) {
        powered[face] = true;
    }

    public void PowerdownFace(int face) {
        powered[face] = false;
    }

    public bool IsPowered(int face) {
        return powered[face];
    }
}

public class Tile {
    public int x, y, value;

    public Tile(int x, int y, int value) {
        this.x = x;
        this.y = y;
        this.value = value;
    }
}

// GameController should manage all discrete game logic
public class GameController : MonoBehaviour
{
    [SerializeField] private PlayerController[] _playerController;
    [SerializeField] private FloorController _floorController;

    public const int GRID_SIZE = 20;

    public const int DIR_UP = 0;
    public const int DIR_DOWN = 1;
    public const int DIR_LEFT = 2;
    public const int DIR_RIGHT = 3;

    public static readonly int[] DELTA_X = {0, 0, -1, 1};
    public static readonly int[] DELTA_Y = {-1, 1, 0, 0};

    private DieState[] _playerDie = new DieState[2];

    public List<DieState> _enemyDice;

    // tileState:
    //  -1 = impassable (i.e., wall)
    //  0 = regular tile
    //  1-6 = powerup (with this label)
    //  10 = enemy
    public int[,] tileStates;

    void Awake() {
        tileStates = new int[GRID_SIZE, GRID_SIZE];
        // Set some arbitrary walls, eventually this should be passed in.
        tileStates[1, 8] = -1;
        tileStates[9, 4] = -1;
        for (int i=0; i<17; i++) {
            tileStates[i, 5] = -1;
        }
        _playerDie[0] = new DieState();
        _playerDie[1] = new DieState();
        _enemyDice = new List<DieState>();
    }

    void Start()
    {
        SpawnEnemy(10, 10);
    }

    public const int SPAWN_RETRIES = 10;
    public Tile SpawnPowerup() {
        for (int i=0; i<SPAWN_RETRIES; i++) {
            int randX = (int)(Random.value * GRID_SIZE);
            int randY = (int)(Random.value * GRID_SIZE);

            if (tileStates[randY, randX] == 0) {
                int randVal = (int)(Random.value * 6) + 1;
                tileStates[randY, randX] = randVal;

                return new Tile(randX, randY, randVal);
            }
        }

        return new Tile(-1, -1, -1);
    }

    public void SpawnEnemy(int x, int y) {
        DieState enemyState = new DieState();
        enemyState.SetPosition(x, y);
        _enemyDice.Add(enemyState);
        tileStates[y, x] = 10;
        _floorController.SpawnEnemy(x, y);
    }

    // TODO: check collision between different players
    bool isValidSquare(int x, int y) {
        return (x >= 0  && y >= 0 && x < GRID_SIZE && y < GRID_SIZE && tileStates[y,x] != -1);
    }

    // TODO: support multiple DieStates
    public bool MovePlayerToSquare(int x, int y, int p) {
        if (!isValidSquare(x, y)) return false;

        _playerDie[p].SetPosition(x,  y);

        return true;
    }

    public bool PlayerRoll(int dir, int p) {
        int nX = _playerDie[p].posX + DELTA_X[dir];
        int nY = _playerDie[p].posY + DELTA_Y[dir];

        if (!isValidSquare(nX, nY)) return false;

        switch (dir) {
            case DIR_UP:
                _playerDie[p].RollUp();
                break;
            
            case DIR_DOWN:
                _playerDie[p].RollDown();
                break;
            
            case DIR_LEFT:
                _playerDie[p].RollLeft();
                break;
            
            case DIR_RIGHT:
                _playerDie[p].RollRight();
                break;
        }

        CheckPowerup(p);
        _floorController.UpdateTargets(p);

        return true;
    }

    public void CheckPowerup(int i) {
        if (_playerDie[i].GetBottom() == tileStates[_playerDie[i].posY, _playerDie[i].posX]) {
            _floorController.RemovePowerup(_playerDie[i].posX, _playerDie[i].posY);

            int puVal = _playerDie[i].GetBottom();

            if (!_playerDie[i].powered[puVal]) {
                _playerController[i].ApplyPowerup(puVal);
                _playerDie[i].PowerupFace(puVal);
            }
        }
    }

    public void ActivatePowerup(int p) {
        if (!IsTopActive(p)) return;
        _playerController[p].UnapplyPowerup(_playerDie[p].GetTop());
        _playerDie[p].PowerdownFace(_playerDie[p].GetTop());
        _floorController.UpdateTargets(p);
        _floorController.ExplodeTiles(GetTiles(_playerDie[p].GetTop(), p));
    }

    public bool IsTopActive(int p) {
        return _playerDie[p].IsPowered(_playerDie[p].GetTop());
    }

    public bool IsTargetableSquare(int x, int y, int p) {
        if (!IsTopActive(p)) return false;
        int xDelta = x - _playerDie[p].posX;
        int yDelta = y - _playerDie[p].posY;
        return (Mathf.Abs(xDelta) + Mathf.Abs(yDelta) == _playerDie[p].GetTop());
    }

    // Gets all valid tiles at the given Manhattan
    public List<Tile> GetTiles(int distance, int p) {
        List<Tile> validTiles = new List<Tile>();
        for (int i = -distance; i <= distance; i++) {
            for (int j = -distance; j <= distance; j++) {
                if (Mathf.Abs(i) + Mathf.Abs(j) != distance) continue;
                int newX = _playerDie[p].posX + i;
                int newY = _playerDie[p].posY + j;
                if (!isValidSquare(newX, newY)) continue;
                validTiles.Add(new Tile(newX, newY, 0));
            }
        }
        return validTiles;
    }
}
