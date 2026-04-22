using System;
using UnityEngine.UIElements;

public class UpgradeSlot
{
    private readonly Label label;
    private readonly VisualElement button;

    private Func<string> getName;
    private Func<int> getCost;
    private Func<bool> isBought;
    private Func<bool> tryBuy;

    public UpgradeSlot(VisualElement root, string labelName, string buttonName)
    {
        label = root.Q<Label>(labelName);
        button = root.Q<VisualElement>(buttonName);
    }

    public UpgradeSlot WithName(Func<string> getName)
    {
        this.getName = getName;
        return this;
    }

    public UpgradeSlot WithCost(Func<int> getCost)
    {
        this.getCost = getCost;
        return this;
    }

    public UpgradeSlot WithBoughtState(Func<bool> isBought)
    {
        this.isBought = isBought;
        return this;
    }

    public UpgradeSlot OnPurchase(Func<bool> tryBuy)
    {
        this.tryBuy = tryBuy;
        return this;
    }

    public void Bind()
    {
        Refresh();
        if (!isBought())
        {
            button.RegisterCallback<ClickEvent>(HandleClick);
        }
    }

    private void HandleClick(ClickEvent _)
    {
        if (tryBuy())
        {
            Refresh();
        }
    }

    private void Refresh()
    {
        label.text = isBought()
            ? $"{getName()} - Bought"
            : $"{getName().ToUpper()} - {getCost()}";
    }
}
