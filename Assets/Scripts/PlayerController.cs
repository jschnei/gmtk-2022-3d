using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float _rollSpeed = 5;
    [SerializeField] private GameController _gameController;
    [SerializeField] private FloorController _floorController;
    [SerializeField] private GameObject _projectile;

    [SerializeField] private Material[] _standardMaterials;
    [SerializeField] private Material[] _powerupMaterials;

    private bool _isMoving;

    // Start is called before the first frame update
    void Start()
    {
        MoveToSquare(1, 1);
    }

    // Update is called once per frame
    void Update()
    {
        if (_isMoving) return;

        // Get movement input
        if (Input.GetKey(KeyCode.A)) {
            if (_gameController.PlayerRoll(GameController.DIR_LEFT)) Assemble(Vector3.left);
        }
        else if (Input.GetKey(KeyCode.D)) {
            if (_gameController.PlayerRoll(GameController.DIR_RIGHT)) Assemble(Vector3.right);
        }
        else if (Input.GetKey(KeyCode.W)) {
            if (_gameController.PlayerRoll(GameController.DIR_UP)) Assemble(Vector3.forward);
        } 
        else if (Input.GetKey(KeyCode.S)) {
            if (_gameController.PlayerRoll(GameController.DIR_DOWN)) Assemble(Vector3.back);
        } 

        if (_isMoving) return;

        // Get projectile input
        if (Input.GetKeyDown(KeyCode.J)) FireProjectile(Vector3.left);
        else if (Input.GetKeyDown(KeyCode.L)) FireProjectile(Vector3.right);
        else if (Input.GetKeyDown(KeyCode.I)) FireProjectile(Vector3.forward);
        else if (Input.GetKeyDown(KeyCode.K)) FireProjectile(Vector3.back);

 
        void Assemble(Vector3 dir) {
            var anchor = transform.position + (Vector3.down + dir) * 0.5f;
            var axis = Vector3.Cross(Vector3.up, dir);
            StartCoroutine(Roll(anchor, axis));
        }
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
        if (_gameController.MovePlayerToSquare(squareX, squareY)) {
            transform.position = _floorController.GetSquareCenter(squareX, squareY) + (0.5f) * Vector3.up;
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
