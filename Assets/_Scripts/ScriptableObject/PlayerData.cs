using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "Scriptable Objects/PlayerData")]
public class PlayerData : ScriptableObject
{
    [Header("Movement Settings")]
    public float movement_speed = 9f;
    [Header("Jump Settings")]
    public float jump_force = 13f;
    public float jump_start_time = 0.25f;
    public float fall_multiplier = 3f;
    public float ground_check_radius = 0.1f;
    public LayerMask ground_layer;
    public float coyote_time = 0.1f;
    public float hang_time_gravity = 0.1f;
    public float hang_time = 0.5f;
    public float release_jump_vel_modifier = 2f;
    [Header("Animation Settings")]
    public float attack_1_step_value = 0f;
}
