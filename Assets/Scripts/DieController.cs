using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DieController : MonoBehaviour
{
    [SerializeField] private float _rollSpeed = 5;
    [SerializeField] private GameController _gameController;
    [SerializeField] private FloorController _floorController;
    [SerializeField] private GameObject _projectile;

    [SerializeField] private Material[] _standardMaterials;
    [SerializeField] private Material[] _powerupMaterials;

    private int id;
    private bool _isMoving;

    // Start is called before the first frame update
    void Start()
    {
        id = _gameController.RegisterDie(this);
        MoveToSquare(id, id);
    }

    public const int INPUT_UP = 0;
    public const int INPUT_DOWN = 1;
    public const int INPUT_LEFT = 2;
    public const int INPUT_RIGHT = 3;
    public const int INPUT_ACTIVATE = 4;

    public void HandleInput(int input) {
        if (_isMoving) return;

        switch(input) {
            case INPUT_LEFT:
                if (_gameController.PlayerRoll(GameController.DIR_LEFT, id)) Assemble(Vector3.left);
                break;
            case INPUT_RIGHT:
                if (_gameController.PlayerRoll(GameController.DIR_RIGHT, id)) Assemble(Vector3.right);
                break;
            case INPUT_DOWN:
                if (_gameController.PlayerRoll(GameController.DIR_DOWN, id)) Assemble(Vector3.back);
                break;
            case INPUT_UP:
                if (_gameController.PlayerRoll(GameController.DIR_UP, id)) Assemble(Vector3.forward);
                break;
            case INPUT_ACTIVATE:
                _gameController.ActivatePowerup(id);
                break;
        }

        void Assemble(Vector3 dir) {
            var anchor = transform.position + (Vector3.down + dir) * 0.5f;
            var axis = Vector3.Cross(Vector3.up, dir);
            StartCoroutine(Roll(anchor, axis));
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Get movement input
        // if (id == 0) {
        //     if (Input.GetKey(KeyCode.A)) HandleInput(INPUT_LEFT);
        //     else if (Input.GetKey(KeyCode.D)) HandleInput(INPUT_RIGHT);
        //     else if (Input.GetKey(KeyCode.W)) HandleInput(INPUT_UP);
        //     else if (Input.GetKey(KeyCode.S)) HandleInput(INPUT_DOWN);
        //     else if (Input.GetKey(KeyCode.E)) HandleInput(INPUT_ACTIVATE);
        // } else if (id == 1) {
        //     if (Input.GetKey(KeyCode.LeftArrow)) HandleInput(INPUT_LEFT);
        //     else if (Input.GetKey(KeyCode.RightArrow)) HandleInput(INPUT_RIGHT);
        //     else if (Input.GetKey(KeyCode.UpArrow)) HandleInput(INPUT_UP);
        //     else if (Input.GetKey(KeyCode.DownArrow)) HandleInput(INPUT_DOWN);
        //     else if (Input.GetKey(KeyCode.RightShift)) HandleInput(INPUT_ACTIVATE);
        // }


        // Get projectile input
        //if (Input.GetKeyDown(KeyCode.J)) FireProjectile(Vector3.left);
        //else if (Input.GetKeyDown(KeyCode.L)) FireProjectile(Vector3.right);
        //else if (Input.GetKeyDown(KeyCode.I)) FireProjectile(Vector3.forward);
        //else if (Input.GetKeyDown(KeyCode.K)) FireProjectile(Vector3.back);
    }

    private IEnumerator Roll(Vector3 anchor, Vector3 axis) {
        _isMoving = true;
        for (var i = 0; i < 90 / _rollSpeed; i++) {
            transform.RotateAround(anchor, axis, _rollSpeed);
            yield return new WaitForSeconds(0.01f);
        }
        _isMoving = false;
    }

    private void MoveToSquare(int squareX, int squareY) {
        if (_gameController.MovePlayerToSquare(squareX, squareY, id)) {
            transform.position = _floorController.GetSquareCenter(squareX, squareY) + (0.5f) * Vector3.up;
        } else {
            Debug.Log("Error moving die " + id + " to square " + squareX + ", " + squareY);
        }
    }

    private void FireProjectile(Vector3 dir) {
        GameObject newObject = Instantiate(_projectile, transform.position, Quaternion.identity);
        Projectile newProjectile = newObject.GetComponent<Projectile>();
        Vector3 destination = transform.position + dir * 4; // actual dice number should go here
        newProjectile.SetDestination(destination);
    }

    public void ApplyPowerup(int face) {
        transform.Find("Face" + face).GetComponent<Renderer>().material = _powerupMaterials[face];
    }

    public void UnapplyPowerup(int face) {
        transform.Find("Face" + face).GetComponent<Renderer>().material = _standardMaterials[face];
    }
}
