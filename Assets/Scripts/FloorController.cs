using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FloorController : MonoBehaviour
{
    [SerializeField] private GameController _gameController;
    [SerializeField] private GameObject _powerupPrefab;
    [SerializeField] private Material[] _powerupMaterials;
    [SerializeField] private GameObject _wallPrefab;
    [SerializeField] private GameObject _targetPrefab;
    [SerializeField] private GameObject _targetPrefab2;
    [SerializeField] private GameObject _explosionPrefab;
    
    [SerializeField] private GameObject[] _diePrefabs;

    Timer _spawnTimer;
    public const int SPAWN_INTERVAL = 5;

    private GameObject[,] powerups;
    private GameObject[,] walls;
    private GameObject[,] targetIndicators;
    private GameObject[,] enemies;

    private bool isActive = false;

    void Awake() {
        // powerups = new GameObject[GameController.GRID_SIZE, GameController.GRID_SIZE];
        // walls = new GameObject[GameController.GRID_SIZE, GameController.GRID_SIZE];
        // targetIndicators = new GameObject[GameController.GRID_SIZE, GameController.GRID_SIZE];
        // enemies = new GameObject[GameController.GRID_SIZE, GameController.GRID_SIZE];
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    public void InitializeFloor(int gridWidth, int gridHeight) {
        transform.localScale = new Vector3(gridWidth / 10f, 1.0f, gridHeight / 10f);
        GetComponent<Renderer>().material.mainTextureScale = new Vector2(gridWidth/2, gridHeight/2);

        powerups = new GameObject[gridHeight, gridWidth];
        walls = new GameObject[gridHeight, gridWidth];
        targetIndicators = new GameObject[gridHeight, gridWidth];
        enemies = new GameObject[gridHeight, gridWidth];

        _spawnTimer = new Timer(SPAWN_INTERVAL);

        isActive = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isActive) return;

        if (Globals.gameType != GameType.Powerwash) {
            _spawnTimer.UpdateTimer(Time.deltaTime);
            if (_spawnTimer.IsOver()) {
                SpawnPowerup();
                _spawnTimer = new Timer(SPAWN_INTERVAL);
            }
        }

        UpdateWalls();
    }

    public void SpawnPowerup() {
        InitializePowerup(_gameController.SpawnPowerup());
    }

    public void InitializePowerup(Tile tile) {
        if (tile.value == -1) return;

        GameObject newPowerup = Instantiate(_powerupPrefab, transform.position, Quaternion.identity);
        newPowerup.transform.parent = transform;
        newPowerup.transform.position = GetSquareCenter(tile.x, tile.y);
        newPowerup.GetComponentInChildren<Renderer>().material = _powerupMaterials[tile.value];
        powerups[tile.y, tile.x] = newPowerup;
    }

    public void RemovePowerup(int squareX, int squareY) {
        if (powerups[squareY, squareX] != null) Destroy(powerups[squareY, squareX]);
    }

    public void SpawnDie(int x, int y, int type) {
        GameObject newDie = Instantiate(_diePrefabs[type], transform.position, Quaternion.identity);
        DieController controller = newDie.GetComponent<DieController>();
        
        controller.spawnX = x;
        controller.spawnY = y;

        controller.SetupControllers();

        // newEnemy.transform.position = GetSquareCenter(x, y);
        // enemies[y, x] = newEnemy;
    }

    public void RemoveEnemy(int x, int y) {
        if (enemies[y, x] != null) Destroy(enemies[y, x]);
    }

    // Uses tileStates to add or remove walls as needed.
    public void UpdateWalls() {
        int[,] tileStates = _gameController.tileStates;
        for (int i=0; i<tileStates.GetLength(0); i++) {
            for (int j=0; j<tileStates.GetLength(1); j++) {
                if (tileStates[i,j] == -1 && walls[i,j] == null) {
                    GameObject newWall = Instantiate(_wallPrefab, transform.position, Quaternion.identity);
                    newWall.transform.position = GetSquareCenter(j, i);
                    walls[i,j] = newWall;
                } else if (tileStates[i,j] != -1 && walls[i,j] != null) {
                    Destroy(walls[i,j]);
                    walls[i,j] = null;
                }
            }
        }
    }

    public void UpdateTargets() {
        if (!isActive) return;

        for (int i=0; i<targetIndicators.GetLength(0); i++) {
            for (int j=0; j<targetIndicators.GetLength(1); j++) {
                int p = _gameController.IsTargetableByAny(i, j);
                if (targetIndicators[i,j] == null && p != -1) {
                    GameObject newTarget;
                    if(_gameController.isPlayerOne(p)) {
                        newTarget = Instantiate(_targetPrefab, transform.position, Quaternion.identity);
                    } else {
                        newTarget = Instantiate(_targetPrefab2, transform.position, Quaternion.identity);
                    }
                    newTarget.transform.position = GetSquareCenter(i, j);
                    targetIndicators[i,j] = newTarget;
                }
                if (targetIndicators[i,j] != null && p == -1) {
                    Destroy(targetIndicators[i,j]);
                    targetIndicators[i,j] = null;
                }
            }
        }
    }

    public void ExplodeTiles(List<Tile> tiles) {
        foreach (Tile tile in tiles) {
            GameObject newExplosion = Instantiate(_explosionPrefab, transform.position, Quaternion.identity);
            newExplosion.transform.position = GetSquareCenter(tile.x, tile.y);
        }
    }

    public Vector3 GetSquareCenter(int squareX, int squareY) {
        Vector3 floorBackLeft = transform.position + (5.0f) * transform.localScale.x * Vector3.left + (5.0f) * transform.localScale.z * Vector3.forward;
        return floorBackLeft + squareX * Vector3.right + squareY * Vector3.back  + (0.5f) * Vector3.right + (0.5f) * Vector3.back;  
    }
}
