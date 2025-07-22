using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "Match2Game/GameConfig")]
public class GameConfig : ScriptableObject
{
    [Header("Grid Settings")]
    public int gridWidth = 6;
    public int gridHeight = 8;
    public float cellSize = 1.0f;
    public float gridSpacing = 0.1f;

    [Header("Gameplay Settings")]
    public int movesPerLevel = 20;
    public int minMatchSize = 2;
    public int rocketTriggerSize = 5;

    [Header("Cube Settings")]
    public Sprite[] cubeSprites = new Sprite[5]; // Assign cube_1 to cube_5 sprites here

    [Header("Special Object Sprites")]
    public Sprite balloonSprite;
    public Sprite duckSprite;
    public Sprite rocketHorizontalSprite;
    public Sprite rocketVerticalSprite;

    [Header("UI Sprites")]
    public Sprite backgroundSprite;
    public Sprite topUISprite;
    public Sprite bottomUISprite;
    public Sprite bordersSprite;

    [Header("Particle Sprites")]
    public Sprite[] particleSprites = new Sprite[2]; // cubeParticle1 and cubeParticle2

    [Header("Level Goals")]
    public LevelGoal[] levelGoals;

    [Header("Animation Settings")]
    public float cubeFallSpeed = 5f;
    public float explosionDuration = 0.5f;
    public float rocketMoveSpeed = 10f;
    public float particleLifetime = 1f;

    [Header("Audio")]
    public AudioClip cubeExplodeSound;
    public AudioClip cubeCollectSound;
    public AudioClip balloonSound;
    public AudioClip duckSound;
}

[System.Serializable]
public class LevelGoal
{
    public int colorIndex;
    public int targetAmount;
    public int currentAmount;
}
