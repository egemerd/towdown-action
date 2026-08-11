using UnityEngine;

/// <summary>
/// Dash preset'i. Curve YOK — matematiksel SmoothStep ease kullanılır, 
/// bu sayede curve editor tuning'ine gerek kalmadan garantili smooth 
/// başlangıç/bitiş elde ederiz (velocity iki uçta da matematiksel olarak sıfır).
/// </summary>
[CreateAssetMenu(fileName = "DashConfig", menuName = "Player/Dash Config", order = 10)]
public class PlayerDashConfigSO : ScriptableObject
{
    [Header("— Distance —")]
    [Tooltip("Dash sırasında katedilecek maksimum mesafe (unit). " +
             "Duvara denk gelirse erken sonlanır. Tipik: 4-8.")]
    [Range(1f, 20f)]
    public float distance = 6f;

    [Header("— Timing —")]
    [Tooltip("Dash süresi (saniye) — İSTENEN minimum süre. " +
             "maxSpeed cap'i aşılırsa süre otomatik uzatılır (bkz. maxSpeed).")]
    [Range(0.05f, 1f)]
    public float duration = 0.22f;

    [Tooltip("Dash sırasında ulaşılabilecek maksimum hız (unit/saniye). " +
             "distance/duration oranı bu değeri aşarsa, duration otomatik uzatılır — " +
             "yani karakter asla bu hızdan daha 'hızlı fırlamış' hissi vermez. " +
             "0 = sınırsız (sadece distance/duration'a göre hesaplanır).")]
    [Range(0f, 60f)]
    public float maxSpeed = 28f;

    [Header("— Ease Shape —")]
    [Tooltip("Bitiş yavaşlamasının yumuşaklığı. Başlangıç HER ZAMAN anlık/güçlüdür " +
         "(input response için) — bu parametre sadece dash'in NASIL durduğunu kontrol eder. " +
         "1 = tamamen linear (sabit hız, sonda snap — SNAP OLUR, önerilmez). " +
         "2 = orta glide (önerilen default). " +
         "3-4 = daha uzun/yumuşak iniş (ağır, kayıcı his).")]
    [Range(1.2f, 4f)]
    public float easePower = 2.2f;

    [Tooltip("Dash sırasında invulnerability süresi (saniye).")]
    [Range(0f, 1.5f)]
    public float iframeDuration = 0.16f;

    [Tooltip("Dash bittikten sonra ne kadar süre yeni dash atılamaz.")]
    [Range(0f, 5f)]
    public float cooldown = 0.4f;

    [Header("— Handoff —")]
    [Tooltip("Dash bittiğinde PlayerMovement'a devredilecek hız (unit/saniye). " +
         "0 = tam durur (SNAP olur, önerilmez). " +
         "moveSpeed'e yakın değerler doğal geçiş verir. " +
         "moveSpeed'in altında: karakter dash sonrası biraz yavaşlar sonra tekrar hızlanır. " +
         "moveSpeed'e eşit: dash biter bitmez normal koşuya devam eder.")]
    [Range(0f, 20f)]
    public float endSpeed = 6f;

    [Header("— Visuals —")]
    public GameObject trailPrefab;
    [Range(0.1f, 5f)]
    public float trailLifetime = 0.8f;
    public Vector3 trailSpawnOffset = Vector3.zero;

    [Header("— Squash —")]
    public EnemySquashConfigSO squashProfile;

    [Header("— Wall Detection —")]
    public LayerMask wallLayer;
    [Range(0.1f, 1f)]
    public float wallCastRadius = 0.4f;

    /// <summary>
    /// maxSpeed cap'i dikkate alarak gerçek kullanılacak süreyi hesaplar.
    /// distance/duration oranı maxSpeed'i aşıyorsa duration'ı uzatır.
    /// </summary>
    public float GetEffectiveDuration()
    {
        if (maxSpeed <= 0f) return duration;

        // Yeni formül: velocity-integrated dash için initial speed
        // Ortalama hız (distance/duration) ile easePower'a göre initial speed hesaplanır
        // Power ease-out'ta ortalama hız = (initialSpeed + endSpeed*easePower) / (easePower+1)
        // Basitleştirme: initialSpeed ≈ (distance/duration) * easePower - endSpeed*(easePower-1)
        float avgSpeed = distance / duration;
        float impliedInitialSpeed = avgSpeed * easePower - endSpeed * (easePower - 1f);

        if (impliedInitialSpeed <= maxSpeed) return duration;

        float scaleFactor = impliedInitialSpeed / maxSpeed;
        return duration * scaleFactor;
    }

    /// <summary>
    /// Config'in matematiksel olarak implike ettiği başlangıç hızını döndürür.
    /// PlayerDash bunu integrate ederek pozisyonu hesaplar.
    /// </summary>
    public float GetInitialSpeed(float effectiveDuration)
    {
        float avgSpeed = distance / effectiveDuration;
        return avgSpeed * easePower - endSpeed * (easePower - 1f);
    }
}