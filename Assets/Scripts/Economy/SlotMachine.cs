using System.Collections;
using TMPro;
using UnityEngine;


public class SlotMachine : MonoBehaviour
{
    [Header("Rules")]
    [SerializeField] private int bet = 100;
    [SerializeField] private int pullsPerDay = 5;
    [Tooltip("Chance weight per symbol, in reel order: carrot, potato, tomato, grape, strawberry, golden egg.")]
    [SerializeField] private int[] weights = { 30, 30, 20, 12, 6, 2 };
    [Tooltip("Payout for three of a kind, per symbol (same order).")]
    [SerializeField] private int[] tripleWins = { 300, 300, 500, 1000, 2500, 10000 };
    [SerializeField] private int pairWin = 130;
    [SerializeField] private string[] symbolNames = { "carrots", "potatoes", "tomatoes", "grapes", "strawberries", "golden eggs" };

    [Header("Reels")]
    [SerializeField] private Transform[] reels = new Transform[3];
    [Tooltip("If the wrong symbol lines up in the window, change this in steps of 60.")]
    [SerializeField] private float angleOffset;
    [Tooltip("Tick if the reels spin the wrong way or land on the wrong symbols.")]
    [SerializeField] private bool invertSpin;
    [SerializeField] private float spinTime = 1.2f;
    [Tooltip("Extra time before each next reel stops.")]
    [SerializeField] private float stagger = 0.45f;
    [SerializeField] private int extraTurns = 3;

    [Header("Lever")]
    [SerializeField] private Transform lever;
    [SerializeField] private float leverAngle = 50f;

    [Header("Prompt")]
    [SerializeField] private GameObject prompt;
    [SerializeField] private TMP_Text label;
    [SerializeField] private string pullFormat = "Pull the lever · {0}g ({1} left today)";
    [SerializeField] private string poorFormat = "Need {0}g · you have {1}g";
    [SerializeField] private string closedText = "Out of order · come back tomorrow";
    [SerializeField] private float resultShowTime = 2.5f;
    [SerializeField] private Color normalColor = new Color(1f, 0.945f, 0.816f);
    [SerializeField] private Color poorColor = new Color(0.95f, 0.64f, 0.54f);
    [SerializeField] private Color winColor = new Color(1f, 0.85f, 0.3f);

    [Header("Sound (optional)")]
    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip leverClip;
    [SerializeField] private AudioClip reelStopClip;
    [SerializeField] private AudioClip winClip;
    [SerializeField] private AudioClip jackpotClip;
    [SerializeField] private AudioClip loseClip;

    public int PullsToday { get; set; }   // saved

    private GameInput input;
    private Wallet wallet;
    private PlayerLook playerLook;
    private bool playerInZone;
    private bool spinning;
    private bool promptShown = true;

    private readonly float[] angles = new float[3];
    private Quaternion[] reelBase;
    private Quaternion leverBase;

    private string resultText;
    private Color resultColor;
    private float resultUntil;

    private void Awake()
    {
        input = new GameInput();
        wallet = FindAnyObjectByType<Wallet>();
        playerLook = FindAnyObjectByType<PlayerLook>();

        reelBase = new Quaternion[reels.Length];
        for (int i = 0; i < reels.Length; i++)
            if (reels[i] != null) reelBase[i] = reels[i].localRotation;
        if (lever != null) leverBase = lever.localRotation;

        SetPrompt(false);
    }

    private void OnEnable()
    {
        input.Enable();
        DayManager.OnDayChanged += ResetPulls;
    }

    private void OnDisable()
    {
        input.Disable();
        DayManager.OnDayChanged -= ResetPulls;
    }

    private void ResetPulls(int day) => PullsToday = 0;

    private void Update()
    {
        bool blocked = Time.timeScale == 0f || (playerLook != null && playerLook.isInShopUI);
        bool show = playerInZone && !blocked && wallet != null;

        SetPrompt(show);
        if (!show) return;

        if (spinning)
        {
            SetLabel("...", normalColor);
            return;
        }

        if (Time.time < resultUntil)
        {
            SetLabel(resultText, resultColor);
            return;
        }

        int left = pullsPerDay - PullsToday;
        if (left <= 0)
        {
            SetLabel(closedText, poorColor);
            return;
        }

        bool canAfford = wallet.CanAfford(bet);
        SetLabel(canAfford ? string.Format(pullFormat, bet, left) : string.Format(poorFormat, bet, wallet.Coins),
                 canAfford ? normalColor : poorColor);

        if (canAfford && input.Player.Interact.WasPressedThisFrame() && wallet.TrySpend(bet))
            StartCoroutine(Pull());
    }

