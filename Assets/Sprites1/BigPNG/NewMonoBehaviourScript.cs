using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class NewMonoBehaviourScript : MonoBehaviour
{
    public RawImage img;
    public float x;
    public float y;

    // Update is called once per frame
    void Update()
    {
        img.uvRect = new Rect(img.uvRect.position + new Vector2(x, y) * Time.deltaTime, img.uvRect.size);
    }
}
