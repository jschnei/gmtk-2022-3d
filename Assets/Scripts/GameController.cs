using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DieState {
    const bool VERBOSE = false;

    public int posX = -1;
    public int posY = -1;

    public int health = 3;

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

    public void GetHit() {
        health--;
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
    [SerializeField] private FloorController _floorController;
    [SerializeField] private TextAsset _levelData;

    public const int GRID_SIZE = 20;

    public const int DIR_UP = 0;
    public const int DIR_DOWN = 1;
    public const int DIR_LEFT = 2;
    public const int DIR_RIGHT = 3;

    public static readonly int[] DELTA_X = {0, 0, -1, 1};
    public static readonly int[] DELTA_Y = {-1, 1, 0, 0};

    private List<DieController> _dieControllers;
    private List<DieState> _dice;

    // public List<DieState> _enemyDice;

    // tileState:
    //  -1 = impassable (i.e., wall)
    //  0 = regular tile
    //  1-6 = powerup (with this label)
    public int[,] tileStates;

    void Awake() {       
        LoadLevel();
        
        // tileStates[1, 8] = -1;
        // tileStates[9, 4] = -1;
        // for (int i=0; i<17; i++) {
        //     tileStates[i, 5] = -1;
        // }

        _dieControllers = new List<DieController>();
        _dice = new List<DieState>();
    }

    void LoadLevel() {
        string data = _levelData.text;
        string[] lines = data.Split('\n');

        tileStates = new int[GRID_SIZE, GRID_SIZE];

        for (int y = 0; y < lines.Length; y++)
        {
            string line = lines[y];
            string[] pieces = line.Split(',');
            for (int x=0; x < pieces.Length; x++)
            {
                tileStates[y, x] = int.Parse(pieces[x]);
            }
        }
    }

    public int RegisterDie(DieController controller) {
        // shouldn't happen but just in case
        if (_dieControllers == null) _dieControllers = new List<DieController>();
        if (_dice == null) _dice = new List<DieState>(); 

        int id = _dice.Count;

        _dieControllers.Add(controller);
        
        DieState die = new DieState();
        _dice.Add(die);

        return id;
    }

    void Start()
    {
        // SpawnEnemy(10, 10);
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

    // public void SpawnEnemy(int x, int y) {
    //     DieState enemyState = new DieState();
    //     enemyState.SetPosition(x, y);
    //     _enemyDice.Add(enemyState);
    //     _floorController.SpawnEnemy(x, y);
    // }

    bool isValidSquare(int x, int y) {
        if (x < 0 || y < 0 || x >= GRID_SIZE || y >= GRID_SIZE || tileStates[y,x] == -1) return false;
        return true;
    }

    bool canMoveSquare(int x, int y) {
        if (!isValidSquare(x, y)) return false;

        foreach (DieState die in _dice) {
            if (die.posX == x && die.posY == y) return false;
        }

        return true;
    } 

    public bool MovePlayerToSquare(int x, int y, int p) {
        if (!canMoveSquare(x, y)) return false;

        _dice[p].SetPosition(x,  y);

        return true;
    }

    public bool PlayerRoll(int dir, int p) {
        int nX = _dice[p].posX + DELTA_X[dir];
        int nY = _dice[p].posY + DELTA_Y[dir];

        if (!canMoveSquare(nX, nY)) return false;

        switch (dir) {
            case DIR_UP:
                _dice[p].RollUp();
                break;
            
            case DIR_DOWN:
                _dice[p].RollDown();
                break;
            
            case DIR_LEFT:
                _dice[p].RollLeft();
                break;
            
            case DIR_RIGHT:
                _dice[p].RollRight();
                break;
        }

        CheckPowerup(p);
        _floorController.UpdateTargets();

        return true;
    }

    public void CheckPowerup(int i) {
        if (_dice[i].GetBottom() == tileStates[_dice[i].posY, _dice[i].posX]) {
            _floorController.RemovePowerup(_dice[i].posX, _dice[i].posY);

            int puVal = _dice[i].GetBottom();

            if (!_dice[i].powered[puVal]) {
                _dieControllers[i].ApplyPowerup(puVal);
                _dice[i].PowerupFace(puVal);
            }
        }
    }

    public void ActivatePowerup(int p) {
        if (!IsTopActive(p)) return;
        _dieControllers[p].UnapplyPowerup(_dice[p].GetTop());
        _dice[p].PowerdownFace(_dice[p].GetTop());
        _floorController.UpdateTargets();

        List<Tile> targets = GetTiles(_dice[p].GetTop(), p);
        AttackTargets(targets);
        _floorController.ExplodeTiles(targets);
    }

    public bool IsTopActive(int p) {
        return _dice[p].IsPowered(_dice[p].GetTop());
    }

    public bool IsTargetableSquare(int x, int y, int p) {
        if (!IsTopActive(p)) return false;
        int xDelta = x - _dice[p].posX;
        int yDelta = y - _dice[p].posY;
        return (Mathf.Abs(xDelta) + Mathf.Abs(yDelta) == _dice[p].GetTop());
    }

    public bool IsTargetableByAny(int x, int y) {
        for (int p = 0; p < _dice.Count; p++) {
            if (IsTargetableSquare(x, y, p)) return true;
        }
        return false;
    }

    public void AttackTargets(List<Tile> targets) {
        Debug.Log("Attacking targets!");
        foreach (Tile tile in targets) {
            for(int p = 0; p < _dice.Count; p++) {
                if (_dice[p].posX == tile.x && _dice[p].posY == tile.y) {
                    AttackPlayer(p);
                }
            }
        }
    }

    public void AttackPlayer(int p) {
        _dice[p].GetHit();

        Debug.Log("Die " + p + " hit! Health " + _dice[p].health);
    }

    // Gets all valid tiles at the given Manhattan
    public List<Tile> GetTiles(int distance, int p) {
        List<Tile> validTiles = new List<Tile>();
        for (int i = -distance; i <= distance; i++) {
            for (int j = -distance; j <= distance; j++) {
                if (Mathf.Abs(i) + Mathf.Abs(j) != distance) continue;
                int newX = _dice[p].posX + i;
                int newY = _dice[p].posY + j;
                if (!isValidSquare(newX, newY)) continue;
                validTiles.Add(new Tile(newX, newY, 0));
            }
        }
        return validTiles;
    }
}
