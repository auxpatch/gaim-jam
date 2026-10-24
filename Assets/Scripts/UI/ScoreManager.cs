using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public PlayerShooter playerShooter;

    public TMP_Text scoreText;
    public TMP_Text highscoreText;

    int score = 0;
    int highscore = 0;

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
            playerShooter.SetUpgradeLevel(upgrades[nextUpgradeIndex].weaponLevel);
            nextUpgradeIndex++;
        }
    }

    public void ResetScore()
    {
        score = 0;
        nextUpgradeIndex=0;
        UpdateUI();
    }

    void UpdateUI()
    {
        scoreText.text = "SCORE: " + score;
        highscoreText.text = "HIGHSCORE: " + highscore;
    }
}