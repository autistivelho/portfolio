using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class GlobalTBModeController : MonoBehaviour
{
    public static GlobalTBModeController Instance;

    public bool IsTurnBased;
    public Canvas CombatCanvas;
    public List<CharacterInformation> CharactersOutOfCombat = new List<CharacterInformation>();
    public List<CharacterInformation> CharactersInCombat = new List<CharacterInformation>();
    public List<CharacterInformation> TurnOrder = new List<CharacterInformation>();
    public CharacterInformation CharacterInTurn;
    public TurnOrderViewController TurnOrderView;

    [SerializeField] private int turnNumber;
    private int roundNumber;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.Log("GlobalTBModeController had another instance running.");
        }
    }

    private void Start()
    {
        CombatCanvas.enabled = false;
    }

    public void TurnBasedMode(bool value)
    {
        if (value == IsTurnBased) return;
        if (IsTurnBased && value == false)
        {
            Debug.Log("Combat ended.");
            CombatCanvas.enabled = false;
        }

        IsTurnBased = value;
        var charactersInScene = FindObjectsOfType<CharacterInformation>().ToList();

        foreach (var character in charactersInScene)
        {
            character.TurnBasedMovement = value;
            if (IsTurnBased)
            {
                //MAKE FILTER LATER.
                character.InCombat = true;
                CharactersInCombat.Add(character);

                //else CharactersOutOfCombat.Add(character);
            }
        }

        if (IsTurnBased && CharactersInCombat.Count > 1)
        {
            Debug.Log("Combat started.");
            CombatCanvas.enabled = true;
            roundNumber = 0;
            TurnOrderView.StartCombat(CharactersInCombat);
            NextRound();
        }

    }

    //THIS STARTS THE NEXT FULL COMBAT ROUND.
    public void NextRound()
    {
        roundNumber++;
        CharactersInCombat = CharactersInCombat.Where(x => !x.Dead).ToList();
        TurnOrder = CharactersInCombat.OrderByDescending(x => x.Abilities.Reaction).ToList();
        turnNumber = 0;
        Debug.Log($"Round {roundNumber}. {TurnOrder.Count} characters in combat.");
        TurnOrderView.NextRound(TurnOrder);
        NextCharacterTurn();
    }

    //THIS STARTS THE TURN OF THE NEXT INDIVIDUAL CHARACTER.
    public void NextCharacterTurn()
    {
        do
        {
            if (turnNumber > CharactersInCombat.Count - 1)
            {
                NextRound();
                return;
            }

            if (turnNumber != 0)
            {
                TurnOrderView.NextTurn();
            }
            CharacterInTurn = TurnOrder[turnNumber];
            turnNumber++;
        } while (CharacterInTurn.Dead);

        CharacterInTurn.GetComponent<CharacterCombatController>().BeginTurn();
    }

    public void EndCurrentTurn()
    {
        CharacterInTurn.GetComponent<CharacterCombatController>().EndTurn();
    }

    public void OnCharacterDeath(CharacterInformation character)
    {
        character.Dead = true;
        TurnOrderView.CharacterDied(character);
        Debug.Log($"{character.Abilities.Name} is now dead. Checking if enemies in combat.");
        if (CheckEnemiesInCombat()) return;
        TurnBasedMode(false);
    }

    public bool CheckEnemiesInCombat()
    {
        var enemies = 0;
        var players = 0;
        foreach (var character in CharactersInCombat)
        {
            if (character.CompareTag("Enemy"))
            {
                if (character.Dead) continue;
                enemies++;
            }
            else if (character.CompareTag("Player"))
            {
                if (character.Dead) continue;
                players++;
            }
        }

        var text = enemies > 0 ? "Still enemies alive in combat. " : "No more enemies alive in combat. ";
        text += players > 0 ? "Still players alive in combat." : "No more players alive in combat.";
        Debug.Log(text);
        return enemies > 0 && players > 0;
    }
}