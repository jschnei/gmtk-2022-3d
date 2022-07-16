using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DieState {
    const bool VERBOSE = true;

    public int posX = 0;
    public int posY = 0;

    // faces stored as [top, bottom, front, back, left, right]
    const int D_TOP = 0;
    const int D_BOTTOM = 1;
    const int D_FRONT = 2;
    const int D_BACK = 3;
    const int D_LEFT = 4;
    const int D_RIGHT = 5;
    public int[] faces = {3, 4, 6, 1, 2, 5};

    public void LogState() {
        if(VERBOSE) Debug.Log("x=" + posX + ", y=" + posY + ", top=" + faces[D_TOP]);
    }

    public void SetPosition(int x, int y) {
        posX = x;
        posY = y;

        LogState();
    }

    public void RollUp() {
        int tmp_top = faces[D_TOP];
        faces[D_TOP] = faces[D_FRONT];
        faces[D_FRONT] = faces[D_BOTTOM];
        faces[D_BOTTOM] = faces[D_BACK];
        faces[D_BACK] = tmp_top;

        posY--;

        LogState();
    }

    public void RollDown() {
        int tmp_top = faces[D_TOP];
        faces[D_TOP] = faces[D_BACK];
        faces[D_BACK] = faces[D_BOTTOM];
        faces[D_BOTTOM] = faces[D_FRONT];
        faces[D_FRONT] = tmp_top;

        posY++;

        LogState();
    }

    public void RollLeft() {
        int tmp_top = faces[D_TOP];
        faces[D_TOP] = faces[D_RIGHT];
        faces[D_RIGHT] = faces[D_BOTTOM];
        faces[D_BOTTOM] = faces[D_LEFT];
        faces[D_LEFT] = tmp_top;

        posX--;

        LogState();
    }

    public void RollRight() {
        int tmp_top = faces[D_TOP];
        faces[D_TOP] = faces[D_LEFT];
        faces[D_LEFT] = faces[D_BOTTOM];
        faces[D_BOTTOM] = faces[D_RIGHT];
        faces[D_RIGHT] = tmp_top;

        posX++;

        LogState();
    }
}

// GameController should manage all discrete game logic
public class GameController : MonoBehaviour
{
    public const int GRID_SIZE = 20;

    public const int DIR_UP = 0;
    public const int DIR_DOWN = 1;
    public const int DIR_LEFT = 2;
    public const int DIR_RIGHT = 3;

    public static readonly int[] DELTA_X = {0, 0, -1, 1};
    public static readonly int[] DELTA_Y = {-1, 1, 0, 0};

    private DieState _playerDie;
    // Start is called before the first frame update
    void Start()
    {
        _playerDie = new DieState();
    }

    public bool isValidSquare(int x, int y) {
        return (x >= 0  && y >= 0 && x < GRID_SIZE && y < GRID_SIZE);
    }

    public bool MovePlayerToSquare(int x, int y) {
        // todo: check if square is valid
        if (!isValidSquare(x, y)) return false;

        _playerDie.SetPosition(x,  y);

        return true;
    }

    public bool PlayerRoll(int dir) {
        int nX = _playerDie.posX + DELTA_X[dir];
        int nY = _playerDie.posY + DELTA_Y[dir];

        if (!isValidSquare(nX, nY)) return false;

        switch (dir) {
            case DIR_UP:
                _playerDie.RollUp();
                break;
            
            case DIR_DOWN:
                _playerDie.RollDown();
                break;
            
            case DIR_LEFT:
                _playerDie.RollLeft();
                break;
            
            case DIR_RIGHT:
                _playerDie.RollRight();
                break;
        }

        return true;
    }
}
