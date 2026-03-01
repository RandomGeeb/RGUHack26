using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class KeyPickupPopup : MonoBehaviour
{
    public static KeyPickupPopup Instance { get; private set; }

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private float displayTime = 2f;
    [SerializeField] private float fadeSpeed = 4f;

    private Coroutine _current;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        canvasGroup.alpha = 0f;
    }

    public void Show(string message = "KEY OBTAINED")
    {
        if (messageText != null) messageText.text = message;
        if (_current != null) StopCoroutine(_current);
        _current = StartCoroutine(PopupRoutine());
    }

    private IEnumerator PopupRoutine()
    {
        // Fade in
        while (canvasGroup.alpha < 1f)
        {
            canvasGroup.alpha += Time.deltaTime * fadeSpeed;
            yield return null;
        }
        canvasGroup.alpha = 1f;

        yield return new WaitForSeconds(displayTime);

        // Fade out
        while (canvasGroup.alpha > 0f)
        {
            canvasGroup.alpha -= Time.deltaTime * fadeSpeed;
            yield return null;
        }
        canvasGroup.alpha = 0f;
    }
}
