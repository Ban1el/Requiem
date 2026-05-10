using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField]
    private Transform pointA;

    [SerializeField]
    private Transform pointB;

    [SerializeField]
    private float pauseAmount;

    [SerializeField]
    private float movementSpeed;
    [SerializeField]
    private float approachSpeed;
    private bool stateComplete;
    private Rigidbody2D rb;
    private Vector2 currentVelocity;
    private Vector2 currentTarget;
    private bool isPausing;
    private bool playerDetected = false;
    EnemyState state;
    private GameObject playerObj;

    [SerializeField]
    private float approachStopDistance = 1f;

    enum EnemyState
    {
        Idle,
        Patrol,
        ApproachPlayer,
        Attack
    }

    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private LayerMask playerLayer;

    //Debugging
    [SerializeField]
    private TextMeshProUGUI stateIndicator;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        PickRandomTarget();
        state = EnemyState.Patrol;
    }

    private void FixedUpdate()
    {
        UpdateFixedState();
    }

    private void Update()
    {
        DetectPlayer();

        if (stateComplete)
        {
            SelectState();
        }

        UpdateState();

        stateIndicator.text = $"State: {state}\nPlayer Detected {playerDetected}\nPausing {isPausing}";
    }

    private void SelectState()
    {
        if (!playerDetected)
        {
            state = EnemyState.Patrol;
            StartPatrol();
        }
        else if (playerDetected)
        {
            state = EnemyState.ApproachPlayer;
            StartApproachPlayer();
        }
    }


    private void UpdateState()
    {
    }

    private void UpdateFixedState()
    {
        switch (state)
        {
            case EnemyState.Patrol:
                UpdatePatrol();
                break;

            case EnemyState.ApproachPlayer:
                UpdateApproachPlayer();
                break;
        }
    }

    private void StartPatrol()
    {
        stateComplete = false;
    }

    private void UpdatePatrol()
    {
        if (isPausing) return;

        float directionX = currentTarget.x > transform.position.x ? 1f : -1f;
        Vector2 targetVelocity = new Vector2(directionX * movementSpeed, rb.linearVelocity.y);
        rb.linearVelocity = targetVelocity;
        float dist = Vector2.Distance(transform.position, currentTarget);

        if (dist < 0.1f)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            StartCoroutine(PauseAndPickNext());
        }
    }

    private void StartApproachPlayer()
    {
        isPausing = false;
        stateComplete = false;
    }

    private void UpdateApproachPlayer()
    {
        Vector2 target = new Vector2(playerObj.transform.position.x, this.transform.position.y);
        float directionX = target.x > transform.position.x ? 1f : -1f;
        Vector2 targetVelocity = new Vector2(directionX * approachSpeed, rb.linearVelocity.y);
        rb.linearVelocity = targetVelocity;
        float dist = Vector2.Distance(transform.position, target);

        if (dist < approachStopDistance)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            stateComplete = true;
        }
    }

    private void PickRandomTarget()
    {
        float randomX = UnityEngine.Random.Range(pointA.position.x, pointB.position.x);
        currentTarget = new Vector2(randomX, transform.position.y);
    }

    private IEnumerator PauseAndPickNext()
    {
        isPausing = true;
        yield return new WaitForSeconds(pauseAmount);
        PickRandomTarget();
        isPausing = false;
        stateComplete = true;
    }

    private void DetectPlayer()
    {
        Collider2D player = Physics2D.OverlapCircle(transform.position, detectionRadius, playerLayer);

        if (player != null)
        {
            playerObj = player.gameObject;
            playerDetected = true;
            stateComplete = true;
        }
        else
        {
            playerDetected = false;
            stateComplete = true;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
