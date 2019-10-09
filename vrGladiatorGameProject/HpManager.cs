using UnityEngine;

public static class HpManager
{
    public static float CalculateHpAmount(float currentHp, float change, SpecialDamage specialDamage = SpecialDamage.None, SpecialDamage weakness = SpecialDamage.None)
    {
        var damageMultiplier = (Mathf.Sign(change) == -1 && specialDamage == weakness && specialDamage != SpecialDamage.None) ? 4f : 1f;

        return currentHp + change * damageMultiplier;
    }
}
