using System;
using UnityEngine;

public class Wallet : MonoBehaviour
{
    [SerializeField] private int coins = 100;

    public int Coins => coins;
    public event Action<int> OnCoinsChanged;

    private void Start()
    {
        OnCoinsChanged?.Invoke(coins);
    }

    public bool CanAfford(int amount) => amount <= coins;

    public void SetCoins(int amount)
    {
        coins = Mathf.Max(0, amount);
        OnCoinsChanged?.Invoke(coins);
    }

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
