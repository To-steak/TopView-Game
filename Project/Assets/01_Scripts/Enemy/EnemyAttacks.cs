using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttacks : MonoBehaviour
{
    [SerializeField] private List<EnemyAttackSO> attacks;

    public int Count => attacks.Count;
    private EnemyAttackInstance[] _instances;

    public void Initialize()
    {
        _instances = new EnemyAttackInstance[attacks.Count];
        for (int i = 0; i < attacks.Count; i++)
        {
            if (attacks[i] != null)
            {
                _instances[i] = attacks[i].CreateInstance();
                attacks[i].Initialize(_instances[i]);
            }
        }
    }

    public void Attack(float damage, int index)
    {
        if (attacks == null || attacks.Count == 0)
        {
            return;
        }

        if (index < 0 || index >= attacks.Count)
        {
            return;
        }

        attacks[index].Attack(_instances[index], transform, damage);
    }

    public int GetAnimationHash(int index)
    {
        if (attacks[index] == null) return 0;
        return attacks[index].AnimationHash;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (attacks == null || attacks.Count == 0) return;
        foreach (var attack in attacks)
        {
            if (attack == null) continue;
            attack.OnDrawAttackGizmos(transform);
        }
    }
#endif
}
