using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private FloorController _floorController;

    // Start is called before the first frame update
    void Start()
    {
        GameObject floor = GameObject.Find("Floor");
        _floorController = floor.GetComponent<FloorController>();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void MoveToSquare(int squareX, int squareY) {
        transform.position = _floorController.GetSquareCenter(squareX, squareY) + (0.5f) * Vector3.up;
    }
}