    private IEnumerator Pull()
    {
        spinning = true;
        PullsToday++;
        Play(leverClip);
        StartCoroutine(PullLever());

        int[] result = { Roll(), Roll(), Roll() };
        Debug.Log($"Slot machine: {symbolNames[result[0]]} | {symbolNames[result[1]]} | {symbolNames[result[2]]}");

        int running = reels.Length;
        for (int i = 0; i < reels.Length; i++)
            StartCoroutine(SpinReel(i, result[i], spinTime + stagger * i, () => running--));
        while (running > 0) yield return null;

        int win = Payout(result, out string message);
        if (win > 0) wallet.Add(win);

        bool jackpot = result[0] == result[1] && result[1] == result[2] && result[0] == weights.Length - 1;
        Play(win == 0 ? loseClip : jackpot ? jackpotClip : winClip);

        resultText = message;
        resultColor = win > 0 ? winColor : normalColor;
        resultUntil = Time.time + resultShowTime;
        spinning = false;

        // Saving now means quitting to the menu can't undo this pull.
        if (SaveManager.Instance != null) SaveManager.Instance.Save();
    }

    private int Payout(int[] r, out string message)
    {
        if (r[0] == r[1] && r[1] == r[2])
        {
            int win = tripleWins[r[0]];
            message = r[0] == weights.Length - 1 ? $"JACKPOT!!! +{win}g" : $"Three {symbolNames[r[0]]}! +{win}g";
            return win;
        }

        if (r[0] == r[1] || r[1] == r[2] || r[0] == r[2])
        {
            message = $"A pair! +{pairWin}g";
            return pairWin;
        }

        message = "Nothing... the turnip wins.";
        return 0;
    }

    private int Roll()
    {
        int total = 0;
        foreach (int w in weights) total += w;

        int pick = Random.Range(0, total);
        for (int i = 0; i < weights.Length; i++)
        {
            if (pick < weights[i]) return i;
            pick -= weights[i];
        }
        return 0;
    }

    // Eases out so the reel slows down before landing, with a few full turns first.
    private IEnumerator SpinReel(int i, int symbol, float duration, System.Action done)
    {
        float start = angles[i];
        float delta = Mathf.Repeat(symbol * 60f - start, 360f) + 360f * extraTurns;

        Play(reelStopClip);

        for (float t = 0f; t < 1f; t += Time.deltaTime / duration)
        {
            SetReel(i, start + delta * (1f - Mathf.Pow(1f - t, 3f)));
            yield return null;
            
        }

        SetReel(i, Mathf.Repeat(start + delta, 360f));
        done();
    }

    private void SetReel(int i, float angle)
    {
        angles[i] = angle;
        if (reels[i] == null) return;
        float a = -angle + angleOffset;
        reels[i].localRotation = reelBase[i] * Quaternion.AngleAxis(invertSpin ? -a : a, Vector3.right);
    }

    private IEnumerator PullLever()
    {
        if (lever == null) yield break;

        Quaternion down = leverBase * Quaternion.AngleAxis(leverAngle, Vector3.right);
        for (float t = 0f; t < 1f; t += Time.deltaTime / 0.15f)
        {
            lever.localRotation = Quaternion.Slerp(leverBase, down, t);
            yield return null;
        }
        for (float t = 0f; t < 1f; t += Time.deltaTime / 0.4f)
        {
            lever.localRotation = Quaternion.Slerp(down, leverBase, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }
        lever.localRotation = leverBase;
    }

    private void Play(AudioClip clip)
    {
        if (source != null && clip != null) source.PlayOneShot(clip);
    }

    private void SetLabel(string text, Color color)
    {
        if (label == null) return;
        label.text = text;
        label.color = color;
    }

    private void SetPrompt(bool show)
    {
        if (show == promptShown) return;
        promptShown = show;
        if (prompt != null) prompt.SetActive(show);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) playerInZone = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) playerInZone = false;
    }
}
