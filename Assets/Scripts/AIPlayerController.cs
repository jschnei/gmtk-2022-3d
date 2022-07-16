using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPlayerController : MonoBehaviour
{
    [SerializeField] private DieController _dieController;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    int getNextInput() {
        // TODO: something smarter
        return (int)(Random.value * 5);
    }

    // Update is called once per frame
    void Update()
    {
        if (!_dieController.IsMoving()) _dieController.HandleInput(getNextInput());
    }
}
