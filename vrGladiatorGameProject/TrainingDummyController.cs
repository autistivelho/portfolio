using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainingDummyController : MonoBehaviour
{
    public static TrainingDummyController Instance;

    public List<TrainingDummy> TrainingDummies;

    public int TrainingLevel;
    public bool TrainingActive;

    public int CurrentCorrectHits;
    public int CurrentFalseHits;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(this);
        }
    }

    [ContextMenu("StartTraining")]
    public void TestStartTraining()
    {
        StartTraining();
    }

    public void StartTraining(int? trainingLevel = null)
    {
        if (trainingLevel != null)
        {
            TrainingLevel = (int) trainingLevel;
        }

        if (!(TrainingDummies.Count > 0))
        {
            Debug.Log("There is no training dummies assigned to TrainingDummies list.");
            return;
        }

        foreach (var dummy in TrainingDummies)
        {
            if (dummy.StartPosition == Vector3.zero || dummy.AttackPosition == Vector3.zero)
            {
                Debug.Log($"{dummy.gameObject.name} doesn't have one or both positions set.");
                return;
            }

            if (!dummy.ReadyToAttack)
            {
                Debug.Log($"{dummy.gameObject.name} is not ready. Training not started.");
                return;
            }
        }

        TrainingActive = true;
        StartCoroutine(Training());
    }

    public void StopTraining()
    {
        foreach (var dummy in TrainingDummies)
        {
            dummy.Attacking = false;
        }
        TrainingActive = false;
        CurrentCorrectHits = 0;
        CurrentFalseHits = 0;
    }

    private IEnumerator Training()
    {
        var correctHitAmount = TrainingLevel * 10;
        var falseHitAmount = Mathf.Ceil(20f / TrainingLevel);
        var speed = Mathf.Ceil(TrainingLevel / 2f);
        var attackLength = (11f - TrainingLevel) / 3f;
        var attackInterval = 5.5f - TrainingLevel / 2f;
        var timer = Time.time;

        foreach (var dummy in TrainingDummies)
        {
            dummy.AttackLength = attackLength;
            dummy.Speed = speed;
        }

        TrainingDummies[Random.Range(0, TrainingDummies.Count)].Attacking = true;

        while (CurrentCorrectHits < correctHitAmount)
        {
            if (Time.time >= timer + attackInterval)
            {
                var readyDummies = TrainingDummies.FindAll(x => x.ReadyToAttack);

                if (readyDummies.Count > 0)
                {
                    var dummy = readyDummies[Random.Range(0, readyDummies.Count)];
                    dummy.RandomColor();
                    dummy.Attacking = true;
                }

                timer = Time.time;
            }

            if (CurrentFalseHits >= falseHitAmount) goto Lose;

            yield return new WaitForFixedUpdate();
        }
        Debug.Log("You is winner.");
        PlayerMoney.Instance.AddMoney(TrainingLevel * 10);
        TrainingLevel++;
        goto Finish;

        Lose:
            Debug.Log("You lost.");

        Finish:
            StopTraining();
    }
}
