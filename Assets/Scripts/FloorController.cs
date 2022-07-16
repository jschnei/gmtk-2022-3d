using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorController : MonoBehaviour
{
    [SerializeField] private GameController _gameController;
    [SerializeField] private GameObject _powerupPrefab;

    // TODO: do timer correctly
    private int _spawnTimer = 0;
    public const int SPAWN_INTERVAL = 600;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        _spawnTimer++;
        if (_spawnTimer == SPAWN_INTERVAL) {
            _spawnTimer = 0;
            SpawnPowerup();
        }
    }

    void SpawnPowerup() {
        Tile tile = _gameController.SpawnPowerup();

        if (tile.x == -1) return;

        GameObject newPowerup = Instantiate(_powerupPrefab, transform.position, Quaternion.identity);
        newPowerup.transform.position = GetSquareCenter(tile.x, tile.y);        
    }

    public Vector3 GetSquareCenter(int squareX, int squareY) {
        Vector3 floorBackLeft = transform.position + (5.0f) * transform.localScale.x * Vector3.left + (5.0f) * transform.localScale.z * Vector3.forward;
        return floorBackLeft + squareX * Vector3.right + squareY * Vector3.back  + (0.5f) * Vector3.right + (0.5f) * Vector3.back;  
    }
}
