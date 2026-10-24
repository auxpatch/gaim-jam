using UnityEngine;
using TMPro;
public class Timer : MonoBehaviour
{
   public float time;
   public float timeLimit = 300f;
   public TextMeshProUGUI timerText;

   bool finished;

    void Update()
    {
        if(finished)return;
        time += Time.deltaTime;
        if (time >- timeLimit)
        {
            time = timeLimit;
            UpdateText();
            QuitGame();
            return;
        }
        UpdateText();
    }

    void UpdateText()
    {
        int minutes = Mathf.FloorToInt(time/60f);
        int seconds = Mathf.FloorToInt(time%60f);
        timerText.text = $"{minutes:00}:{seconds:00}";
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
