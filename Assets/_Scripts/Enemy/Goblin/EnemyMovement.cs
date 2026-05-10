using System.Collections;
using Unity.VisualScripting;
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
    private bool stateComplete;
    private Rigidbody2D rb;
    private Vector2 currentVelocity;
    private Vector2 currentTarget;
    private bool isPausing;
    EnemyState state;

    enum EnemyState
    {
        Idle,
        Patrol
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        PickRandomTarget();
    }

    private void FixedUpdate()
    {
        if (!isPausing)
            UpdatePatrol();
    }

    private void Update()
    {
        // if (stateComplete)
        // {
        //     SelectState();
        // }

        // UpdateState();
    }

    private void SelectState()
    {
        StartPatrol();
    }


    private void UpdateState()
    {
        switch (state)
        {
            case EnemyState.Patrol:
                StartPatrol();
                break;
        }
    }

    private void StartPatrol()
    {
    }

    private void UpdatePatrol()
    {
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

    private void PickRandomTarget()
    {
        float randomX = Random.Range(pointA.position.x, pointB.position.x);
        currentTarget = new Vector2(randomX, transform.position.y);
    }

    private IEnumerator PauseAndPickNext()
    {
        isPausing = true;
        yield return new WaitForSeconds(pauseAmount);
        PickRandomTarget();
        isPausing = false;
    }
}
