using UnityEngine;

public class BalloonController : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    public Vector2Int GridPosition { get; private set; }

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Initialize(Vector2Int gridPos)
    {
        GridPosition = gridPos;
        if (GameManager.Instance != null && GameManager.Instance.gameConfig != null)
        {
            spriteRenderer.sprite = GameManager.Instance.gameConfig.balloonSprite;
        }
        gameObject.name = $"Balloon_{gridPos.x}_{gridPos.y}";
    }

    public void SetGridPosition(Vector2Int newPos)
    {
        GridPosition = newPos;
        gameObject.name = $"Balloon_{newPos.x}_{newPos.y}";
    }

    public void Explode()
    {
        GameManager.Instance.CollectBalloon();

        GameManager.Instance.audioManager.PlayBalloonSound();
        GameManager.Instance.effectsManager.PlayExplosionEffect(transform.position);

        Destroy(gameObject);
    }
}
