using System;

public class TowerUpgrade
{
    public string Name { get; }
    public int Cost { get; }
    public bool IsPurchased { get; private set; }

    private readonly Action applyAction;

    public TowerUpgrade(string name, int cost, Action applyAction)
    {
        Name = name;
        Cost = cost;
        this.applyAction = applyAction;
    }

    public bool TryPurchase(PlayerEconomyManager economy)
    {
        if (IsPurchased || !economy.SpendMoney(Cost)) return false;
        applyAction();
        IsPurchased = true;
        return true;
    }

    public void ForceApply()
    {
        applyAction();
        IsPurchased = true;
    }
}
