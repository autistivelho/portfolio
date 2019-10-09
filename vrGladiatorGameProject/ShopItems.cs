using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShopItems : MonoBehaviour
{
    public static ShopItems Instance;

    public List<ShopItem> PurchasableItems;

    public List<ShopItem> PurchasedItems;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        PurchasedItems.Clear();

        if (PlayerPrefs.GetString("PurchasedItemsIndexList") != "")
        {
            var itemIndexList = PlayerPrefs.GetString("PurchasedItemsIndexList").Split().ToList();
            if (itemIndexList[0] == "") return;
            foreach (var itemIndex in itemIndexList)
            {
                var item = PurchasableItems.Find(x => x.ItemData.Index == int.Parse(itemIndex));
                PurchasedItems.Add(item);
                ShopItemFloater.Instance.PlaceOnTable(item);
                item.ItemData.Purchased = true;
            }
        }
        else
        {
            PlayerPrefs.SetString("PurchasedItemsIndexList", "");
        }
    }

    public void ResetShop()
    {
        foreach (var item in PurchasedItems)
        {
            ShopItemFloater.Instance.PlaceOnTable(item);
        }
    }

    public void AddPurchasedItem(ShopItem purchasedItem)
    {
        if (PurchasableItems.Contains(purchasedItem))
        {
            PurchasedItems.Add(purchasedItem);
            purchasedItem.ItemData.Purchased = true;
            UpdatePlayerPrefsList();

        }
        else
        {
            Debug.Log("Item isn't in the PurchasableItems list.");
        }
    }

    public void RemovePurchasedItem(ShopItem purchasedItem)
    {
        if (PurchasedItems.Contains(purchasedItem))
        {
            PurchasedItems.Remove(purchasedItem);
            purchasedItem.ItemData.Purchased = false;
            UpdatePlayerPrefsList();
        }
        else
        {
            Debug.Log("PurchasedItems list doesn't contain item to be removed.");
        }
    }

    public void EmptyPurchasedItemsList()
    {
        foreach (var item in PurchasableItems)
        {
            item.ItemData.Purchased = false;
        }
        PurchasedItems.Clear();
        PlayerPrefs.SetString("PurchasedItemsIndexList", "");
    }

    public void PurchaseAllItems()
    {
        PurchasedItems.Clear();
        foreach (var item in PurchasableItems)
        {
            AddPurchasedItem(item);
        }
    }

    private void UpdatePlayerPrefsList()
    {
        var indexString = "";
        foreach (var item in PurchasedItems)
        {
            indexString += indexString.Length > 0 ? $" {item.ItemData.Index}" : $"{item.ItemData.Index}";
        }
        PlayerPrefs.SetString("PurchasedItemsIndexList", indexString);
    }
}
