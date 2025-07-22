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
        if (particleSystemPrefab == null)
        {
            // Create particle system prefab programmatically
            GameObject prefab = new GameObject("CubeExplosionParticle");
            ParticleSystem ps = prefab.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.startLifetime = config.particleLifetime;
            main.startSpeed = 5f;
            main.startSize = 0.2f;
            main.startColor = Color.white;
            main.maxParticles = 20;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0, 15) });

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.5f;

            var velocityOverLifetime = ps.velocityOverLifetime;
            velocityOverLifetime.enabled = true;
            velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
            velocityOverLifetime.radial = new ParticleSystem.MinMaxCurve(3f);

            // Set texture renderer
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            if (config.particleSprites.Length > 0)
            {
                renderer.material = new Material(Shader.Find("Sprites/Default"));
                renderer.material.mainTexture = config.particleSprites[0].texture;
            }

            particleSystemPrefab = prefab;
        }
    }

    public void PlayExplosionEffect(Vector3 position, int colorIndex = 0)
    {
        if (particleSystemPrefab != null)
        {
            GameObject particle = Instantiate(particleSystemPrefab, position, Quaternion.identity);

            // Set particle color based on cube color
            ParticleSystem ps = particle.GetComponent<ParticleSystem>();
            var main = ps.main;

            // Get color from cube sprite (you can customize this)
            Color particleColor = GetColorFromIndex(colorIndex);
            main.startColor = particleColor;

            // Auto-destroy after particle lifetime
            StartCoroutine(DestroyAfterDelay(particle, config.particleLifetime + 0.5f));
        }
    }

    Color GetColorFromIndex(int colorIndex)
    {
        // Map color indices to actual colors
        Color[] colors = { Color.red, Color.blue, Color.green, Color.yellow, Color.magenta };
        return colorIndex < colors.Length ? colors[colorIndex] : Color.white;
    }

    IEnumerator DestroyAfterDelay(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (obj != null)
            Destroy(obj);
    }
}
