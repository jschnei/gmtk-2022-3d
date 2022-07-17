public enum GameType {
    SinglePlayer,
    Battle,
    Race,
    Powerwash,
    Menu
}

public static class Globals {
    public static GameType gameType;
    public static bool playVsHuman = true;
    public static int aiDifficulty = AIPlayerController.DIFFICULTY_EASY;
}