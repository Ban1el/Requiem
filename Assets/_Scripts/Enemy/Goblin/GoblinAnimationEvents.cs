using UnityEngine;

public class GoblinAnimationEvents : MonoBehaviour
{
    private EnemyMovement enemyMovement;

    private void Awake()
    {
        enemyMovement = GetComponentInParent<EnemyMovement>();
    }

    private void OnAnimationEvent(string eventName)
    {
        string[] events = eventName.Split('|');

        foreach (string e in events)
        {
            HandleEvent(e.Trim());
        }
    }

    private void HandleEvent(string eventName)
    {
        if (!System.Enum.TryParse(eventName, out AnimationEvent animEvent))
        {
            Debug.LogWarning($"Unknown animation event: {eventName}");
            return;
        }

        switch (animEvent)
        {
            case AnimationEvent.OnStepForward:
                OnStepForward();
                break;
            case AnimationEvent.OnCanAttack:
                OnCanAttack();
                break;
            case AnimationEvent.OnAttackHitboxActive:
                OnAttackHitboxActive();
                break;
            case AnimationEvent.OnAttackHitboxInActive:
                OnAttackHitboxInActive();
                break;
            case AnimationEvent.OnAnimationEnd:
                OnAnimationEnd();
                break;
        }
    }

    public void OnAttackHitboxActive()
    {
        enemyMovement.OnAttackHitboxActive();
    }

    public void OnAttackHitboxInActive()
    {
        enemyMovement.OnAttackHitboxInActive();
    }

    public void OnAnimationEnd()
    {
        enemyMovement.AnimationEnd();
    }

    public void OnCanAttack()
    {
        enemyMovement.CanAttack();
    }

    public void OnStepForward()
    {
        enemyMovement.StepForward();
    }
}
