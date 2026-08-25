using UnityEngine;
using UnityEngine.Pool;

public class EnemyPool : MonoBehaviour
{
    [SerializeField] private EnemyMediator enemyPrefab;
    [SerializeField] private GaugeWorldView gaugePrefab;
    [SerializeField] private Transform worldSpaceCanvas;
    [SerializeField] private int defaultCapacity = 10;
    [SerializeField] private int maxPoolSize = 20;

    private IObjectPool<EnemyMediator> _enemyPool;
    private IObjectPool<GaugeWorldView> _uiPool;

    private void Awake()
    {
        _enemyPool = new ObjectPool<EnemyMediator>(
            createFunc: CreateEnemy,
            actionOnRelease: Release,
            actionOnDestroy: enemy => Destroy(enemy.gameObject),
            collectionCheck: true,
            defaultCapacity: defaultCapacity,
            maxSize: maxPoolSize
        );

        _uiPool = new ObjectPool<GaugeWorldView>(
            createFunc: CreateUI,
            actionOnRelease: ui => ui.gameObject.SetActive(false),
            actionOnDestroy: ui => Destroy(ui.gameObject),
            collectionCheck: true,
            defaultCapacity: defaultCapacity,
            maxSize: maxPoolSize
        );
    }

    private EnemyMediator CreateEnemy()
    {
        EnemyMediator enemy = Instantiate(enemyPrefab, transform);
        enemy.SetPool(_enemyPool);
        return enemy;
    }

    private GaugeWorldView CreateUI()
    {
        return Instantiate(gaugePrefab, worldSpaceCanvas);
    }

    private void Release(EnemyMediator enemy)
    {
        var view = enemy.Presenter.DetachUI();
        if (view != null)
        {
            _uiPool.Release(view);
        }
        enemy.gameObject.SetActive(false);
    }

    public void Spawn(Vector3 position, Quaternion rotation, int level)
    {
        EnemyMediator enemy = _enemyPool.Get();
        var uiView = _uiPool.Get();

        enemy.transform.position = position;
        enemy.transform.rotation = rotation;

        enemy.Health.PrepareSpawn(level);
        enemy.Presenter.AttachUI(uiView);

        enemy.gameObject.SetActive(true);
        uiView.gameObject.SetActive(true);
    }
}