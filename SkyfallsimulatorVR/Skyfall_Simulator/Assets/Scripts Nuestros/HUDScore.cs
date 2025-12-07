using TMPro;
using UnityEngine;

public class HUDScore : MonoBehaviour
{
    public TMP_Text scoreText;      // "Puntos: 0"
    public TMP_Text nextText;       // "Siguiente: 100"
    public TMP_Text messageText;    // "¡Fallaste! ..." (temporal)
    public float messageTime = 1.2f;

    void Awake()
    {
        if (!scoreText) scoreText = GetComponent<TMP_Text>();
        UpdateScore(Score.Value);
        UpdateNext(RingTrigger.nextRingPoints);

        Score.OnChanged += UpdateScore;
        RingTrigger.OnNextPointsChanged += UpdateNext;
        RingTrigger.OnMissed += ShowMissMessage;
    }

    void OnDestroy()
    {
        Score.OnChanged -= UpdateScore;
        RingTrigger.OnNextPointsChanged -= UpdateNext;
        RingTrigger.OnMissed -= ShowMissMessage;
    }

    void UpdateScore(int val)
    {
        if (scoreText) scoreText.text = $"Puntos: {val}";
    }

    void UpdateNext(int val)
    {
        if (nextText) nextText.text = $"Siguiente: {val}";
    }

    void ShowMissMessage()
    {
        if (!messageText) return;
        StopAllCoroutines();
        StartCoroutine(FlashMessage($"¡Fallaste! "));
    }

    System.Collections.IEnumerator FlashMessage(string msg)
    {
        messageText.gameObject.SetActive(true);
        messageText.text = msg;
        yield return new WaitForSeconds(messageTime);
        messageText.gameObject.SetActive(false);
    }
}
