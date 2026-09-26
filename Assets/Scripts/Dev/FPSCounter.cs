using UnityEngine;
using UnityEngine.InputSystem;

// Temporary: shows FPS in every scene, including builds. Spawns itself, so there's
// nothing to set up — delete this file to remove it. F3 toggles it.
public class FPSCounter : MonoBehaviour
{
    private const float SampleWindow = 0.5f;

    private bool visible = true;
    private float elapsed;
    private int frames;
    private float worstFrame;

    private float fps;
    private float avgMs;
    private float worstMs;

    private GUIStyle style;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Create()
    {
        GameObject go = new GameObject("FPS Counter");
        go.AddComponent<FPSCounter>();
        DontDestroyOnLoad(go);
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.f3Key.wasPressedThisFrame)
            visible = !visible;

        // Unscaled so the counter keeps working while the game is paused.
        float dt = Time.unscaledDeltaTime;
        elapsed += dt;
        frames++;
        if (dt > worstFrame) worstFrame = dt;

        if (elapsed < SampleWindow) return;

        fps = frames / elapsed;
        avgMs = elapsed / frames * 1000f;
        worstMs = worstFrame * 1000f;

        elapsed = 0f;
        frames = 0;
        worstFrame = 0f;
    }

    private void OnGUI()
    {
        if (!visible) return;

        if (style == null) style = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold };
        style.fontSize = Mathf.Max(14, Screen.height / 40);
        style.normal.textColor = fps >= 60f ? Color.green : fps >= 30f ? Color.yellow : Color.red;

        float width = style.fontSize * 12f;
        float height = style.fontSize * 3f;
        Rect box = new Rect(Screen.width - width - 10f, 10f, width, height);

        GUI.Box(box, GUIContent.none);
        GUI.Label(new Rect(box.x + 8f, box.y + 4f, box.width, box.height),
                  $"{fps:0} FPS\n{avgMs:0.0} ms  (worst {worstMs:0.0})", style);
    }
}
