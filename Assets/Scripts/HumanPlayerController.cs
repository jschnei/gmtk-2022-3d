using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanPlayerController : MonoBehaviour
{

    [SerializeField] private KeyCode _leftInput;
    [SerializeField] private KeyCode _rightInput;
    [SerializeField] private KeyCode _upInput;
    [SerializeField] private KeyCode _downInput;
    [SerializeField] private KeyCode _activateInput;

    [SerializeField] private DieController _dieController;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(_leftInput)) _dieController.HandleInput(DieController.INPUT_LEFT);
        else if (Input.GetKey(_rightInput)) _dieController.HandleInput(DieController.INPUT_RIGHT);
        else if (Input.GetKey(_upInput)) _dieController.HandleInput(DieController.INPUT_UP);
        else if (Input.GetKey(_downInput)) _dieController.HandleInput(DieController.INPUT_DOWN);
        else if (Input.GetKey(_activateInput)) _dieController.HandleInput(DieController.INPUT_ACTIVATE);
    }
}
