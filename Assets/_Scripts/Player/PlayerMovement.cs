using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private PlayerData playerData;
    private Rigidbody2D rb;
    public InputSystem_Actions actions;
    private float horizontal_movement;
    private bool is_facing_right = true;

    [SerializeField]
    private Transform ground_check;
    private float default_gravity_scale;
    private bool is_jumping = false;
    private float coyote_time_remaining;
    private bool can_coyote_jump = false;
    private bool hang_time_active = false;
    private bool can_move_horizontally = true;

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
        HorizontalMovement();
    }

    private void Update()
    {
        Falling();
        CoyoteTimer();
        FlipPlayer();
    }

    private void OnEnable()
    {
        actions.Player.Enable();
        actions.Player.Move.performed += SetHorizontalValue;
        actions.Player.Jump.performed += Jump;
        actions.Player.Move.canceled += SetHorizontalValue;
    }

    private void OnDisable()
    {
        actions.Player.Disable();
        actions.Player.Move.performed -= SetHorizontalValue;
        actions.Player.Jump.performed -= Jump;
        actions.Player.Move.canceled -= SetHorizontalValue;
    }

    private void SetHorizontalValue(InputAction.CallbackContext ctx)
    {
        horizontal_movement = ctx.ReadValue<Vector2>().x;
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

    private void HorizontalMovement()
    {
        if (can_move_horizontally)
        {
            rb.linearVelocity = new Vector2(horizontal_movement * playerData.movement_speed, rb.linearVelocity.y);
        }

        //Flip the player
        if (horizontal_movement > 0.1f)
        {
            is_facing_right = true;
        }

        if (horizontal_movement < -0.1f)
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
