using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorController : MonoBehaviour
{
    [SerializeField] private GameController _gameController;
    [SerializeField] private GameObject _powerupPrefab;
    [SerializeField] private Material[] _powerupMaterials;
    [SerializeField] private GameObject _wallPrefab;

    // TODO: do timer correctly
    private int _spawnTimer = 0;
    public const int SPAWN_INTERVAL = 600;

    private GameObject[,] powerups;
    private GameObject[,] walls;
    // Start is called before the first frame update
    void Start()
    {
        powerups = new GameObject[GameController.GRID_SIZE, GameController.GRID_SIZE];
        walls = new GameObject[GameController.GRID_SIZE, GameController.GRID_SIZE];
    }

    // Update is called once per frame
    void Update()
    {
        _spawnTimer++;
        if (_spawnTimer == SPAWN_INTERVAL) {
            _spawnTimer = 0;
            SpawnPowerup();
        }
        UpdateWalls();
    }

    public void SpawnPowerup() {
        Tile tile = _gameController.SpawnPowerup();

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

    // Uses tileStates to add or remove walls as needed.
    private void UpdateWalls() {
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

    public Vector3 GetSquareCenter(int squareX, int squareY) {
        Vector3 floorBackLeft = transform.position + (5.0f) * transform.localScale.x * Vector3.left + (5.0f) * transform.localScale.z * Vector3.forward;
        return floorBackLeft + squareX * Vector3.right + squareY * Vector3.back  + (0.5f) * Vector3.right + (0.5f) * Vector3.back;  
    }
}
