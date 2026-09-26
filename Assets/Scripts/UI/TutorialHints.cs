using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

// A short run of hint popups at the start of a new game. Skipped when continuing a
// save. Each hint fades in, stays for its duration (or until Enter), then fades out.
public class TutorialHints : MonoBehaviour
{
    [System.Serializable]
    public class Hint
    {
        [TextArea(2, 4)] public string message;
        public float duration = 6f;
    }

    [SerializeField] private CanvasGroup panel;
    [SerializeField] private TMP_Text label;
    [SerializeField] private Key nextKey = Key.Enter;
    [SerializeField] private float startDelay = 1.5f;
    [SerializeField] private float fadeTime = 0.4f;
    [SerializeField] private float gapBetweenHints = 0.4f;

    [SerializeField] private List<Hint> hints = new()
    {
        new Hint { message = "Welcome to Harvest Hollow!\nWASD to move, mouse to look, Space to jump.\n(Enter: next tip)" },
        new Hint { message = "Press 1-5 to switch what you're holding, and left-click to use it." },
        new Hint { message = "With the hoe, click an empty plot to lay soil, then click it again to till it." },
        new Hint { message = "Water tilled soil before planting. Seeds in wet soil grow faster and give more." },
        new Hint { message = "Press Tab to open your inventory and click a seed to pick it, then plant it with the seed bag." },
        new Hint { message = "Harvest ripe crops with the harvest tool. Walk up to the merchant's truck to sell them and buy seeds." },
        new Hint { message = "Look at a plot with a price over it and press B to buy it." },
        new Hint { message = "Press X on a fence to open a path through it." },
        new Hint { message = "Walk into the sleep spot to end the day. Crops grow overnight and your game is saved." },
    };

    private void Start()
    {
        if (panel != null)
        {
            panel.alpha = 0f;
            panel.blocksRaycasts = false;
        }

        // A save file means the player has already played this farm.
        if (SaveSystem.HasSave || panel == null || label == null || hints.Count == 0) return;

        StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        yield return new WaitForSeconds(startDelay);

        foreach (Hint hint in hints)
        {
            label.text = hint.message;
            yield return Fade(0f, 1f);

            float t = 0f;
            while (t < hint.duration && !NextPressed())
            {
                t += Time.deltaTime;
                yield return null;
            }

            yield return Fade(1f, 0f);
            yield return new WaitForSeconds(gapBetweenHints);
        }
    }

    private bool NextPressed() =>
        Keyboard.current != null && Keyboard.current[nextKey].wasPressedThisFrame;

    private IEnumerator Fade(float from, float to)
    {
        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            panel.alpha = Mathf.Lerp(from, to, t / fadeTime);
            yield return null;
        }
        panel.alpha = to;
    }
}
