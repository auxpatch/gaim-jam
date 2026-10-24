using UnityEngine;
using TMPro;
public class Timer : MonoBehaviour
{
   public float time;
   public float timeLimit = 300f;
   public TextMeshProUGUI timerText;

   public UnityEvent onTimeUp;
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
        UpdateText():
    }




        timerText.text = Mathf.Floor(time).ToString();
    }
}
