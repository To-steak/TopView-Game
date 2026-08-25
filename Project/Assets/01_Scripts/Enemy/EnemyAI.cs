using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [SerializeField] private EnemyAISO enemyAISO;

    public float NextStepTime => enemyAISO.NextStepTime;
    public float AttackRotationSpeed => enemyAISO.AttackRotationSpeed;

    private NavMeshAgent _agent;
    private Collider[] _hits = new Collider[1];

    public void Initialize()
    {
        _agent = GetComponent<NavMeshAgent>();
        if (enemyAISO != null)
        {
            _agent.speed = enemyAISO.Speed;
        }
    }

    public void SetMoveToPosition(Vector3 position)
    {
        _agent.isStopped = false;
        _agent.stoppingDistance = 0f;
        _agent.SetDestination(position);
    }

    public void SetMoveToTarget(Transform target)
    {
        _agent.isStopped = false;
        _agent.stoppingDistance = enemyAISO.AttackDistance;
        _agent.SetDestination(target.position);
    }

    public void StopMove()
    {
        _agent.ResetPath();
        _agent.velocity = Vector3.zero;
        _agent.isStopped = true;
    }

    public Transform DetectPlayer()
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, enemyAISO.DetectionRadius, _hits, enemyAISO.DetectionLayer);
        if (count > 0)
        {
            return _hits[0].transform;
        }

        return null;
    }

    public bool GetRandomPosition(out Vector3 result)
    {
        Vector3 direction = Random.insideUnitSphere * enemyAISO.DetectionRadius;
        direction += transform.position;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(direction, out hit, enemyAISO.DetectionRadius, NavMesh.AllAreas))
        {
            result = hit.position;
            return true;
        }

        result = Vector3.zero;
        return false;
    }

    public bool HasReachedDestination()
    {
        if (!_agent.pathPending)
        {
            if (_agent.remainingDistance <= _agent.stoppingDistance)
            {
                if (!_agent.hasPath || _agent.velocity.sqrMagnitude < 0.01f)
                {
                    return true;
                }
            }
        }
        return false;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (enemyAISO != null)
        {
            // 1. 선명한 노란색으로 바닥에 테두리 선을 먼저 그립니다.
            UnityEditor.Handles.color = Color.yellow;
            UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.up, enemyAISO.DetectionRadius);

            // 2. 투명도를 낮춘 노란색으로 원 내부를 반투명하게 채워줍니다.
            UnityEditor.Handles.color = new Color(1f, 1f, 0f, 0.15f); // 알파값 0.15
            UnityEditor.Handles.DrawSolidDisc(transform.position, Vector3.up, enemyAISO.DetectionRadius);

            enemyAISO.OnDrawAttackGizmos(transform);
        }
    }
#endif
}