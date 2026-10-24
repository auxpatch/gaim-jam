using UnityEngine;

//Code just identifies a cube as a player
public class TEMP_player : MonoBehaviour
{

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("TEMP PLAYER - SOMETHING HIT ME!!!");
    }

}
