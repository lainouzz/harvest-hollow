using System;
using UnityEngine;

// The single source of truth for the player's money. Everything that earns or
// spends goes through here, and the HUD listens to OnCoinsChanged to display it.
public class Wallet : MonoBehaviour
{
    [SerializeField] private int coins = 100;

    public int Coins => coins;

    // Fired whenever the balance changes (passes the new amount).
    public event Action<int> OnCoinsChanged;

    private void Start()
    {
        // Push the starting amount so any HUD shows it without a first purchase.
        OnCoinsChanged?.Invoke(coins);
    }

    public bool CanAfford(int amount) => amount <= coins;

    public void Add(int amount)
    {
        if (amount <= 0) return;
        coins += amount;
        OnCoinsChanged?.Invoke(coins);
    }

    // Spends only if affordable. Returns false (and changes nothing) otherwise.
    public bool TrySpend(int amount)
    {
        if (amount < 0 || coins < amount) return false;

        coins -= amount;
        OnCoinsChanged?.Invoke(coins);
        return true;
    }
}
