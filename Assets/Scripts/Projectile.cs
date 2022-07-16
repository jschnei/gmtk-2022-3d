using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 1.0f;

    private Vector3 _destination;

    // Start is called before the first frame update
    void Start() {
    }

    // Update is called once per frame
    void Update() {
        var step = speed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, _destination, step);
    }

    public void SetDestination(Vector3 destination) {
        _destination = destination;
    }
}
