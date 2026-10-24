using UnityEngine;
using TMPro;
public class Timer : MonoBehaviour
{
   public float time;
   public float startTime = 300f;
   public TextMeshProUGUI timerText;

   bool finished;

   void Start()
   {
       time = startTime;
       UpdateText();
   }

    void Update()
    {
        if(finished)return;
        time -= Time.deltaTime;
        if (time <= 0f)
        {
            time = 0f;
            UpdateText();
            QuitGame();
            return;
        }
        UpdateText();
    }

    void UpdateText()
    {
        //int totalSeconds = Math.CeilToInt(time);
        int minutes = Mathf.FloorToInt(time/60f);
        int seconds = Mathf.FloorToInt(time%60f);
        timerText.text = $"{minutes}:{seconds:00}";
    }

    void QuitGame()
    {
        finished=true;

        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #else
                Application.Quit(); //close the entire .exe file
        #endif
    }

}
