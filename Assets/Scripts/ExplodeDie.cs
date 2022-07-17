using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceVelocities {
    public float x;
    public float y;
    public float z;

    public float xAngle;
    public float yAngle;
    public float zAngle;

    public void Randomize(float maxSpeed) {
        x = Random.Range(-maxSpeed, maxSpeed);
        y = Random.Range(1, maxSpeed);
        z = Random.Range(-maxSpeed, maxSpeed);

        float maxAngle = maxSpeed * 10;
        xAngle = Random.Range(0, maxAngle);
        yAngle = Random.Range(0, maxAngle);
        zAngle = Random.Range(0, maxAngle);
    }
}

public class ExplodeDie : MonoBehaviour
{
    [SerializeField] private GameObject[] _faces;
    private FaceVelocities[] _fv;

    [SerializeField] private float duration = 5;
    [SerializeField] private float speed = 5;
    private float currTime = 0;

    private bool hasExploded = false;
    private bool doneExploding = false;

    // Start is called before the first frame update
    void Start() {
        _fv = new FaceVelocities[_faces.Length];
        for (int i=0; i<_fv.Length; i++) {
            FaceVelocities fv = new FaceVelocities();
            fv.Randomize(speed);
            _fv[i] = fv;
        }
    }

    // Update is called once per frame
    void Update() {
        if (!hasExploded || doneExploding) return;
        currTime += Time.deltaTime;
        for (int i=0; i<_faces.Length; i++) {
            Vector3 offset = new Vector3(_fv[i].x, _fv[i].y, _fv[i].z);
            _faces[i].transform.Translate(offset * Time.deltaTime);
            // Debug.Log("setting transform to " + offset*Time.deltaTime);
            float xAngleDelta = _fv[i].xAngle * Time.deltaTime;
            float yAngleDelta = _fv[i].yAngle * Time.deltaTime;
            float zAngleDelta = _fv[i].zAngle * Time.deltaTime;
            _faces[i].transform.Rotate(xAngleDelta, yAngleDelta, zAngleDelta);
        }

        if (currTime > duration) {
            foreach (GameObject face in _faces) {
                Destroy(face);
            }
            doneExploding = true;
        }
    }

    public void Explode() {
        hasExploded = true;
    }
}
