using System;
using System.Collections;
using TMPro;
using TMPro.Examples;
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
    private bool isAirAttacking = false;
    private bool canAttack = true;
    private bool hitboxActive = false;
    [SerializeField]
    private BoxCollider2D hitbox;
    private float hit_box_left = 0f;
    private float hit_box_right = 0f;
    private float hit_box_top = 0f;
    private float hit_box_bottom = 0f;
    [Header("Hitbox Preview")]
    [SerializeField] private GroundAttackState previewAttackState = GroundAttackState.attack_1;

    //Debugging
    [SerializeField]
    private TextMeshProUGUI stateIndicator;
    private PlayerHealth playerHealth;

    //Enemy
    private GameObject nearestEnemy;
    [SerializeField]
    private Transform LockSprite;
    [SerializeField] private float enemyDetectRadius = 5f;
    [SerializeField] private LayerMask enemyLayer;
    private bool airAttack = false;
    [SerializeField]
    private float gapCloserRangeSqr = 2f;
    [SerializeField]
    private float airGapCloserForcevalue = 20f;
    [SerializeField] private float gapCloserForce = 15f;
    [SerializeField] private float arcUpwardBias = 0.5f;

    enum PlayerState
    {
        Idle,
        Running,
        Airborne,
        Falling,
        GroundAttack,
        AirGapCloser,
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
        Physics2D.IgnoreLayerCollision(
        LayerMask.NameToLayer("Player"),
        LayerMask.NameToLayer("Enemy")
    );
        rb = GetComponent<Rigidbody2D>();
        actions = new InputSystem_Actions();
        playerHealth = GetComponent<PlayerHealth>();

        playerHealth.SetMaxHealth(playerData.health);
    }

    private void Start()
    {
        hitbox.enabled = false;
        coyote_time_remaining = playerData.coyote_time;
        default_gravity_scale = rb.gravityScale;
    }

    private void FixedUpdate()
    {
        HandleXMovement();
    }

    private void Update()
    {
        UpdateNearestEnemy();
        Falling();
        CoyoteTimer();
        FlipPlayer();

        if (stateComplete)
        {
            SelectState();
        }

        UpdateState();

        stateIndicator.text = $"State: {state}";
        //Debug.Log(stateIndicator.text);
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
            case PlayerState.AirGapCloser:
                UpdateAirGapCloserAttack();
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

        hit_box_left = playerData.attack_1_hitbox_left;
        hit_box_right = playerData.attack_1_hitbox_right;
        hit_box_top = playerData.attack_1_hitbox_top;
        hit_box_bottom = playerData.attack_1_hitbox_bottom;

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

        hit_box_left = playerData.attack_2_hitbox_left;
        hit_box_right = playerData.attack_2_hitbox_right;
        hit_box_top = playerData.attack_2_hitbox_top;
        hit_box_bottom = playerData.attack_2_hitbox_bottom;

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

        hit_box_left = playerData.attack_3_hitbox_left;
        hit_box_right = playerData.attack_3_hitbox_right;
        hit_box_top = playerData.attack_3_hitbox_top;
        hit_box_bottom = playerData.attack_3_hitbox_bottom;

        stepValue = playerData.attack_3_step_value;
        groundAttackState = GroundAttackState.attack_3;
        canMove = false;
        canAttack = false;
        isAnimating = true;
        animator.Play("Attack-3");
    }

    private void StartAirGapCloser()
    {
        StopVelocity();
        isAirAttacking = true;
        stateComplete = false;
        animator.speed = 1f;
        canMove = false;
        canAttack = false;
        isAnimating = true;
        animator.Play("Air-Attack-Gap-Closer");
        AddArcForceTowardsEnemy();
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
        if (!isAnimating)
        {
            is_dodge_rolling = false;
        }

        if (!is_dodge_rolling)
        {
            stateComplete = true;
        }
    }

    private void UpdateAirGapCloserAttack()
    {
        if (!isAnimating)
        {
            isAirAttacking = false;
        }

        if (!isAirAttacking)
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

        TryAirAttack();
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
        hitbox.enabled = true;
        ApplyHitboxSize();
    }

    public void OnAttackHitboxInActive()
    {
        hitbox.enabled = false;
    }

    private void SetAttackHitbox()
    {
        switch (previewAttackState)
        {
            case GroundAttackState.attack_1:
                hit_box_left = playerData.attack_1_hitbox_left;
                hit_box_right = playerData.attack_1_hitbox_right;
                hit_box_top = playerData.attack_1_hitbox_top;
                hit_box_bottom = playerData.attack_1_hitbox_bottom;
                break;
            case GroundAttackState.attack_2:
                hit_box_left = playerData.attack_2_hitbox_left;
                hit_box_right = playerData.attack_2_hitbox_right;
                hit_box_top = playerData.attack_2_hitbox_top;
                hit_box_bottom = playerData.attack_2_hitbox_bottom;
                break;
            case GroundAttackState.attack_3:
                hit_box_left = playerData.attack_3_hitbox_left;
                hit_box_right = playerData.attack_3_hitbox_right;
                hit_box_top = playerData.attack_3_hitbox_top;
                hit_box_bottom = playerData.attack_3_hitbox_bottom;
                break;
        }
    }

    [ContextMenu("Preview Hitbox Size")]
    private void PreviewHitboxSize()
    {
        if (hitbox == null || playerData == null) return;
        SetAttackHitbox();
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
        float width = hit_box_left + hit_box_right;
        float height = hit_box_top + hit_box_bottom;

        float offsetX = (hit_box_right - hit_box_left) / 2f;
        float offsetY = (hit_box_top - hit_box_bottom) / 2f;

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
        SetAttackHitbox();
        ApplyHitboxSize();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            IDamageable damageable = other.GetComponent<IDamageable>();
            damageable?.TakeDamage(5f, 3f, transform.position);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(ground_check.position, playerData.ground_check_radius);

        // Enemy detection radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, enemyDetectRadius);

        if (nearestEnemy != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, nearestEnemy.transform.position);
        }

        if (hitbox == null) return;

        Gizmos.color = hitbox.enabled ? Color.red : Color.green;

        // Draw the hitbox using its world position + size
        Gizmos.DrawWireCube(hitbox.bounds.center, hitbox.bounds.size);
    }


    private void UpdateNearestEnemy()
    {
        nearestEnemy = null;
        float nearestDistSqr = Mathf.Infinity;
        Vector2 currentPos = transform.position;

        Collider2D[] hits = Physics2D.OverlapCircleAll(currentPos, enemyDetectRadius, enemyLayer);

        foreach (Collider2D hit in hits)
        {
            float distSqr = (currentPos - (Vector2)hit.transform.position).sqrMagnitude;
            if (distSqr < nearestDistSqr)
            {
                nearestDistSqr = distSqr;
                nearestEnemy = hit.gameObject;
                LockSprite.transform.position = nearestEnemy.transform.position;
                //Debug.Log("Enemy detected");
            }
        }
    }

    private void TryAirAttack()
    {
        if (nearestEnemy == null) return;

        Enemy enemy = nearestEnemy.GetComponent<Enemy>();

        if (enemy == null || !enemy.IsAirborne) return;

        float distSqr = ((Vector2)transform.position - (Vector2)nearestEnemy.transform.position).sqrMagnitude;

        airAttack = true; // mark state as active

        if (distSqr > gapCloserRangeSqr)
        {
            stateComplete = false;
            isAirAttacking = true;
            state = PlayerState.AirGapCloser;
            StartAirGapCloser();
        }
        else
        {
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
    }

    private void AddArcForceTowardsEnemy()
    {
        if (nearestEnemy == null) return;

        Vector2 direction = ((Vector2)nearestEnemy.transform.position - rb.position).normalized;

        // Blend the direct direction with a bit of "up"
        Vector2 arcDirection = (direction + Vector2.up * arcUpwardBias).normalized;

        rb.AddForce(arcDirection * airGapCloserForcevalue, ForceMode2D.Impulse);
    }

    #region HEALTH
    public void TakeDamage(float damage)
    {

    }
    #endregion
}
