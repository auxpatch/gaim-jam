using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public PlayerShooter playerShooter;

    public TMP_Text scoreText;
    public TMP_Text highscoreText;
    public TMP_Text levelText;

    int score = 0;
    int highscore = 0;
    int currentLevel= 1;

    public int CurrentLevel => currentLevel;


    [System.Serializable]
    public struct UpgradeThreshold
    {
        public int requiredScore;
        public int weaponLevel;
    }

    public UpgradeThreshold[] upgrades;
    private int nextUpgradeIndex = 0;

    const string HighscoreKey = "Highscore";

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);
    }

    void Start()
    {
        highscore = PlayerPrefs.GetInt(HighscoreKey, 0);
        UpdateUI();
    }

    public void AddScore(int points)
    {
        score += points;
        CheckForUpgrades();

        if (score > highscore)
        {
            highscore = score;
            PlayerPrefs.SetInt(HighscoreKey, highscore);
            PlayerPrefs.Save();
        }

        UpdateUI();
    }

    void CheckForUpgrades()
    {
        while(nextUpgradeIndex < upgrades.Length && score >= upgrades[nextUpgradeIndex].requiredScore)
        {
            currentLevel = upgrades[nextUpgradeIndex].weaponLevel;
            if (playerShooter != null) playerShooter.SetUpgradeLevel(currentLevel);
            playerShooter.SetUpgradeLevel(upgrades[nextUpgradeIndex].weaponLevel);
            nextUpgradeIndex++;
        }
    }

    public void ResetScore()
    {
        score = 0;
        nextUpgradeIndex=0;
        currentLevel = 1;
        if (playerShooter !=null) playerShooter.SetUpgradeLevel(currentLevel);
        UpdateUI();
    }

    void UpdateUI()
    {
        scoreText.text = "SCORE: " + score;
        highscoreText.text = "HIGHSCORE: " + highscore;
        if (levelText !=null) levelText.text="LEVEL: " + currentLevel;
    }
}