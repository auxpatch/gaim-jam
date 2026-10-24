using UnityEngine;

public class GlitchController : MonoBehaviour
{
    public Material mat;
    public float noiseAmount;
    public float glitchStrength;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        mat.SetFloat("_NoiseAmount", noiseAmount);
        mat.SetFloat("_GlitchStrength", glitchStrength);

    }
}
