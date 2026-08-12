using UnityEngine;

[CreateAssetMenu(fileName = "WallBounceEffectConfig", menuName = "GameFeel/WallBounceEffectConfig")]
public class WallBounceEffectConfigSO : ScriptableObject
{
    [Header("Bounce Scakle Effect Settings")]
    public Vector3 bounceForce = Vector3.one;
    public float bounceDuration = 0.5f;
    [Range(0f, 180f)]
    public float randomness = 90f;
    public int bounceVibrato = 10;
    public bool fadeOut = true;

    [Header("Bounce Position Effect Settings")]
    public Vector3 positionForce = Vector3.one;
    public float positionDuration = 0.5f;
    [Range(0f, 180f)]
    public float positionRandomness = 90f;
    public int positionVibrato = 10;    
    public bool positionFadeOut = true; 
}
