using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class InventoryViewUpdate : MonoBehaviour
{
    public static InventoryViewUpdate Instance; 

    public Transform InventoryContentTransform;
    public GameObject InventorySlotPrefab;
    public PlayerControl ControlScript;
    public CharacterInventory Inventory;


    public List<InventorySlot> AddedSlots = new List<InventorySlot>();
    private List<InventorySlot> previousAddedSlots = new List<InventorySlot>();
    public List<EquipmentSlotScript> EquipmentSlots = new List<EquipmentSlotScript>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else { Debug.Log("Another Instance of InventoryViewUpdate was running");}
    }

    private void Start () {
        if (EquipmentSlots.Count < 1)
        {
            EquipmentSlots = GetComponentsInChildren<EquipmentSlotScript>().ToList();
        }
        ControlScript.OnCharacterChangeCallback += UpdateCharacter;
        AddedSlots = GetComponentsInChildren<InventorySlot>().ToList();

        if (AddedSlots.Count >= 40) return;
	    for(var i = AddedSlots.Count; i < 40; i++)
	    {
	        var addedSlot = Instantiate(InventorySlotPrefab, InventoryContentTransform);
            var addedSlotScript = addedSlot.GetComponent<InventorySlot>();
            addedSlotScript.SlotNumber = i;
            AddedSlots.Add(addedSlotScript);
	    }
	}

	
    private void UpdateInventory()
    {
        var emptySlot = new InventoryItem(null, 0, -1);

        foreach (var slot in EquipmentSlots)
        {
            slot.Item = emptySlot;
            slot.HasItem = false;
        }

        //FATHER FORGIVE ME FOR I HAVE SINNED
         EquipmentSlots[0].Item = new InventoryItem(Inventory.EquippedItems.Head, 1, -1);
         EquipmentSlots[1].Item =  new InventoryItem(Inventory.EquippedItems.Neck, 1, -1);
         EquipmentSlots[2].Item =  new InventoryItem(Inventory.EquippedItems.Torso, 1, -1);
         EquipmentSlots[3].Item =  new InventoryItem(Inventory.EquippedItems.Legs, 1, -1);
         EquipmentSlots[4].Item =  new InventoryItem(Inventory.EquippedItems.Feet, 1, -1);
         EquipmentSlots[5].Item =  new InventoryItem(Inventory.EquippedItems.Cloak, 1, -1);
         EquipmentSlots[6].Item = new InventoryItem(Inventory.EquippedItems.LeftHand, 1, -1); 
         EquipmentSlots[7].Item = new InventoryItem(Inventory.EquippedItems.RightHand, 1, -1);
        var count = 0;
        foreach (var slot in EquipmentSlots)
        {
            if (slot.Item.Item != null)
            {
                Debug.Log(count +" "+ slot.Item.Item);
                Debug.Log(slot.gameObject.name);
                slot.HasItem = true;
            }
            count++;
        }


        //addedSlots = GetComponentsInChildren<InventorySlot>().ToList();
        var items = Inventory.InventoryItems;
        var maxInventorySlotValue = items.Count > 0 ? items.Max(s => s.SlotInInventory) : 0;

        if (maxInventorySlotValue > 32)
        {
            var divideRemainder = maxInventorySlotValue % 8;

            if (AddedSlots.Count < previousAddedSlots.Count)
            {
                for (var i = AddedSlots.Count-1; i >= maxInventorySlotValue + (16 - divideRemainder); i--)
                {
                    Destroy(AddedSlots[i].gameObject);
                }
            }
            else
            {
                for (var i = AddedSlots.Count; i < maxInventorySlotValue + (16 - divideRemainder); i++)
                {
                    var addedSlot = Instantiate(InventorySlotPrefab, InventoryContentTransform);
                    var addedSlotScript = addedSlot.GetComponent<InventorySlot>();
                    addedSlotScript.SlotNumber = i;
                    AddedSlots.Add(addedSlotScript);
                }
            }
        }
        else
        {
            for (var i = AddedSlots.Count - 1; i >= 40; i--)
            {

                Destroy(AddedSlots[i].gameObject);
                AddedSlots.RemoveAt(i);
            }
        }
        
        foreach (var slot in AddedSlots)
        {
            if (slot.Item.Item != null)
            {
                //Debug.Log(slot.Item.Item.name);
            }
            slot.Item = emptySlot;
            slot.HasItem = false;
        }

        foreach (var item in items)
        {
            AddedSlots[item.SlotInInventory].Item = item;
            AddedSlots[item.SlotInInventory].HasItem = true;
        }
        previousAddedSlots = AddedSlots;
        //Debug.Log("Inventory Updated"); 
    }


    private void UpdateCharacter()
    {
        Inventory = ControlScript.ActiveCharacters[0].GetComponent<CharacterInventory>();
        Inventory.OnItemChangeCallback += UpdateInventory;
        UpdateInventory();
    }

}
