using UnityEngine;

public class GlitchController : MonoBehaviour
{
    [SerializeField] Material mat;
    [Range(0f, 1f)] public float noiseAmount = 0.5f;
    [Range(0f, 1f)] public float baseStrength = 0f;   // constant low-level glitch
    [SerializeField] float decaySpeed = 3f;

    static readonly int NoiseId = Shader.PropertyToID("_NoiseAmount");
    static readonly int StrengthId = Shader.PropertyToID("_GlitchStrength");

    float burst; // temporary extra strength

    // Call this from gameplay: glitch.Trigger(1f);
    public void Trigger(float intensity = 1f)
    {
        burst = Mathf.Max(burst, intensity);
    }

    void Update()
    {
        if (mat == null) return;

        burst = Mathf.MoveTowards(burst, 0f, decaySpeed * Time.deltaTime);

        mat.SetFloat(NoiseId, noiseAmount);
        mat.SetFloat(StrengthId, Mathf.Clamp01(baseStrength + burst));
    }

    void OnDisable()
    {
        if (mat != null) mat.SetFloat(StrengthId, 0f); // reset the asset
    }
}