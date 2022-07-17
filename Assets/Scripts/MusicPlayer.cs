 using UnityEngine;
 using System.Collections;
 using UnityEngine.SceneManagement;
 
 // Do some stuff to make music persist between scenes.
 public class MusicPlayer : MonoBehaviour
 {
     private AudioSource _audioSource;
     private void Awake()
     {
         DontDestroyOnLoad(transform.gameObject);
         _audioSource = GetComponent<AudioSource>();
     }

     public void Start() {
        SceneManager.LoadScene("TitleScene");
     }
 
     public void PlayMusic()
     {
         if (_audioSource.isPlaying) return;
         _audioSource.Play();
     }
 
     public void StopMusic()
     {
         _audioSource.Stop();
     }
 }