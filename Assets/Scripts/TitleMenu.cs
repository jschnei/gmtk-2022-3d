using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleMenu : MonoBehaviour {
    [SerializeField] private GameObject _difficultyText;
    [SerializeField] private GameObject _difficultyToggleGroup;
    
    // Start is called before the first frame update
    void Start()
    {
        _difficultyText.SetActive(false);
        _difficultyToggleGroup.SetActive(false);
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

    public void HumanToggle() {
        Globals.playVsHuman = true;
        _difficultyText.SetActive(false);
        _difficultyToggleGroup.SetActive(false);
    }
    public void AIToggle() {
        Globals.playVsHuman = false;
        _difficultyText.SetActive(true);
        _difficultyToggleGroup.SetActive(true);
    }

    public void SetDifficultyEasy() {
        Globals.aiDifficulty = AIPlayerController.DIFFICULTY_EASY;
    }

    public void SetDifficultyMedium() {
        Globals.aiDifficulty = AIPlayerController.DIFFICULTY_MEDIUM;
    }

    public void SetDifficultyHard() {
        Globals.aiDifficulty = AIPlayerController.DIFFICULTY_HARD;
    }

    public void SetDifficultyBrutal() {
        Globals.aiDifficulty = AIPlayerController.DIFFICULTY_BRUTAL;
    }
}