using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleMenu : MonoBehaviour {
    
    // Start is called before the first frame update
    void Start()
    {
    }

    public void SinglePlayerButton() {
        Globals.gameType = GameType.SinglePlayer;
        SceneManager.LoadScene("MainScene");
    }

    public void BattleButton() {
        Globals.gameType = GameType.Battle;
        SceneManager.LoadScene("MainScene");
    }

    public void RaceButton() {
        Globals.gameType = GameType.Race;
        SceneManager.LoadScene("MainScene");
    }

    public void PowerwashButton() {
        Globals.gameType = GameType.Powerwash;
        SceneManager.LoadScene("MainScene");
    }
}