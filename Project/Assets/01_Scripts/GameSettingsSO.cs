using UnityEngine;

[CreateAssetMenu(fileName = "GameSettingsSO", menuName = "Scriptable Objects/GameSettingsSO")]
public class GameSettingsSO : ScriptableObject
{
    [Header("Game Rule Settings")]
    public float GameTimeLimit = 180f;
    public int TargetFrameRate = 144;
    public float SpawnInterval = 5f;
    public int EnemyLevel = 1;
    public Vector3 StartPosition = Vector3.zero;
    public Vector3 BossSpawnPosition = Vector3.one;
    public float[] WaveTimes = { 0f, 30f, 60f, 90f };
}