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
    [SerializeField] private PlayerController _playerController;
    [SerializeField] private FloorController _floorController;

    public const int GRID_SIZE = 20;

    public const int DIR_UP = 0;
    public const int DIR_DOWN = 1;
    public const int DIR_LEFT = 2;
    public const int DIR_RIGHT = 3;

    public static readonly int[] DELTA_X = {0, 0, -1, 1};
    public static readonly int[] DELTA_Y = {-1, 1, 0, 0};

    private DieState _playerDie;

    // tileState:
    //  -1 = impassable (i.e., wall)
    //  0 = regular tile
    //  1-6 = powerup (with this label)
    public int[,] tileStates;


    void Start()
    {
        tileStates = new int[GRID_SIZE, GRID_SIZE];
        // Set some arbitrary walls, eventually this should be passed in.
        tileStates[1, 8] = -1;
        tileStates[9, 4] = -1;
        for (int i=0; i<17; i++) {
            tileStates[i, 5] = -1;
        }
        _playerDie = new DieState();
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

    bool isValidSquare(int x, int y) {
        return (x >= 0  && y >= 0 && x < GRID_SIZE && y < GRID_SIZE && tileStates[y,x] != -1);
    }

    // TODO: support multiple DieStates
    public bool MovePlayerToSquare(int x, int y) {
        if (!isValidSquare(x, y)) return false;

        _playerDie.SetPosition(x,  y);

        return true;
    }

    public bool PlayerRoll(int dir) {
        int nX = _playerDie.posX + DELTA_X[dir];
        int nY = _playerDie.posY + DELTA_Y[dir];

        if (!isValidSquare(nX, nY)) return false;

        switch (dir) {
            case DIR_UP:
                _playerDie.RollUp();
                break;
            
            case DIR_DOWN:
                _playerDie.RollDown();
                break;
            
            case DIR_LEFT:
                _playerDie.RollLeft();
                break;
            
            case DIR_RIGHT:
                _playerDie.RollRight();
                break;
        }

        CheckPowerup();
        _floorController.UpdateTargets();

        return true;
    }

    public void CheckPowerup() {
        if (_playerDie.GetBottom() == tileStates[_playerDie.posY, _playerDie.posX]) {
            _floorController.RemovePowerup(_playerDie.posX, _playerDie.posY);

            int puVal = _playerDie.GetBottom();

            if (!_playerDie.powered[puVal]) {
                _playerController.ApplyPowerup(puVal);
                _playerDie.PowerupFace(puVal);
            }
        }
    }

    public void ActivatePowerup() {
        if (!IsTopActive()) return;
        _playerController.UnapplyPowerup(_playerDie.GetTop());
        _playerDie.PowerdownFace(_playerDie.GetTop());
        _floorController.UpdateTargets();
        _floorController.ExplodeTiles(GetTiles(_playerDie.GetTop()));
    }

    public bool IsTopActive() {
        return _playerDie.IsPowered(_playerDie.GetTop());
    }

    public bool IsTargetableSquare(int x, int y) {
        if (!IsTopActive()) return false;
        int xDelta = x - _playerDie.posX;
        int yDelta = y - _playerDie.posY;
        return (Mathf.Abs(xDelta) + Mathf.Abs(yDelta) == _playerDie.GetTop());
    }

    // Gets all valid tiles at the given Manhattan
    public List<Tile> GetTiles(int distance) {
        List<Tile> validTiles = new List<Tile>();
        for (int i = -distance; i <= distance; i++) {
            for (int j = -distance; j <= distance; j++) {
                if (Mathf.Abs(i) + Mathf.Abs(j) != distance) continue;
                int newX = _playerDie.posX + i;
                int newY = _playerDie.posY + j;
                if (!isValidSquare(newX, newY)) continue;
                validTiles.Add(new Tile(newX, newY, 0));
            }
        }
        return validTiles;
    }
}
