using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioHandler : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlaySound(string soundName) {
        if (soundName == "pickup") {
            transform.Find("Pickup").GetComponent<AudioSource>().Play();
        } else if (soundName == "usePowerup") {
            transform.Find("UsePowerup").GetComponent<AudioSource>().Play();
        } else if (soundName == "hit") {
            transform.Find("Hit").GetComponent<AudioSource>().Play();       
        }
    }
}
