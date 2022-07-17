using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DieController : MonoBehaviour
{
    [SerializeField] private float _rollSpeed = 5;
    [SerializeField] private GameController _gameController;
    [SerializeField] private FloorController _floorController;
    [SerializeField] private GameObject _projectile;
    [SerializeField] private GameObject _stunText;

    [SerializeField] private Material[] _standardMaterials;
    [SerializeField] private Material[] _powerupMaterials;

    [SerializeField] public int spawnX;
    [SerializeField] public int spawnY;

    [SerializeField] public int playerType;

    public const int PTYPE_PLAYER_ONE = 1;
    public const int PTYPE_PLAYER_TWO = 2;
    public const int PTYPE_ENEMY = 3;

    public const int DTYPE_PLAYER_ONE = 1;
    public const int DTYPE_PLAYER_TWO = 2;
    public const int DTYPE_PLAYER_TWO_AI = 3;
    public const int DTYPE_RED_ENEMY = 4;

    public int id;
    private bool _isMoving;

    private bool _isStunned = false;
    public const float STUN_DURATION = 5;
    private float _stunTime = 0;

    public void SetupControllers()
    {
        _floorController = GameObject.Find("Floor").GetComponent<FloorController>();
        _gameController = GameObject.Find("GameController").GetComponent<GameController>();
    }

    public GameController GetGameController() {
        return _gameController;
    }

    // Start is called before the first frame update
    void Start()
    {
        id = _gameController.RegisterDie(this);
        if (_stunText != null) {
            _stunText.SetActive(false);
        }
        MoveToSquare(spawnX, spawnY);
    }

    public const int INPUT_UP = 0;
    public const int INPUT_DOWN = 1;
    public const int INPUT_LEFT = 2;
    public const int INPUT_RIGHT = 3;
    public const int INPUT_ACTIVATE = 4;

    public const int INPUT_RED_ATTACK_1 = 10;
    public const int INPUT_RED_ATTACK_2 = 11;
    public const int INPUT_RED_ATTACK_3 = 12;

    public bool IsMoving() {
        return _isMoving;
    }

    public bool IsInactive() {
        return (_isMoving || _isStunned);
    }

    public void HandleInput(int input) {
        if (_isStunned) return;
        if (_isMoving) return;

        if (input == -1) return;
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
            
            default:
                _gameController.EnemyAttack(input, id);
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
        if (_isStunned) {
            _stunTime += Time.deltaTime;
            float stunTimeLeft = STUN_DURATION - _stunTime;
            _stunText.transform.position = transform.position + (2f) * Vector3.up;
            _stunText.transform.rotation = Quaternion.identity;
            _stunText.GetComponent<TextMeshPro>().text = "" + Mathf.Ceil(stunTimeLeft);
            if (_stunTime > STUN_DURATION) {
                Unstun();
            }
            return;
        }
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
        _gameController.FinishRoll(id);
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

    public void Die() {
        MeshRenderer meshRenderer = transform.GetComponent<MeshRenderer>();
        meshRenderer.enabled = false;
        ExplodeDie exploder = transform.GetComponent<ExplodeDie>();
        exploder.Explode();
        // Destroy(this.gameObject);
        // Destroy(this);
    }

    public void Stun() {
        _isStunned = true;
        _stunTime = 0;

        for (int i=1; i<=6; i++) {
            transform.Find("Face" + i).GetComponent<Renderer>().material.SetColor("_Color", new Color(0.4f, 0.4f, 0.4f, 0.9f));
        }
        _stunText.transform.position = transform.position + (2f) * Vector3.up;
        _stunText.transform.rotation = Quaternion.identity;
        _stunText.GetComponent<TextMeshPro>().text = "";
        _stunText.SetActive(true);
    }

    public void Unstun() {
        _isStunned = false;
        for (int i=1; i<=6; i++) {
            transform.Find("Face" + i).GetComponent<Renderer>().material.SetColor("_Color", new Color(1, 1, 1, 0.5f));
        }
        _stunText.SetActive(false);
    }
}
