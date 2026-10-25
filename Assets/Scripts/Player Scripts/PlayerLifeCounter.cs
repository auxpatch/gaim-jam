using TMPro;
using UnityEngine;

public class PlayerLifeCounter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI counter;
    private PlayerHealth pHealth;

    private void Start()
    {
        pHealth = GameObject.FindAnyObjectByType<PlayerHealth>(FindObjectsInactive.Include);
    }

    private void Update()
    {
        if(pHealth == null) GameObject.FindAnyObjectByType<PlayerHealth>(FindObjectsInactive.Include);
        counter.text = pHealth.GetCurrentLives().ToString();
    }
}
