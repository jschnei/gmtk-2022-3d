using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPlayerController : MonoBehaviour
{
    [SerializeField] private DieController _dieController;
    [SerializeField] private int aiType = 1;

    private StateMachine _aiStateMachine;

    public const int AI_NONE = 0;
    public const int AI_RANDOM = 1;
    public const int AI_RED_ENEMY = 2;

    // Start is called before the first frame update
    void Start()
    {
        
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
