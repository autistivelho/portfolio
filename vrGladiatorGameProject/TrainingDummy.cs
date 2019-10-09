using UnityEngine;
using Valve.VR.InteractionSystem;

public class TrainingDummy : MonoBehaviour
{
    public Vector3 StartPosition;
    public Vector3 AttackPosition;

    public bool ReadyToAttack;
    public bool Attacking;
    public float AttackLength;
    public float Speed;
    public TrainingWeaponEnum DummyColor;

    public Renderer Rend;
    public Material RedDummyMaterial;
    public Material BlueDummyMaterial;

    public AudioSource DummySound;

    private float attackTimer;

    private void Start()
    {
        Speed = Speed > 0 ? Speed : 1f;

        if (Rend == null) Rend = GetComponent<Renderer>();

        Rend.material = new Material(Shader.Find("Standard"));

        RandomColor();
    }

    private void Update()
    {
        if (!Attacking)
        {
            if (transform.position == StartPosition)
            {
                if (!ReadyToAttack) ReadyToAttack = true;
                return;
            }

            transform.position = Vector3.MoveTowards(transform.position, StartPosition, Speed * Time.deltaTime);
        }
        else
        {
            if (transform.position == AttackPosition)
            {
                if (attackTimer >= AttackLength)
                {
                    TrainingDummyController.Instance.CurrentFalseHits++;
                    ToggleAttack();
                    attackTimer = 0;
                }
                attackTimer += Time.deltaTime;
                return;
            }

            if (ReadyToAttack) ReadyToAttack = false;

            transform.position = Vector3.MoveTowards(transform.position, AttackPosition, Speed * Time.deltaTime);
        }
    }

    public void ToggleAttack()
    {
        Attacking = !Attacking;
    }

    public void SetSpeed(float speed)
    {
        Speed = speed;
    }

    public void Hit(TrainingWeaponEnum weaponColor)
    {
        if(!Attacking) return;

        if (weaponColor == DummyColor)
        {
            TrainingDummyController.Instance.CurrentCorrectHits++;
        }
        else
        {
            TrainingDummyController.Instance.CurrentFalseHits++;
        }
        Attacking = false;
    }

    public void SetColor(Color color)
    {
        Rend.material.color = color;
    }

    public void RedColor()
    {
        SetColor(Color.red);
        DummyColor = TrainingWeaponEnum.Red;
    }

    public void BlueColor()
    {
        SetColor(Color.blue);
        DummyColor = TrainingWeaponEnum.Blue;
    }

    public void RandomColor()
    {
        var rendIndex = Random.Range(0, 2);
        DummyColor = rendIndex > 0 ? TrainingWeaponEnum.Red : TrainingWeaponEnum.Blue;
        Rend.material = rendIndex > 0 ? RedDummyMaterial : BlueDummyMaterial;
    }

    [ContextMenu("Apply start position")]
    public void ApplyStartPoint()
    {
        StartPosition = transform.position;
    }

    [ContextMenu("Apply attack position")]
    public void ApplyAttackPosition()
    {
        AttackPosition = transform.position;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.relativeVelocity.magnitude > 2f)
        {
            var baton = other.gameObject.GetComponent<Baton>();
            if (baton != null)
            {
                var hand = baton.GetComponent<Interactable>().attachedToHand.gameObject.name;
                Debug.Log(hand);
                ControllerRumble.Instance.Rumble(0.05f,1f, hand);
                Hit(baton.WeaponColor);
                var audioClip = AudioClips.Instance.DummyHitSounds[Random.Range(0, AudioClips.Instance.DummyHitSounds.Count)];
                DummySound.PlayOneShot(audioClip);
            }
        }
    }
}