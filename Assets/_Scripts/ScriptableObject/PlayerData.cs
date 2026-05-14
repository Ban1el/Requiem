using System;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "Scriptable Objects/PlayerData")]
public class PlayerData : ScriptableObject
{
    public event Action OnPlayerDataChanged;

    private void OnValidate()
    {
        OnPlayerDataChanged?.Invoke();
    }

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
    [Header("Attack 1")]
    public float attack_1_step_value = 0f;
    public float attack_1_hitbox_left = 0f;
    public float attack_1_hitbox_right = 0f;
    public float attack_1_hitbox_top = 0f;
    public float attack_1_hitbox_bottom = 0f;
    [Header("Attack 2")]
    public float attack_2_step_value = 0f;
    public float attack_2_hitbox_left = 0f;
    public float attack_2_hitbox_right = 0f;
    public float attack_2_hitbox_top = 0f;
    public float attack_2_hitbox_bottom = 0f;
    [Header("Attack 3")]
    public float attack_3_step_value = 0f;
    public float attack_3_hitbox_left = 0f;
    public float attack_3_hitbox_right = 0f;
    public float attack_3_hitbox_top = 0f;
    public float attack_3_hitbox_bottom = 0f;
    [Header("Dodge Roll")]
    public float dodge_roll_step_value = 0f;
}
