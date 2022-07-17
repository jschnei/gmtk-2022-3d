using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class DieState {
    const bool VERBOSE = false;
    public const int MAX_HEALTH = 3;
    public int posX = -1;
    public int posY = -1;
    public int prevPosX = -1;
    public int prevPosY = -1;

    public int health = MAX_HEALTH;
    public bool isDead = false;
    public int powerupsCollected = 0;

    // faces stored as [top, bottom, front, back, left, right]
    public const int D_TOP = 0;
    public const int D_BOTTOM = 1;
    public const int D_FRONT = 2;
    public const int D_BACK = 3;
    public const int D_LEFT = 4;
    public const int D_RIGHT = 5;
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
        powerupsCollected += 1;
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

    // copies just relevant fields of state for BFS computation
    public DieState CopyState() {
        DieState nstate = new DieState();

        nstate.posX = posX;
        nstate.posY = posY;
        for (int i=0;i<6;i++) nstate.faces[i] = faces[i];

        return nstate;
    }

    // used for AI purposes
    public string Serialize() {
        string ans = posX + "," + posY + ",";
        for (int i=0;i<6;i++) {
            ans += faces[i] + ",";
        }

        return ans;
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
    [SerializeField] private TextAsset _defaultLevelData;
    [SerializeField] private TextAsset _raceLevelData;
    [SerializeField] private TextAsset _powerwashLevelData;

    // public const int GRID_SIZE = 20;

    public int gridWidth;
    public int gridHeight;

    public const int DIR_UP = 0;
    public const int DIR_DOWN = 1;
    public const int DIR_LEFT = 2;
    public const int DIR_RIGHT = 3;

    public static readonly int[] DELTA_X = {0, 0, -1, 1};
    public static readonly int[] DELTA_Y = {-1, 1, 0, 0};

    private int totalPowerups = 0; // only used in powerwash mode
    private int powerwashGoal; // only used in powerwash mode
    private int raceGoal = 10; // only used in race mode
    public bool gameFinished = false;
    [SerializeField] private float _finishDelay = 3;
    private float _finishTimer = 0;

    // TODO: move to UI controller?
    [SerializeField] private Image healthBarP1;
    [SerializeField] private Image healthBarP2;
    [SerializeField] private TextMeshProUGUI scoreTextP1;
    [SerializeField] private TextMeshProUGUI scoreTextP2;
    [SerializeField] private GameObject endScreen;
    [SerializeField] private TextMeshProUGUI winText;

    private List<DieController> _dieControllers;
    private List<DieState> _dice;

    [SerializeField] private GameObject audioObject;
    private AudioHandler audioHandler;

    public DieState GetDie(int id) {
        return _dice[id].CopyState();
    }

    // public List<DieState> _enemyDice;

    // tileState:
    //  -1 = impassable (i.e., wall)
    //  0 = regular tile
    //  1-6 = powerup (with this label)
    public int[,] tileStates;

    void Awake() {
        if (endScreen != null) {  
            endScreen.SetActive(false);
        }
        LoadLevel();

        _dieControllers = new List<DieController>();
        _dice = new List<DieState>();

        if (Globals.gameType == GameType.Race) {
            UpdateScore(DieController.PTYPE_PLAYER_ONE, 0);
            UpdateScore(DieController.PTYPE_PLAYER_TWO, 0);
        } else if (Globals.gameType == GameType.Powerwash) {
            UpdateScore(DieController.PTYPE_PLAYER_ONE, 0);
            UpdateScore(DieController.PTYPE_PLAYER_TWO, 0);
        }
    }

    void LoadLevel() {
        TextAsset levelData;
        if (Globals.gameType == GameType.Race) {
            levelData = _raceLevelData;
        } else if (Globals.gameType == GameType.Powerwash) {
            levelData = _powerwashLevelData;
        } else {
            levelData = _defaultLevelData;
        }
        string data = levelData.text;
        string[] lines = data.Split('\n');

        string[] dims = lines[0].Split(',');
        gridWidth = int.Parse(dims[0]);
        gridHeight = int.Parse(dims[1]);

        Debug.Log(gridHeight + " " + gridWidth);
        _floorController.InitializeFloor(gridWidth, gridHeight);
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
                    SpawnDie(x, y, DieController.DTYPE_RED_ENEMY);
                } else if (line[x] == '1') {
                    // player 1 spawns at ()
                    SpawnDie(x, y, DieController.DTYPE_PLAYER_ONE);
                } else if (line[x] == '2') {
                    // player 1 spawns at ()

                    // TODO: check whether to spawn AI based on settings
                    // SpawnDie(x, y, DieController.DTYPE_PLAYER_TWO);
                    SpawnDie(x, y, DieController.DTYPE_PLAYER_TWO_AI);
                } else if (line[x] == '.') {
                    // in powerwash mode, spawn "powerups" on all the tiles
                    // (except safe tiles denoted by '-')
                    if (Globals.gameType == GameType.Powerwash) {
                        int randVal = (int)(Random.value * 6) + 1;
                        tileStates[y, x] = randVal;
                        _floorController.InitializePowerup(new Tile(x, y, randVal));
                        totalPowerups += 1;
                    }
                }
            }
        }
        powerwashGoal = (totalPowerups + 1) / 2;

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

    void Start() {
        if (audioObject != null) {
            audioHandler = audioObject.GetComponent<AudioHandler>();
        }
    }

    void Update() {
        if (gameFinished) {
            _finishTimer += Time.deltaTime;
            if (_finishTimer > _finishDelay && !endScreen.activeSelf) {
                endScreen.SetActive(true);
            }
            if (endScreen.activeSelf && Input.GetKey(KeyCode.Escape)) {
                Globals.gameType = GameType.Menu;
                SceneManager.LoadScene("TitleScene");
            }
        }
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

    public bool IsValidState(DieState die) {
        return CanMoveSquare(die.posX, die.posY, die.faces[DieState.D_BOTTOM]);
    }

    public bool IsTargetState(DieState die) {
        return (tileStates[die.posY, die.posX] > 0);
    }

    public bool CanMoveSquare(int x, int y, int bottomFace = 0) {
        if (!isValidSquare(x, y)) return false;

        foreach (DieState die in _dice) {
            if (die.posX == x && die.posY == y) return false;
        }

        if (tileStates[y, x] > 0 && tileStates[y, x] != bottomFace) return false;

        return true;
    } 

    public bool MovePlayerToSquare(int x, int y, int p) {
        if (_dice[p].isDead) return false;
        if (!CanMoveSquare(x, y)) return false;

        _dice[p].SetPosition(x,  y);

        return true;
    }

    public bool PlayerRoll(int dir, int p) {
        if (_dice[p].isDead) return false;
        if (gameFinished) return false;

        int nX = _dice[p].posX + DELTA_X[dir];
        int nY = _dice[p].posY + DELTA_Y[dir];
        int newBottomFace = GetNewBottom(dir, p);

        if (!CanMoveSquare(nX, nY, newBottomFace)) return false;

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

    public bool AnyPowerups() {
        for (int y=0;y<gridHeight;y++) {
            for (int x=0;x<gridWidth;x++) {
                if (tileStates[y,x] > 0) return true;
            }
        }

        return false;
    }

    public void CheckPowerup(int p) {
        if (_dice[p].isDead) return;

        if (_dice[p].GetBottom() == tileStates[_dice[p].posY, _dice[p].posX]) {
            _floorController.RemovePowerup(_dice[p].posX, _dice[p].posY);
            tileStates[_dice[p].posY, _dice[p].posX] = 0;
            _dice[p].IncrementPowerupCount();
            UpdateScore(_dieControllers[p].playerType, _dice[p].powerupsCollected);
            PlaySound("pickup");
            CheckGameFinish();

            if (Globals.gameType != GameType.Powerwash) {
                int puVal = _dice[p].GetBottom();

                if (!_dice[p].powered[puVal]) {
                    _dieControllers[p].ApplyPowerup(puVal);
                    _dice[p].PowerupFace(puVal);
                }
            }
        }
    }

    public void ActivatePowerup(int p) {
        if (_dice[p].isDead) return;
        if (!IsTopActive(p)) return;
        if (gameFinished) return;

        _dieControllers[p].UnapplyPowerup(_dice[p].GetTop());
        _dice[p].PowerdownFace(_dice[p].GetTop());
        _floorController.UpdateTargets();
        PlaySound("usePowerup");

        List<Tile> targets = GetTiles(_dice[p].GetTop(), p);
        AttackTargets(targets);
        _floorController.ExplodeTiles(targets);
    }

    public bool CanHit(int p) {
        List<Tile> targets = GetTiles(_dice[p].GetTop(), p);

        foreach (Tile tile in targets) {
            for(int q = 0; q < _dice.Count; q++) {
                if (_dice[q].isDead) continue;
                bool playerOnTile = _dice[q].posX == tile.x && _dice[q].posY == tile.y;
                bool movingPlayerWasOnTile = _dieControllers[q].IsMoving() && _dice[q].prevPosX == tile.x && _dice[q].prevPosY == tile.y;
                if (playerOnTile || movingPlayerWasOnTile) {
                    return true;
                }
            }
        }

        return false;
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

    public int IsTargetableByAny(int x, int y) {
        for (int p = 0; p < _dice.Count; p++) {
            if (IsTargetableSquare(x, y, p)) return p;
        }
        return -1;
    }

    public bool isPlayerOne(int p) {
        return _dieControllers[p].playerType == DieController.PTYPE_PLAYER_ONE;
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
        if (Globals.gameType == GameType.Battle) {
            _dice[p].GetHit();
            PlaySound("hit");

            AdjustHealthbar(_dieControllers[p].playerType, _dice[p].health);
            // if (_dieControllers[p].playerType == DieController.PTYPE_PLAYER_ONE) {
            //     AdjustHealthbar()
            // }
            // _dieControllers[p].AdjustHealthbar(_dice[p].health);
            // Debug.Log("Die " + p + " hit! Health " + _dice[p].health);

            if (_dice[p].health == 0) {
                KillPlayer(p);
                CheckGameFinish();
            }
        } else if (Globals.gameType == GameType.Race) {
            _dieControllers[p].Stun();
        }
    }

    public void KillPlayer(int p) {
        if (_dice[p].isDead) return;

        _dice[p].isDead = true;
        // _dice[p].posX = -1;
        // _dice[p].posY = -1;
        _dieControllers[p].Die();
    }

    public void CheckGameFinish() {
        if (Globals.gameType == GameType.Battle) {
            int numAlive = 0;
            int indexAlive = 0;
            for (int i=0; i<_dice.Count; i++) {
                if (!_dice[i].isDead) {
                    numAlive += 1;
                    indexAlive = i;
                }
            }
            if (numAlive == 1) {
                FinishGame();
                UpdateWinnerText(_dieControllers[indexAlive].playerType);
            }
        } else if (Globals.gameType == GameType.Powerwash || Globals.gameType == GameType.Race) {
            int winningPlayer = -1;
            int goal = Globals.gameType == GameType.Powerwash ? powerwashGoal : raceGoal;
            for (int i=0; i<_dice.Count; i++) {
                if (_dice[i].powerupsCollected >= goal) {
                    winningPlayer = i;
                }
            }
            if (winningPlayer != -1) {
                for (int i=0; i<_dice.Count; i++) {
                    if (i != winningPlayer) KillPlayer(i);
                }
                FinishGame();
                UpdateWinnerText(_dieControllers[winningPlayer].playerType);
            }
        }
    }

    public void FinishGame() {
        gameFinished = true;
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

    public void UpdateScore(int ptype, int score) {
        int targetScore = 0;
        if (Globals.gameType == GameType.Powerwash) {
            targetScore = powerwashGoal;
        } else if (Globals.gameType == GameType.Race) {
            targetScore = raceGoal;
        } else {
            return; // not a score-based game mode
        }
        if (ptype == DieController.PTYPE_PLAYER_ONE) {
            scoreTextP1.text = score + "/" + targetScore;
        } else if (ptype == DieController.PTYPE_PLAYER_TWO) {
            scoreTextP2.text = score + "/" + targetScore;
        }
    }

    public void UpdateWinnerText(int ptype) {
        if (ptype == DieController.PTYPE_PLAYER_ONE) {
            winText.text = "Player 1 wins!";
        } else if (ptype == DieController.PTYPE_PLAYER_TWO) {
            winText.text = "Player 2 wins!";
        }
    }

    // Get the resulting bottom face of the dice if it were to roll in the given direction.
    public int GetNewBottom(int dir, int p) {
        switch (dir) {
            case DIR_UP:
                return _dice[p].faces[DieState.D_BACK];
            
            case DIR_DOWN:
                return _dice[p].faces[DieState.D_FRONT];
            
            case DIR_LEFT:
                return _dice[p].faces[DieState.D_LEFT];
            
            case DIR_RIGHT:
                return _dice[p].faces[DieState.D_RIGHT];
        }
        return 0;
    }

    public bool IsPowerupRestricted() {
        if (Globals.gameType == GameType.Powerwash) {
            return false;
        } else {
            return true;
        }
    }
    
    public AudioHandler GetAudioHandler() {
        return audioHandler;
    }

    public void PlaySound(string sound) {
        if (Globals.gameType == GameType.Menu) return;
        audioHandler.PlaySound(sound);
    }
}
