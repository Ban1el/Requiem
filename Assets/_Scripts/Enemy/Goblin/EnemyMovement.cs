using System.Collections;
using TMPro;
using UnityEngine;

public class EnemyMovement : MonoBehaviour, IDamageable
{
    public Animator animator;

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

    [SerializeField]
    private float health = 100f;
    private bool isStaggered = false;
    private bool is_facing_right = true;

    [SerializeField]
    private float staggerTime = 5f;
    private Coroutine staggerCoroutine;
    private bool playerInAttackRange = false;
    private bool isAnimating = false;
    private bool isAlerted = false;
    [SerializeField]
    private CircleCollider2D playerDetectionCollider;

    enum EnemyState
    {
        Idle,
        Patrol,
        Alert,
        ApproachPlayer,
        Attack,
        Stagger,
        KnockBack
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
        Flip();
    }

    private void Update()
    {
        if (stateComplete)
        {
            SelectState();
        }

        UpdateState();

        stateIndicator.text = $"State: {state}\nHealth: {health}";
    }

    private void SelectState()
    {
        // if (playerInAttackRange)
        // {
        //     state = EnemyState.Attack;
        //     StartAttack();
        // }
        if (!playerDetected)
        {
            state = EnemyState.Patrol;
            StartPatrol();
        }
        else if (playerDetected && !isAlerted)
        {
            Debug.Log("ALERTED");
            state = EnemyState.Alert;
            StartAlert();
        }
        else if (playerDetected && isAlerted)
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

            case EnemyState.Alert:
                UpdateAlert();
                break;

            case EnemyState.ApproachPlayer:
                UpdateApproachPlayer();
                break;

            case EnemyState.Stagger:
                UpdateStagger();
                break;
        }
    }

    private void StartAttack()
    {
        stateComplete = false;
    }

    private void UpdateAlert()
    {
        if (!isAnimating)
        {
            Debug.Log("HERE");
            stateComplete = true;
        }
    }

    private void StartAlert()
    {
        StopMovement();
        stateComplete = false;
        isAnimating = true;
        isAlerted = true;
        animator.Play("Alert");
    }

    private void StartPatrol()
    {
        stateComplete = false;
    }

    private void StopMovement()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    private void UpdatePatrol()
    {
        if (isPausing)
        {
            animator.Play("Idle");
            return;
        }

        animator.Play("walk");

        float directionX = currentTarget.x > transform.position.x ? 1f : -1f;
        Vector2 targetVelocity = new Vector2(directionX * movementSpeed, rb.linearVelocity.y);
        rb.linearVelocity = targetVelocity;
        float dist = Vector2.Distance(transform.position, currentTarget);

        if (dist < 0.1f)
        {
            StopMovement();
            StartCoroutine(PauseAndPickNext());
        }
    }

    private void StartApproachPlayer()
    {
        isPausing = false;
        stateComplete = false;
        animator.Play("Approach");
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
            playerInAttackRange = true;
        }
    }

    private void StartStagger()
    {
        stateComplete = false;
    }

    private void UpdateStagger()
    {

    }

    private void PickRandomTarget()
    {
        float randomX = UnityEngine.Random.Range(pointA.position.x, pointB.position.x);
        currentTarget = new Vector2(randomX, transform.position.y);
    }

    private void Flip()
    {
        if (rb.linearVelocity.x > 0.1f)
            is_facing_right = true;
        else if (rb.linearVelocity.x < -0.1f)
            is_facing_right = false;

        transform.localScale = is_facing_right ? new Vector3(1f, 1f, 1f) : new Vector3(-1f, 1f, 1f);
    }


    private IEnumerator PauseAndPickNext()
    {
        isPausing = true;
        yield return new WaitForSeconds(pauseAmount);
        PickRandomTarget();
        isPausing = false;
        stateComplete = true;
    }

    private IEnumerator StaggerWaitTime()
    {
        yield return new WaitForSeconds(staggerTime);
        stateComplete = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Triggered");

        if (isStaggered) return;
        if (((1 << other.gameObject.layer) & playerLayer) == 0) return;

        playerObj = other.gameObject;
        playerDetected = true;
        stateComplete = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log("TriggeredExit");

        if (((1 << other.gameObject.layer) & playerLayer) == 0) return;

        playerObj = null;
        isAlerted = false;
        playerDetected = false;
        stateComplete = true;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }

    public void TakeDamage(float amount, float pushBackValue, Vector2 hitPosition)
    {
        isStaggered = true;
        stateComplete = false;
        state = EnemyState.Stagger;
        StartStagger();

        float direction = hitPosition.x < transform.position.x ? 1f : -1f;
        rb.AddForce(new Vector2(pushBackValue * direction, 0f), ForceMode2D.Impulse);
        health -= amount;

        if (staggerCoroutine != null)
            StopCoroutine(staggerCoroutine);

        staggerCoroutine = StartCoroutine(StaggerWaitTime());
    }

    public void OnAttackHitboxActive()
    {
        // hitbox.enabled = true;
        // ApplyHitboxSize();
    }

    public void OnAttackHitboxInActive()
    {
        // hitbox.enabled = false;
    }

    public void StepForward()
    {
        // float direction = is_facing_right ? 1f : -1f;
        // rb.linearVelocity = new Vector2(stepValue * direction, rb.linearVelocity.y);
    }

    public void AnimationEnd()
    {
        isAnimating = false;
    }

    public void CanAttack()
    {
        // canAttack = true;
    }
}
