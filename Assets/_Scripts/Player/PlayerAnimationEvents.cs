using UnityEngine;

public enum AnimationEvent
{
    OnStepForward,
    OnCanAttack,
    OnAttackHitboxActive,
    OnAttackHitboxInActive,
    OnAnimationEnd
}

public class PlayerAnimationEvents : MonoBehaviour
{
    private PlayerMovement playerMovement;

    private void Awake()
    {
        playerMovement = GetComponentInParent<PlayerMovement>();
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
        playerMovement.OnAttackHitboxActive();
    }

    public void OnAttackHitboxInActive()
    {
        playerMovement.OnAttackHitboxInActive();
    }

    public void OnAnimationEnd()
    {
        playerMovement.AnimationEnd();
    }

    public void OnCanAttack()
    {
        playerMovement.CanAttack();
    }

    public void OnStepForward()
    {
        playerMovement.StepForward();
    }
}
