using UnityEngine;

public class PlayerAnimationEvents : MonoBehaviour
{
    private PlayerMovement playerMovement;

    private void Awake()
    {
        playerMovement = GetComponentInParent<PlayerMovement>();
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
