using TMPro;
using UnityEngine;

// Shows the game version from Player Settings (e.g. "v1.1.0"), so it never needs updating by hand.
[RequireComponent(typeof(TMP_Text))]
public class VersionLabel : MonoBehaviour
{
    [SerializeField] private string format = "v{0}";

    private void Awake()
    {
        GetComponent<TMP_Text>().text = string.Format(format, Application.version);
    }
}
