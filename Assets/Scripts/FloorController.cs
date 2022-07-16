using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FloorController : MonoBehaviour
{
    [SerializeField] private GameController _gameController;
    [SerializeField] private GameObject _powerupPrefab;
    [SerializeField] private Material[] _powerupMaterials;

    Timer _spawnTimer;
    public const int SPAWN_INTERVAL = 5;

    private GameObject[,] powerups;

    // Start is called before the first frame update
    void Start()
    {
        powerups = new GameObject[GameController.GRID_SIZE, GameController.GRID_SIZE];
        _spawnTimer = new Timer(SPAWN_INTERVAL);
    }

    // Update is called once per frame
    void Update()
    {
        _spawnTimer.UpdateTimer(Time.deltaTime);
        if (_spawnTimer.IsOver()) {
            SpawnPowerup();
            _spawnTimer = new Timer(SPAWN_INTERVAL);
        }
    }

    void SpawnPowerup() {
        Tile tile = _gameController.SpawnPowerup();

        if (tile.value == -1) return;

        GameObject newPowerup = Instantiate(_powerupPrefab, transform.position, Quaternion.identity);
        newPowerup.transform.parent = transform;
        newPowerup.transform.position = GetSquareCenter(tile.x, tile.y);
        newPowerup.GetComponentInChildren<Renderer>().material = _powerupMaterials[tile.value];
    }

    public Vector3 GetSquareCenter(int squareX, int squareY) {
        Vector3 floorBackLeft = transform.position + (5.0f) * transform.localScale.x * Vector3.left + (5.0f) * transform.localScale.z * Vector3.forward;
        return floorBackLeft + squareX * Vector3.right + squareY * Vector3.back  + (0.5f) * Vector3.right + (0.5f) * Vector3.back;  
    }
}
