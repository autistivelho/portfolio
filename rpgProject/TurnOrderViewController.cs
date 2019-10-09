using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class TurnOrderViewController : MonoBehaviour
{
    public RectTransform RectTransform;
    public GameObject SlotContainer;
    public GameObject SlotPrefab;
    public List<TurnOrderSlotController> Slots;

    private Vector2 movementAmount;

    public void StartCombat(List<CharacterInformation> charactersInCombat)
    {
        for(var i = 0; i < charactersInCombat.Count; i++)
        {
            var character = charactersInCombat[i];
            Slots.Add(Instantiate(SlotPrefab, SlotContainer.transform).GetComponent<TurnOrderSlotController>());
            Slots[i].SetCharacter(character);
            Slots[i].SetIcon(character.CharacterIcon);
        }

        var slotSpace = Slots[0].RectTransform.rect.width * 1.05f;
        var rectWidth = Slots.Count > 10 ? slotSpace * 10f : slotSpace * Slots.Count;
        RectTransform.sizeDelta = new Vector2(rectWidth, slotSpace);

        movementAmount = Vector2.left * slotSpace;
    }

    public void NextRound(List<CharacterInformation> charactersInCombat)
    {
        DeleteDiedCharacters();
        
        Slots = Slots.OrderBy(x => charactersInCombat.IndexOf(x.Character)).ToList();
        for (var i = 0; i < Slots.Count; i++)
        {
            Slots[i].SetOrderNumber(i, Slots);
        }
        Slots[0].MoveToStartPosition();
        Debug.Log($"Slots list has {Slots.Count} slots. Arranged slots, first character is {Slots[0].Character.Abilities.Name}.");
    }

    public void NextTurn()
    {
        Slots[0].MoveToNextPosition(movementAmount);
    }

    public void CharacterDied(CharacterInformation character)
    {
        Slots.Find(x => x.Character == character).SetDead();
    }

    public void DeleteDiedCharacters()
    {
        for (var i = 0; i < Slots.Count; i++)
        {
            if (!Slots[i].Dead) continue;
            Destroy(Slots[i].gameObject);
            Slots.RemoveAt(i);
            i--;
            Debug.Log($"Slot {i} removed from Slots list.");
        }
    }
}
