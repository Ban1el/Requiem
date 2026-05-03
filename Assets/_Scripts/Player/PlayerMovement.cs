using System;
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
    private float coyote_time_remaining;
    private bool can_coyote_jump = false;
    private bool hang_time_active = false;
    enum PlayerState
    {
        Idle,
        Running,
        Airborne,
        Falling,
        Attack1
    }
    PlayerState state;

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
        animator.Play("Idle");
    }

    private void StartRunning()
    {
        animator.Play("Run");
    }

    private void StartAirborne()
    {
        animator.Play("Jump");
    }

    private void UpdateIdle()
    {
        if (IsGrounded() || xInput == 0)
        {
            stateComplete = true;
        }
    }

    private void UpdateRun()
    {
        if (xInput == 0 && !IsGrounded())
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

    private void OnEnable()
    {
        actions.Player.Enable();
        actions.Player.Move.performed += CheckInput;
        actions.Player.Jump.performed += Jump;
        actions.Player.Move.canceled += CheckInput;
        actions.Player.Jump.canceled += Jump;
    }

    private void OnDisable()
    {
        actions.Player.Disable();
        actions.Player.Move.performed -= CheckInput;
        actions.Player.Jump.performed -= Jump;
        actions.Player.Move.canceled -= CheckInput;
        actions.Player.Jump.canceled -= Jump;
    }

    private void CheckInput(InputAction.CallbackContext ctx)
    {
        xInput = ctx.ReadValue<Vector2>().x;
        yInput = ctx.ReadValue<Vector2>().y;
    }

    private void Jump(InputAction.CallbackContext ctx)
    {
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

    private void HandleXMovement()
    {
        rb.linearVelocity = new Vector2(xInput * playerData.movement_speed, rb.linearVelocity.y);

        float velX = rb.linearVelocity.x;
        animator.speed = Mathf.Abs(velX) / playerData.movement_speed;

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
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }
        else
        {
            transform.rotation = Quaternion.Euler(0f, -180f, 0f);
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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(ground_check.position, playerData.ground_check_radius);
    }


}
