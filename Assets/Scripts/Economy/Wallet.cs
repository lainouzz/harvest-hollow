using System;
using UnityEngine;

// Single source of truth for the player's money.
public class Wallet : MonoBehaviour
{
    [SerializeField] private int coins = 100;

    public int Coins => coins;
    public event Action<int> OnCoinsChanged;

    private void Start()
    {
        // Push the starting amount so HUDs show it before any transaction.
        OnCoinsChanged?.Invoke(coins);
    }

    public bool CanAfford(int amount) => amount <= coins;

    public void Add(int amount)
    {
        if (amount <= 0) return;
        coins += amount;
        OnCoinsChanged?.Invoke(coins);
    }

    public bool TrySpend(int amount)
    {
        if (amount < 0 || coins < amount) return false;

        coins -= amount;
        OnCoinsChanged?.Invoke(coins);
        return true;
    }
}
