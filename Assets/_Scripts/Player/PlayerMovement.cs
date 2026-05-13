using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private PlayerData playerData;
    public Animator animator;
    private bool stateComplete;
    private Rigidbody2D rb;
    public InputSystem_Actions actions;
    private float xInput;
    private float yInput;
    private bool is_facing_right = true;

    [SerializeField]
    private Transform ground_check;
    private float default_gravity_scale;
    private bool is_jumping = false;
    private bool is_dodge_rolling = false;
    private float coyote_time_remaining;
    private bool can_coyote_jump = false;
    private bool hang_time_active = false;
    private bool canMove = true;
    private bool isAnimating = false;
    private float stepValue = 0f;

    //Attack variables
    private bool isGroundAttacking = false;
    private bool canAttack = true;
    private bool hitboxActive = false;
    [SerializeField]
    private BoxCollider2D hitbox;

    //Debugging
    [SerializeField]
    private TextMeshProUGUI stateIndicator;


    enum PlayerState
    {
        Idle,
        Running,
        Airborne,
        Falling,
        GroundAttack,
        DodgeRoll
    }

    enum GroundAttackState
    {
        attack_1,
        attack_2,
        attack_3
    }
    PlayerState state;
    GroundAttackState groundAttackState;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        actions = new InputSystem_Actions();
    }

    private void Start()
    {
        coyote_time_remaining = playerData.coyote_time;
        default_gravity_scale = rb.gravityScale;
    }

    private void FixedUpdate()
    {
        HandleXMovement();
    }

    private void Update()
    {
        Falling();
        CoyoteTimer();
        FlipPlayer();

        if (stateComplete)
        {
            SelectState();
        }

        UpdateState();

        stateIndicator.text = $"State: {state}";
    }

    private void UpdateState()
    {
        switch (state)
        {
            case PlayerState.Idle:
                UpdateIdle();
                break;
            case PlayerState.Running:
                UpdateRun();
                break;
            case PlayerState.Airborne:
                UpdateAirborne();
                break;
            case PlayerState.GroundAttack:
                UpdateGroundAttack();
                break;
            case PlayerState.DodgeRoll:
                UpdateDodgeRoll();
                break;
        }
    }

    private void SelectState()
    {
        if (IsGrounded())
        {
            if (xInput == 0)
            {
                state = PlayerState.Idle;
                StartIdle();
            }
            else
            {
                state = PlayerState.Running;
                StartRunning();
            }
        }
        else
        {
            state = PlayerState.Airborne;
            StartAirborne();
        }
    }

    private void StartIdle()
    {
        stateComplete = false;
        animator.speed = 1f;
        animator.Play("Idle");
        canMove = true;
        canAttack = true;
    }

    private void StartRunning()
    {
        stateComplete = false;
        animator.speed = 1f;
        animator.Play("Run");
        canMove = true;
        canAttack = true;
    }

    private void StartDodgeRoll()
    {
        stateComplete = false;
        animator.speed = 1f;
        canMove = false;
        canAttack = false;
        state = PlayerState.DodgeRoll;
        is_dodge_rolling = true;
        StopVelocity();
        stepValue = playerData.dodge_roll_step_value;
        isAnimating = true;
        animator.Play("Dodge");
    }

    private void StartAirborne()
    {
        animator.Play("Jump");
    }

    private void StartGroundAttack1()
    {
        stateComplete = false;
        animator.speed = 1f;
        StopVelocity();
        stepValue = playerData.attack_1_step_value;
        groundAttackState = GroundAttackState.attack_1;
        canMove = false;
        canAttack = false;
        isAnimating = true;
        animator.Play("Attack-1");
    }

    private void StartGroundAttack2()
    {
        StopVelocity();
        animator.speed = 1f;
        stepValue = playerData.attack_2_step_value;
        groundAttackState = GroundAttackState.attack_2;
        canMove = false;
        canAttack = false;
        isAnimating = true;
        animator.Play("Attack-2");
    }

    private void StartGroundAttack3()
    {
        StopVelocity();
        animator.speed = 1f;
        stepValue = playerData.attack_3_step_value;
        groundAttackState = GroundAttackState.attack_3;
        canMove = false;
        canAttack = false;
        isAnimating = true;
        animator.Play("Attack-3");
    }

    private void UpdateIdle()
    {
        if (!IsGrounded() || xInput != 0)
        {
            stateComplete = true;
        }
    }

    private void UpdateRun()
    {
        // float velX = rb.linearVelocity.x;
        // animator.speed = Mathf.Abs(velX) / playerData.movement_speed;

        if (xInput == 0 || !IsGrounded())
        {
            stateComplete = true;
        }
    }

    private void UpdateAirborne()
    {
        if (IsGrounded())
        {
            stateComplete = true;
        }
    }

    private void UpdateGroundAttack()
    {
        if (!isAnimating)
        {
            isGroundAttacking = false;
        }

        if (!isGroundAttacking)
        {
            stateComplete = true;
        }
    }

    private void UpdateDodgeRoll()
    {
        Debug.Log("Roll state");
        if (!isAnimating)
        {
            is_dodge_rolling = false;
        }

        if (!is_dodge_rolling)
        {
            stateComplete = true;
        }
    }

    private void OnEnable()
    {
        actions.Player.Enable();
        actions.Player.Move.performed += CheckInput;
        actions.Player.Jump.performed += Jump;
        actions.Player.Move.canceled += CheckInput;
        actions.Player.Jump.canceled += Jump;
        actions.Player.Attack.performed += Attack;
        actions.Player.Sprint.performed += DodgeRoll;
    }

    private void OnDisable()
    {
        actions.Player.Disable();
        actions.Player.Move.performed -= CheckInput;
        actions.Player.Jump.performed -= Jump;
        actions.Player.Move.canceled -= CheckInput;
        actions.Player.Jump.canceled -= Jump;
        actions.Player.Attack.performed -= Attack;
        actions.Player.Sprint.performed -= DodgeRoll;
    }

    public void StepForward()
    {
        float direction = is_facing_right ? 1f : -1f;
        rb.linearVelocity = new Vector2(stepValue * direction, rb.linearVelocity.y);
    }

    private void StopVelocity()
    {
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        rb.angularVelocity = 0f;
    }

    private void Attack(InputAction.CallbackContext ctx)
    {
        if (!canAttack) return;

        if (!isGroundAttacking)
        {
            stateComplete = false;
            isGroundAttacking = true;
            state = PlayerState.GroundAttack;
            StartGroundAttack1();
        }
        else if (isGroundAttacking)
        {
            stateComplete = false;
            SelectNextGroundAttack();
        }
    }

    private void SelectNextGroundAttack()
    {
        switch (groundAttackState)
        {
            case GroundAttackState.attack_1:
                StartGroundAttack2();
                break;

            case GroundAttackState.attack_2:
                StartGroundAttack3();
                break;
        }
    }

    private void DodgeRoll(InputAction.CallbackContext ctx)
    {
        if (isGroundAttacking) return;

        if (!is_dodge_rolling && canMove)
            StartDodgeRoll();
    }

    private void CheckInput(InputAction.CallbackContext ctx)
    {
        xInput = ctx.ReadValue<Vector2>().x;
        yInput = ctx.ReadValue<Vector2>().y;
    }

    private void Jump(InputAction.CallbackContext ctx)
    {
        if (!canMove) return;

        if (
              ctx.performed && (IsGrounded() || can_coyote_jump)
           )
        {
            can_coyote_jump = false;
            rb.gravityScale = default_gravity_scale;
            is_jumping = true;
            rb.AddForce(Vector2.up * playerData.jump_force, ForceMode2D.Impulse);
        }

        //Variable height jump
        //To stop the player from jumping early
        if (ctx.canceled && is_jumping)
        {
            is_jumping = false;
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                rb.linearVelocity.y / playerData.release_jump_vel_modifier
            );
        }
    }

    public void CanAttack()
    {
        canAttack = true;
    }

    public void AnimationEnd()
    {
        isAnimating = false;
    }

    private void HandleXMovement()
    {
        if (!canMove) return;

        rb.linearVelocity = new Vector2(xInput * playerData.movement_speed, rb.linearVelocity.y);

        //Flip the player
        if (xInput > 0.1f)
        {
            is_facing_right = true;
        }

        if (xInput < -0.1f)
        {
            is_facing_right = false;
        }
    }

    private void FlipPlayer()
    {
        if (is_facing_right)
        {
            transform.localScale = new Vector3(1f, 1f, 1f);
        }
        else
        {
            transform.localScale = new Vector3(-1f, 1f, 1f);
        }
    }
    private void Falling()
    {
        //Character is falling code
        if (rb.linearVelocity.y < 0 && !hang_time_active)
        {
            is_jumping = false;
            rb.gravityScale = default_gravity_scale * playerData.fall_multiplier;
        }
        else
        {
            rb.gravityScale = default_gravity_scale;
        }
    }

    private void CoyoteTimer()
    {
        if (!IsGrounded() && Math.Abs(rb.linearVelocity.y) > 0f)
        {
            coyote_time_remaining -= Time.deltaTime;
        }

        if (!IsGrounded())
        {
            if (coyote_time_remaining <= 0f)
                can_coyote_jump = false;
        }
        else if (IsGrounded() && !is_jumping)
        {
            coyote_time_remaining = playerData.coyote_time;
            can_coyote_jump = true;
        }
    }

    private bool IsGrounded()
    {
        Collider2D hit = Physics2D.OverlapCircle(
            ground_check.position,
            playerData.ground_check_radius,
            playerData.ground_layer
        );

        return hit != null;
    }

    public void OnAttackHitboxActive()
    {
        Debug.Log("Here");
        hitbox.enabled = true;
        ApplyHitboxSize();
    }

    public void OnAttackHitboxInActive()
    {
        hitbox.enabled = false;
    }

    [ContextMenu("Preview Hitbox Size")]
    private void PreviewHitboxSize()
    {
        if (hitbox == null) return;
        hitbox.enabled = true;
        ApplyHitboxSize();
    }

    [ContextMenu("Hide hitbox Size")]
    private void ResetHitbox()
    {
        if (hitbox == null) return;
        hitbox.enabled = false;
    }

    private void ApplyHitboxSize()
    {
        float width = playerData.attack_1_hitbox_left + playerData.attack_1_hitbox_right;
        float height = playerData.attack_1_hitbox_top + playerData.attack_1_hitbox_bottom;

        float offsetX = (playerData.attack_1_hitbox_right - playerData.attack_1_hitbox_left) / 2f;
        float offsetY = (playerData.attack_1_hitbox_top - playerData.attack_1_hitbox_bottom) / 2f;

        hitbox.size = new Vector2(width, height);
        hitbox.offset = new Vector2(offsetX, offsetY);
    }

    private void OnValidate()
    {
        if (playerData == null || hitbox == null) return;

        // Resubscribe every time to avoid duplicate subscriptions
        playerData.OnPlayerDataChanged -= UpdateHitboxPreview;
        playerData.OnPlayerDataChanged += UpdateHitboxPreview;

        UpdateHitboxPreview();
    }

    private void UpdateHitboxPreview()
    {
        if (hitbox == null || playerData == null) return;
        ApplyHitboxSize();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(ground_check.position, playerData.ground_check_radius);

        if (hitbox == null) return;

        Gizmos.color = hitbox.enabled ? Color.red : Color.green;

        // Draw the hitbox using its world position + size
        Gizmos.DrawWireCube(hitbox.bounds.center, hitbox.bounds.size);
    }
}
