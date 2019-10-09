using UnityEngine;

public class BloodSplatter : MonoBehaviour
{
    public static BloodSplatter Instance;

    public GameObject BloodSplatterEffect;

    void Start()
    {
        if(Instance == null)
        {
            Instance = this;
        }
    }

    public void SpillBlood(Vector3 collisionPoint, Transform colliderHit, float hitMagnitude)
    {
        var instance = Instantiate(BloodSplatterEffect, collisionPoint, Quaternion.LookRotation(CalculateRotation(collisionPoint, colliderHit), Vector3.up), colliderHit);
        var effect = instance.GetComponent<ParticleSystem>();
        short minAmount = (short)(hitMagnitude * 50);
        short maxAmount = (short)(hitMagnitude * 500);
        var burst = new ParticleSystem.Burst(0, minAmount, maxAmount, 1, 0.01f);
        effect.emission.SetBurst(0, burst);
        effect.Play();
        Destroy(instance, 1);
    }

    private Vector3 CalculateRotation(Vector3 collisionPoint, Transform colliderHit)
    {
        var spawnRotation = colliderHit.position - collisionPoint;
        return spawnRotation;
    }
}
