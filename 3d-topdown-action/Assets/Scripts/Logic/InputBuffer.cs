using UnityEngine;

/// <summary>
/// Belirli bir input'un son ne zaman verildiðini takip eder.
/// Sistem input'u anýnda tüketemediðinde (attack recovery'de vs.) input kaybolmasýn
/// diye kýsa bir "hafýza" penceresi tutar.
///
/// Kullaným:
///   1. Buffer.RegisterInput() — oyuncu input verdiðinde çaðýr
///   2. Buffer.HasBufferedInput — sonra kontrol et
///   3. Buffer.ConsumeInput() — input'u iþlediðinde tüket, tekrar tetiklenmesin
/// </summary>
[System.Serializable]
public class InputBuffer
{
    [Tooltip("Input'un buffer'da ne kadar süre kalacaðý (saniye)")]
    [SerializeField] private float bufferDuration = 0.15f;

    private float bufferedUntil = -1f;

    /// <summary>
    /// Þu an buffer'da geçerli bir input var mý?
    /// Time.time buffer'ýn expire süresinden küçükse evet.
    /// </summary>
    public bool HasBufferedInput => Time.time <= bufferedUntil;

    /// <summary>
    /// Kaç saniye önce input verildi. Debug/log için kullanýþlý.
    /// </summary>
    public float TimeSinceInput => bufferedUntil - Time.time - bufferDuration + (Time.time);

    /// <summary>
    /// Yeni bir input'u buffer'a kaydet. Süre baþlar.
    /// </summary>
    public void RegisterInput()
    {
        bufferedUntil = Time.time + bufferDuration;
    }

    /// <summary>
    /// Buffered input'u tüket. Ayný input tekrar tekrar tetiklenmesin diye.
    /// </summary>
    public void ConsumeInput()
    {
        bufferedUntil = -1f;
    }

    /// <summary>
    /// Buffer'ý temizle (ölüm, cutscene vs.)
    /// </summary>
    public void Clear()
    {
        bufferedUntil = -1f;
    }
}