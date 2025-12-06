using TMPro;
using UnityEngine;

public class HUDScore : MonoBehaviour
{
    public TMP_Text scoreText;

    void Awake()
    {
        if (!scoreText) scoreText = GetComponent<TMP_Text>();
        Score.OnChanged += UpdateText;
        UpdateText(Score.Value); // inicial
    }

    void OnDestroy() => Score.OnChanged -= UpdateText;

    void UpdateText(int val) => scoreText.text = $"Puntos: {val}";
}
