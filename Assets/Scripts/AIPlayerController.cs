using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPlayerController : MonoBehaviour
{
    [SerializeField] private DieController _dieController;
    [SerializeField] private GameController _gameController;
    [SerializeField] private int aiType = 1;
    [SerializeField] private int _difficulty = 0;

    private float _difficultyWait;
    private float _difficultyAttack;

    public const int DIFFICULTY_EASY = 0;
    public const int DIFFICULTY_MEDIUM = 1;
    public const int DIFFICULTY_HARD = 2;
    public const int DIFFICULTY_BRUTAL = 3;

    public static readonly float[] WAIT_PROBS = {0.05f, 0.1f, 0.4f, 1.0f};
    public static readonly float[] ATTACK_PROBS = {0.2f, 0.3f, 0.6f, 1.0f};


    private StateMachine _aiStateMachine;

    public const int AI_NONE = 0;
    public const int AI_RANDOM = 1;
    public const int AI_RED_ENEMY = 2;
    public const int AI_BFS = 3;

    private List<int> curPath;
    private int curInd = 0;
    private Dictionary<string, DieState> stateLookup;

    public const int MAX_BFS_SEARCH = 5000;

    // Start is called before the first frame update
    void Start()
    {
        curPath = new List<int>();  
        stateLookup = new Dictionary<string, DieState>(); 

        _difficultyWait = WAIT_PROBS[_difficulty];
        _difficultyAttack = ATTACK_PROBS[_difficulty];

        _gameController = _dieController.GetGameController();
    }

    int RandomDirection() {
        return (int)(Random.value * 4);
    }

    List<int> GetNewPath() {
        List<string> bfs = new List<string>();
        Dictionary<string, string> parent = new Dictionary<string, string>();
        Dictionary<string, int> parentEdge = new Dictionary<string, int>();

        DieState startState = _gameController.GetDie(_dieController.id);
        string startString = startState.Serialize();

        if (!stateLookup.ContainsKey(startString)) stateLookup[startString] = startState;
        
        parent[startString] = "";
        parentEdge[startString] = -1;
        bfs.Add(startString);

        int bfsInd = 0;

        while (bfsInd < bfs.Count 
               && bfsInd < MAX_BFS_SEARCH) {
            string curString = bfs[bfsInd];
            bfsInd++;

            DieState curState = stateLookup[curString];
            
            if (_gameController.IsTargetState(curState)) {
                // found good state, return the path to this state

                List<int> path = new List<int>();
                while (curString != startString) {
                    path.Add(parentEdge[curString]);
                    curString = parent[curString];
                }

                path.Reverse();

                return path;
            }

            // try rolling in each of four directions

            DieState[] nxtStates = {curState.CopyState(), curState.CopyState(), curState.CopyState(), curState.CopyState()};

            nxtStates[DieController.INPUT_UP].RollUp();
            nxtStates[DieController.INPUT_DOWN].RollDown();
            nxtStates[DieController.INPUT_LEFT].RollLeft();
            nxtStates[DieController.INPUT_RIGHT].RollRight();

            for (int i=0;i<4;i++) {
                string nxtString = nxtStates[i].Serialize();
                if (_gameController.IsValidState(nxtStates[i]) &&
                        !parent.ContainsKey(nxtString)) {
                    
                    if(!stateLookup.ContainsKey(nxtString)) stateLookup[nxtString] = nxtStates[i];

                    parent[nxtString] = curString;
                    parentEdge[nxtString] = i;

                    bfs.Add(nxtString);
                }
            }
        }

        return new List<int>();
    }

    int NextInputBFS() {
        if (_dieController.IsInactive()) return -1;

        if (_gameController.IsTopActive(_dieController.id)) {
            // attack if would hit
            if (Random.value > _difficultyAttack &&
                    _gameController.CanHit(_dieController.id)) {
                return DieController.INPUT_ACTIVATE;
            }
        }

        if (curInd >= curPath.Count) {
            if (Random.value > _difficultyWait) return RandomDirection();
            if (!_gameController.AnyPowerups()) return RandomDirection();

            curPath = GetNewPath();
            curInd = 0;
        }

        if (curPath.Count == 0) {
            // no path, move randomly;
            return RandomDirection();
        } 

        int move = curPath[curInd];
        curInd++;

        return move;
    }

    int NextInputRedEnemy() {
        if (_aiStateMachine == null) {
            _aiStateMachine = RedEnemySM();
        }

        if (!_aiStateMachine.IsReady()) return -1;
        
        _aiStateMachine.Transition("next");
        return DieController.INPUT_RED_ATTACK_1;
    }

    int NextInputRandom() {
        return (int)(Random.value * 5);
    }

    int getNextInput() {
        switch(aiType) {
            case AI_RANDOM:
                return NextInputRandom();
            case AI_RED_ENEMY:
                return NextInputRedEnemy();
            case AI_BFS:
                return NextInputBFS();
            case AI_NONE:
            default:
                return -1;
        }

        return -1;
    }

    // Update is called once per frame
    void Update()
    {
        if (!_dieController.IsMoving()) _dieController.HandleInput(getNextInput());

        if (_aiStateMachine != null) _aiStateMachine.UpdateTimer(Time.deltaTime);
    }

    static StateMachine RedEnemySM() {
        StateMachine machine = new StateMachine();

        machine.AddState("attack", 3.0f);

        machine.AddTransition("attack", "next", "attack");

        machine.SetState("attack");
        return machine;
    }
}
