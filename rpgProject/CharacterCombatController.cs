using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class CharacterCombatController : MonoBehaviour
{
    public float MovementLeft;
    public int ActionsLeft;
    public int BonusActionsLeft;
    public int HpLeft;
    public bool IsTargetingSkill;
    public SkillScriptableObject SkillBeingTargeted;
    public bool MyTurn;

    public SkillScriptableObject TestFireball;

    protected CharacterAnimator animator;
    protected TextPopUpController TextPopUpController;



    protected CharacterInformation characterInfo;
    
    protected GameObject cam;
    protected PlayerControl characterControl;
    protected CharacterMovement characterMovement;
    public bool tryingToAttack;
    public bool offHandAttacking;
    protected Transform attackTarget;
    protected InventoryItem scroll; // Scroll To Remove from inventory after casting

    protected void Super()
    {
        animator = GetComponent<CharacterAnimator>();
        characterInfo = GetComponent<CharacterInformation>();
        TextPopUpController = GameObject.Find("PopUpTextSystem").GetComponent<TextPopUpController>();
        cam = GameObject.Find("MainCamera");
        HpLeft = characterInfo.Health;
    }

    protected void SuperBeginTurn()
    {
        MovementLeft = characterInfo.MovementSpeed;
        ActionsLeft = characterInfo.Actions;
        BonusActionsLeft = characterInfo.BonusActions;
        MyTurn = true;
        Debug.Log($"{characterInfo.Abilities.Name}'s turn.");
    }

    public virtual void BeginTurn()
    {

    }

    public void EndTurn()
    {
        GetComponent<CharacterMovement>().Stop();
        Debug.Log($"{characterInfo.Abilities.Name}'s turn ended.");
        MyTurn = false;
        GlobalTBModeController.Instance.NextCharacterTurn();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="target"></param>
    /// <param name="forceOffHand">Allows Player to use Offhand first. Normally attacking will use Primary action/hand first</param>
    public void AttemptToAttack(Transform target, bool forceOffHand = false)
    {
        if(ActionsLeft > 0 || BonusActionsLeft > 0 || !GlobalTBModeController.Instance.IsTurnBased)
        {
            tryingToAttack = true;
            attackTarget = target;
        }
        if (forceOffHand && BonusActionsLeft > 0 || BonusActionsLeft > 0 && ActionsLeft < 1)
        {
            offHandAttacking = true;
            if (GlobalTBModeController.Instance.IsTurnBased) { BonusActionsLeft -= 1; }
            return;
        }
        if (GlobalTBModeController.Instance.IsTurnBased) { ActionsLeft -= 1; }
    }

    public void AttemptToUseSkill(SkillScriptableObject skill, InventoryItem scrollToRemove = null)
    {
        Debug.Log($"Attempting to use SkillInfo {skill.Name}");
        if (ActionsLeft > 0 || !GlobalTBModeController.Instance.IsTurnBased)
        {
            IsTargetingSkill = true;
            SkillBeingTargeted = skill;
            scroll = scrollToRemove;
        }
    }

    public void TriggerSkill(Vector3 target)
    {
        var usedSkill = Instantiate(SkillBeingTargeted.SkillPrefab, Vector3.MoveTowards(transform.position + Vector3.up, target, 0.4f), Quaternion.identity).GetComponent<SkillCastInfo>();
        animator.SpellAnimation(SkillBeingTargeted.Name);

        usedSkill.AnimationLength = SkillBeingTargeted.AnimationLength;
        usedSkill.CasterHand = GetComponent<EquippedItemsManager>().RightHand;
        usedSkill.Caster = transform;
        usedSkill.Target = target;
        if(scroll != null)
        {
            GetComponent<CharacterInventory>().DestroyItem(scroll);
        }
        scroll = null;
        IsTargetingSkill = false;
    }

    public void CancelSkillUse()
    {
        Debug.Log($"Cancelling use of SkillInfo {SkillBeingTargeted.Name}");
        IsTargetingSkill = false;
        SkillBeingTargeted = null;
    }

    public void Damaged(AttackData atkData)
    {
        var damage = Damage.TakeDamage(atkData, characterInfo.DefensiveStats);
        HpLeft = HpLeft - damage.PhysicalDamageTrough - damage.SpecialDamageTrough;
        TextPopUpController.CreateDamageTextPopUp(damage, transform);
        if(HpLeft < 1)
        {
            Death();
        }
    }

    public void Death()
    {
        GlobalTBModeController.Instance.OnCharacterDeath(characterInfo);
        gameObject.layer = 13;
        var movement = characterMovement as PlayerMovement;
        if (movement != null)
        {
            characterControl.ActiveCharacters.Remove(movement);
        }
        animator.Die();
        GetComponent<Collider>().enabled = false;
    }

    public AttackData GetWeaponAttackData(bool primaryHand = true)
    {
        if (primaryHand) return characterInfo.OffensiveStats.GetPrimaryAttack();
        return characterInfo.OffensiveStats.GetSecondaryAttack();
    }

    private void Update()
    {
        if(tryingToAttack && Vector3.Distance(attackTarget.position, transform.position) <= characterInfo.Reach)
        {
            characterMovement.LookAt(attackTarget);
            animator.AttackAnimate();
            Debug.Log("Hit Chance was: " + characterInfo.OffensiveStats.GetHitChancePercentage(attackTarget.GetComponent<CharacterInformation>().DefensiveStats) + "%");
            if (characterInfo.OffensiveStats.Attack(attackTarget.GetComponent<CharacterInformation>().DefensiveStats))
            {
                var atkData = GetWeaponAttackData(!offHandAttacking);
                if (!offHandAttacking)
                {
                    atkData.PhysicalDamage += characterInfo.Abilities.Strength - 10; //this huge calculation should be done somewhere else
                }
                attackTarget.GetComponent<CharacterCombatController>().Damaged(atkData);
            }
            tryingToAttack = false;
            offHandAttacking = false;
            attackTarget = null;
        }

        if (Input.GetKeyUp(KeyCode.F))
        {
            Debug.Log("Button F Pressed.");
            if (IsTargetingSkill)
            {
                CancelSkillUse();
                return;
            }
            if (TestFireball != null)
            {
                AttemptToUseSkill(TestFireball);
            }
        }
    }
}
