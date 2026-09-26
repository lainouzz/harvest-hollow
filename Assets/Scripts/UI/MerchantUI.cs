using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class MerchantUI : MonoBehaviour
{
    [SerializeField] private GameObject MerchantUIGameobject;

    [SerializeField] private PlayerLook playerLook;

    void Start()
    {
        MerchantUIGameobject.gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other != null)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                playerLook.isInShopUI = true;
                MerchantUIGameobject.gameObject.SetActive(true);
                Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerLook.isInShopUI = false;
            MerchantUIGameobject.gameObject.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
