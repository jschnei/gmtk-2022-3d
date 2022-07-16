using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class DieState {
    const bool VERBOSE = false;
    public const int MAX_HEALTH = 3;
    public int posX = -1;
    public int posY = -1;
    public int prevPosX = -1;
    public int prevPosY = -1;

    public int health = MAX_HEALTH;
    public bool isDead = false;
    public int totalPowerups = 0;

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

    public void SavePreviousPosition() {
        prevPosX = posX;
        prevPosY = posY;
    }

    public void ClearPreviousPosition() {
        prevPosX = -1;
        prevPosY = -1;
    }

    public void RollUp() {
        SavePreviousPosition();
        int tmp_top = faces[D_TOP];
        faces[D_TOP] = faces[D_FRONT];
        faces[D_FRONT] = faces[D_BOTTOM];
        faces[D_BOTTOM] = faces[D_BACK];
        faces[D_BACK] = tmp_top;

        posY--;

        LogState();
    }

    public void RollDown() {
        SavePreviousPosition();
        int tmp_top = faces[D_TOP];
        faces[D_TOP] = faces[D_BACK];
        faces[D_BACK] = faces[D_BOTTOM];
        faces[D_BOTTOM] = faces[D_FRONT];
        faces[D_FRONT] = tmp_top;

        posY++;

        LogState();
    }

    public void RollLeft() {
        SavePreviousPosition();
        int tmp_top = faces[D_TOP];
        faces[D_TOP] = faces[D_RIGHT];
        faces[D_RIGHT] = faces[D_BOTTOM];
        faces[D_BOTTOM] = faces[D_LEFT];
        faces[D_LEFT] = tmp_top;

        posX--;

        LogState();
    }

    public void RollRight() {
        SavePreviousPosition();
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

    public void IncrementPowerupCount() {
        totalPowerups += 1;
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

    // public const int GRID_SIZE = 20;

    public int gridWidth;
    public int gridHeight;

    public const int DIR_UP = 0;
    public const int DIR_DOWN = 1;
    public const int DIR_LEFT = 2;
    public const int DIR_RIGHT = 3;

    public static readonly int[] DELTA_X = {0, 0, -1, 1};
    public static readonly int[] DELTA_Y = {-1, 1, 0, 0};

    // TODO: move to UI controller?
    [SerializeField] private Image healthBarP1;
    [SerializeField] private Image healthBarP2;

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

        _dieControllers = new List<DieController>();
        _dice = new List<DieState>();
    }

    void LoadLevel() {
        string data = _levelData.text;
        string[] lines = data.Split('\n');

        string[] dims = lines[0].Split(',');
        gridWidth = int.Parse(dims[0]);
        gridHeight = int.Parse(dims[1]);

        Debug.Log(gridHeight + " " + gridWidth);
        tileStates = new int[gridHeight, gridWidth];

        for (int y = 0; y < gridHeight; y++)
        {
            string line = lines[y+1];
            // string[] pieces = line.Split(',');
            for (int x=0; x < gridWidth; x++)
            {
                if (line[x] == '#') {
                    tileStates[y, x] = -1;
                } else if (line[x] == 'R') {
                    // make new red enemy at (x, y)
                    SpawnDie(x, y, DieController.PTYPE_ENEMY);
                } else if (line[x] == '1') {
                    // player 1 spawns at ()
                    SpawnDie(x, y, DieController.PTYPE_PLAYER_ONE);
                } else if (line[x] == '2') {
                    // player 1 spawns at ()
                    SpawnDie(x, y, DieController.PTYPE_PLAYER_TWO);
                }
                // tileStates[y, x] = int.Parse(pieces[x]);
            }
        }

        _floorController.InitializeFloor(gridWidth, gridHeight);
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
    }

    public const int SPAWN_RETRIES = 10;
    public Tile SpawnPowerup() {
        for (int i=0; i<SPAWN_RETRIES; i++) {
            int randX = (int)(Random.value * gridWidth);
            int randY = (int)(Random.value * gridHeight);

            if (tileStates[randY, randX] == 0) {
                int randVal = (int)(Random.value * 6) + 1;
                tileStates[randY, randX] = randVal;

                return new Tile(randX, randY, randVal);
            }
        }

        return new Tile(-1, -1, -1);
    }

    public void SpawnDie(int x, int y, int type) {
        _floorController.SpawnDie(x, y, type);
    }

    bool isValidSquare(int x, int y) {
        if (x < 0 || y < 0 || x >= gridWidth || y >= gridHeight || tileStates[y,x] == -1) return false;
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
        if (_dice[p].isDead) return false;
        if (!canMoveSquare(x, y)) return false;

        _dice[p].SetPosition(x,  y);

        return true;
    }

    public bool PlayerRoll(int dir, int p) {
        if (_dice[p].isDead) return false;

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

    public void FinishRoll(int p) {
        _dice[p].ClearPreviousPosition();
    }

    public void CheckPowerup(int p) {
        if (_dice[p].isDead) return;

        if (_dice[p].GetBottom() == tileStates[_dice[p].posY, _dice[p].posX]) {
            _floorController.RemovePowerup(_dice[p].posX, _dice[p].posY);
            tileStates[_dice[p].posY, _dice[p].posX] = 0;
            _dice[p].IncrementPowerupCount();

            int puVal = _dice[p].GetBottom();

            if (!_dice[p].powered[puVal]) {
                _dieControllers[p].ApplyPowerup(puVal);
                _dice[p].PowerupFace(puVal);
            }
        }
    }

    public void ActivatePowerup(int p) {
        if (_dice[p].isDead) return;
        if (!IsTopActive(p)) return;

        _dieControllers[p].UnapplyPowerup(_dice[p].GetTop());
        _dice[p].PowerdownFace(_dice[p].GetTop());
        _floorController.UpdateTargets();

        List<Tile> targets = GetTiles(_dice[p].GetTop(), p);
        AttackTargets(targets);
        _floorController.ExplodeTiles(targets);
    }

    public bool IsTopActive(int p) {
        if (_dice[p].isDead) return false;

        return _dice[p].IsPowered(_dice[p].GetTop());
    }

    public bool IsTargetableSquare(int x, int y, int p) {
        if (_dice[p].isDead) return false;
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
        foreach (Tile tile in targets) {
            for(int p = 0; p < _dice.Count; p++) {
                if (_dice[p].isDead) continue;
                bool playerOnTile = _dice[p].posX == tile.x && _dice[p].posY == tile.y;
                bool movingPlayerWasOnTile = _dieControllers[p].IsMoving() && _dice[p].prevPosX == tile.x && _dice[p].prevPosY == tile.y;
                if (playerOnTile || movingPlayerWasOnTile) {
                    AttackPlayer(p);
                }
            }
        }
    }

    public void EnemyAttack(int attack, int p) {
        if (_dice[p].isDead) return;
        List<Tile> targets = GetEnemyAttackTargets(attack, p);

        if (targets.Count == 0) return;

        AttackTargets(targets);
        _floorController.ExplodeTiles(targets);
    }

    public List<Tile> GetEnemyAttackTargets(int attack, int p) {
        List<Tile> validTiles = new List<Tile>();
        if (attack == DieController.INPUT_RED_ATTACK_1) {
            for (int i = -1; i <= 1; i++) {
                for (int j = -1; j <= 1; j++) {
                    if (i == 0 && j == 0) continue;
                    int newX = _dice[p].posX + i;
                    int newY = _dice[p].posY + j;
                    if (!isValidSquare(newX, newY)) continue;
                    validTiles.Add(new Tile(newX, newY, 0));
                }
            }
        }

        return validTiles;
    }

    public void AttackPlayer(int p) {
        if (_dice[p].isDead) return;

        _dice[p].GetHit();

        AdjustHealthbar(_dieControllers[p].playerType, _dice[p].health);
        // if (_dieControllers[p].playerType == DieController.PTYPE_PLAYER_ONE) {
        //     AdjustHealthbar()
        // }
        // _dieControllers[p].AdjustHealthbar(_dice[p].health);
        Debug.Log("Die " + p + " hit! Health " + _dice[p].health);

        if (_dice[p].health == 0) {
            KillPlayer(p);
        }
    }

    public void KillPlayer(int p) {
        if (_dice[p].isDead) return;

        _dice[p].isDead = true;
        _dice[p].posX = -1;
        _dice[p].posY = -1;
        _dieControllers[p].Die();
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

    public void AdjustHealthbar(int ptype, int health) {
        if (ptype == DieController.PTYPE_PLAYER_ONE) {
            healthBarP1.fillAmount = Mathf.Clamp((float)health / DieState.MAX_HEALTH, 0, 1f);
        } else if (ptype == DieController.PTYPE_PLAYER_TWO) {
            healthBarP2.fillAmount = Mathf.Clamp((float)health / DieState.MAX_HEALTH, 0, 1f);
        }
    }
}
