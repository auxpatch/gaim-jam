using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public PlayerShooter playerShooter;

    public Text scoreText;
    public Text highscoreText;

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
        else Destroy(gameObject);
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
        UpdateUI();
    }

    void UpdateUI()
    {
        scoreText.text = score + " POINTS";
        highscoreText.text = "HIGHSCORE: " + highscore;
    }
}