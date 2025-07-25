using System.Collections;
using UnityEngine;

public class EffectsManager : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject particleSystemPrefab;

    private GameConfig config;

    void Start()
    {
        config = GameManager.Instance.gameConfig;
        CreateParticleSystemPrefab();
    }

    void CreateParticleSystemPrefab()
    {
        if (particleSystemPrefab == null && config.particleSprites.Length > 0)
        {
            // Create particle system prefab programmatically
            GameObject prefab = new GameObject("CubeExplosionParticle");
            ParticleSystem ps = prefab.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.startLifetime = config.particleLifetime;
            main.startSpeed = 5f;
            main.startSize = 0.3f;
            main.startColor = Color.white;
            main.maxParticles = 15;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0, 10) });

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.3f;

            var velocityOverLifetime = ps.velocityOverLifetime;
            velocityOverLifetime.enabled = true;
            velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
            velocityOverLifetime.radial = new ParticleSystem.MinMaxCurve(2f, 4f);

            // Set texture renderer with proper material
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Shader.Find("Sprites/Default"));

            // Use the first particle sprite
            renderer.material.mainTexture = config.particleSprites[0].texture;
            renderer.sortingLayerName = "Default";
            renderer.sortingOrder = 10; // Above cubes

            particleSystemPrefab = prefab;
            particleSystemPrefab.SetActive(false); // Keep it inactive as prefab
        }
    }

    public void PlayExplosionEffect(Vector3 position, int colorIndex = 0)
    {
        if (particleSystemPrefab != null)
        {
            // Ensure position is valid and not NaN or infinity
            if (float.IsNaN(position.x) || float.IsNaN(position.y) || float.IsNaN(position.z))
            {
                Debug.LogWarning("Invalid particle position detected, skipping effect");
                return;
            }

            GameObject particle = Instantiate(particleSystemPrefab, position, Quaternion.identity);
            particle.SetActive(true);

            // Set particle color based on cube color
            ParticleSystem ps = particle.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                var main = ps.main;

                // Get color from cube color index
                Color particleColor = GetColorFromIndex(colorIndex);
                main.startColor = particleColor;

                // Randomize particle sprite if we have multiple
                var renderer = ps.GetComponent<ParticleSystemRenderer>();
                if (config.particleSprites.Length > 1)
                {
                    int randomSpriteIndex = Random.Range(0, config.particleSprites.Length);
                    renderer.material.mainTexture = config
                        .particleSprites[randomSpriteIndex]
                        .texture;
                }

                // Play the particle system
                ps.Play();

                // Auto-destroy after particle lifetime
                StartCoroutine(DestroyAfterDelay(particle, config.particleLifetime + 0.5f));
            }
        }
        else
        {
            Debug.LogWarning("Particle system prefab is null!");
        }
    }

    Color GetColorFromIndex(int colorIndex)
    {
        // Map color indices to actual colors - you can customize these
        Color[] colors =
        {
            new Color(1f, 0.2f, 0.2f), // Red
            new Color(0.2f, 0.2f, 1f), // Blue
            new Color(0.2f, 1f, 0.2f), // Green
            new Color(1f, 1f, 0.2f), // Yellow
            new Color(1f, 0.2f, 1f) // Magenta
        };
        return colorIndex < colors.Length ? colors[colorIndex] : Color.white;
    }

    IEnumerator DestroyAfterDelay(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (obj != null)
        {
            Destroy(obj);
        }
    }
}
