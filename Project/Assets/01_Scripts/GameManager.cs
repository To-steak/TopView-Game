using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Game Settings
    public GameSettingsSO settingsSO;
    // Prefab
    public GameObject Player;
    public GameObject BossPrefab;
    // Camera
    public CinemachineCamera environmentCam;
    public CinemachineCamera playerCam;
    // Pool
    public EnemyPool[] enemyPools;
    public Transform[] spawnPositions;
    // Canvas
    public Transform worldCanvas;
    public Transform overlayCanvas;
    // View
    public GameInfoView infoView;

    private float _remainingTime;
    private int _currentScore;
    private GameObject _spawnedPlayer;
    private bool[] _isSpawning;
    private float[] _spawnTimers;
    private bool[] _wave;
    private bool _isGameOver;
    private bool _isGamestarted;
    private PlayerMediator _mediator;

    void Awake()
    {
        _isSpawning = new bool[enemyPools.Length];
        _spawnTimers = new float[enemyPools.Length];
        _isGameOver = false;
        _isGamestarted = false;
        _currentScore = 0;
    }

    void OnEnable()
    {
        GameEvents.OnEntityDied += AddScore;
        GameEvents.OnPlayerDied += CallbackGameOver;
        GameEvents.OnBossDied += CallbackGameOver;
    }

    void OnDisable()
    {
        GameEvents.OnEntityDied -= AddScore;
        GameEvents.OnPlayerDied -= CallbackGameOver;
        GameEvents.OnBossDied -= CallbackGameOver;
    }

    void Start()
    {
        if (settingsSO != null)
        {
            _remainingTime = settingsSO.GameTimeLimit;
            _wave = new bool[settingsSO.WaveTimes.Length];
            Application.targetFrameRate = settingsSO.TargetFrameRate;
        }

        _spawnedPlayer = Instantiate(Player, settingsSO.StartPosition, Quaternion.identity);
        _mediator = _spawnedPlayer.GetComponent<PlayerMediator>();
        _spawnedPlayer.SetActive(false);

        Transform followTarget = null;

        if (_mediator != null)
        {
            followTarget = _mediator.gameObject.transform;
        }

        if (followTarget != null && playerCam != null)
        {
            playerCam.Follow = followTarget;
            playerCam.Priority = 2;
        }

        Time.timeScale = 0;
    }

    void Update()
    {
        if (!_isGamestarted)
        {
            return;
        }

        if (_remainingTime > 0)
        {
            _remainingTime -= Time.deltaTime;
            infoView.UpdateTimeText(_remainingTime);
            if (_remainingTime <= 0)
            {
                _remainingTime = 0;
                CallbackGameOver();
            }
        }

        for (int i = 0; i < _isSpawning.Length; i++)
        {
            if (!_isSpawning[i]) continue;

            _spawnTimers[i] -= Time.deltaTime;
            if (_spawnTimers[i] <= 0f)
            {
                SpawnFromPool(i);
                _spawnTimers[i] = settingsSO.SpawnInterval;
            }
        }

        float elapsed = settingsSO.GameTimeLimit - _remainingTime;
        for (int i = 0; i < settingsSO.WaveTimes.Length; i++)
        {
            if (_wave[i] || elapsed < settingsSO.WaveTimes[i]) continue;

            _wave[i] = true;

            if (i < enemyPools.Length)
                SetSpawning(i, true);
            else
                SpawnBoss();
        }
    }

    public void GameStart()
    {
        _isGamestarted = true;
        infoView.UpdateScoreText(_currentScore);
        _mediator.AttachUI(overlayCanvas, worldCanvas);
        _spawnedPlayer.SetActive(true);

        Time.timeScale = 1f;
        infoView.DeactiveGameStartPanel();
    }

    private void CallbackGameOver()
    {
        if (_isGameOver)
        {
            return;
        }
        _isGameOver = true;
        Time.timeScale = 0f;
        infoView.ActiveGameoverPanel();
    }

    private void AddScore(int score)
    {
        _currentScore += score;
        infoView.UpdateScoreText(_currentScore);
    }

    private void SpawnFromPool(int poolIndex)
    {
        if (enemyPools == null || poolIndex >= enemyPools.Length) return;
        if (spawnPositions == null || spawnPositions.Length == 0) return;

        Transform spawnPoint = spawnPositions[Random.Range(0, spawnPositions.Length)];
        enemyPools[poolIndex].Spawn(spawnPoint.position, spawnPoint.rotation, settingsSO.EnemyLevel);
    }

    private void SetSpawning(int poolIndex, bool active)
    {
        if (poolIndex >= _isSpawning.Length) return;

        _isSpawning[poolIndex] = active;

        if (active)
        {
            SpawnFromPool(poolIndex);
            _spawnTimers[poolIndex] = settingsSO.SpawnInterval;
        }
        else
        {
            _spawnTimers[poolIndex] = 0f;
        }
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
    }

    public void ReStart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    [ContextMenu("Spawn Boss")]
    public void SpawnBoss()
    {
        if (BossPrefab != null)
        {
            GameObject spawnedBoss = Instantiate(BossPrefab, settingsSO.BossSpawnPosition, Quaternion.identity);
            BossMediator bossMediator = spawnedBoss.GetComponent<BossMediator>();
            bossMediator.Initialize(_spawnedPlayer.transform, 1);
            bossMediator.AttachUI(overlayCanvas);
        }
    }

    [ContextMenu("Activate Enemy A")]
    public void ActivateEnemyA() => SetSpawning(0, true);

    [ContextMenu("Activate Enemy B")]
    public void ActivateEnemyB() => SetSpawning(1, true);

    [ContextMenu("Activate Enemy C")]
    public void ActivateEnemyC() => SetSpawning(2, true);

    [ContextMenu("Deactivate Enemy A")]
    public void DeactivateEnemyA() => SetSpawning(0, false);

    [ContextMenu("Deactivate Enemy B")]
    public void DeactivateEnemyB() => SetSpawning(1, false);

    [ContextMenu("Deactivate Enemy C")]
    public void DeactivateEnemyC() => SetSpawning(2, false);
}