using UnityEngine;

public class BuyBuilding : MonoBehaviour
{
    public Greenhouse greenhouse;


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !greenhouse.IsOwned)
        {
            greenhouse.TryPurchase(Wallet.Instance.TrySpend);
            greenhouse.OpenDoors();
            Debug.Log("Bought");
  
        }else if(other.CompareTag("Player") && greenhouse.IsOwned)
        {
            greenhouse.OpenDoors();
            Debug.Log("Opening");
        }
    }
}
