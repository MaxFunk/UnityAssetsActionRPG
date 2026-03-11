using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PanelItemCollectList
{
    private VisualElement container;
    private VisualTreeAsset VTAItemCollectElement;

    private readonly List<ItemCollectData> queuedItems = new();

    public void LinkToUI(VisualElement root, VisualTreeAsset visualTreeAsset)
    {
        container = root.Q<VisualElement>("Container");
        VTAItemCollectElement = visualTreeAsset;
    }

    public void ExternalUpdate()
    {
        if (container.childCount < 3 && queuedItems.Count > 0)
            CreateElement();
    }

    public void QueueNewItem(ItemManager.Item item, int amountRecieved)
    {
        if (item.data == null) return;

        ItemCollectData newItem = new()
        {
            Name = item.data.itemName,
            Icon = item.data.icon,
            Amount = amountRecieved
        };

        queuedItems.Add(newItem);
    }

    public void CreateElement()
    {
        VisualElement newElement = VTAItemCollectElement.CloneTree();
        newElement.dataSource = queuedItems[0];
        queuedItems.RemoveAt(0);
        container.Add(newElement);
        newElement.AddToClassList("item-collect-element");

        newElement.schedule.Execute(() =>
        {
            newElement.style.opacity = 0f;

            newElement.schedule.Execute(() =>
            {
                newElement.RemoveFromHierarchy();
            }).StartingIn(300);

        }).StartingIn(2000);
    }
}

public class ItemCollectData
{
    public string Name = string.Empty;
    public int Amount = 0;
    public Texture2D Icon = null;
}
