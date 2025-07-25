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

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Shader.Find("Sprites/Default"));

            renderer.material.mainTexture = config.particleSprites[0].texture;
            renderer.sortingLayerName = "Default";
            renderer.sortingOrder = 10;

            particleSystemPrefab = prefab;
            particleSystemPrefab.SetActive(false);
        }
    }

    public void PlayExplosionEffect(Vector3 position, int colorIndex = 0)
    {
        if (particleSystemPrefab != null)
        {
            if (float.IsNaN(position.x) || float.IsNaN(position.y) || float.IsNaN(position.z))
            {
                Debug.LogWarning("Invalid particle position detected, skipping effect");
                return;
            }

            GameObject particle = Instantiate(particleSystemPrefab, position, Quaternion.identity);
            particle.SetActive(true);

            ParticleSystem ps = particle.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                var main = ps.main;

                Color particleColor = GetColorFromIndex(colorIndex);
                main.startColor = particleColor;

                var renderer = ps.GetComponent<ParticleSystemRenderer>();
                if (config.particleSprites.Length > 1)
                {
                    int randomSpriteIndex = Random.Range(0, config.particleSprites.Length);
                    renderer.material.mainTexture = config
                        .particleSprites[randomSpriteIndex]
                        .texture;
                }

                ps.Play();

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
        Color[] colors =
        {
            Color.yellow,
            Color.red,
            new Color(0f, 0.6f, 1f),
            Color.green,
            new Color(0.8f, 0.4f, 0.9f)
        };

        if (colorIndex >= 0 && colorIndex < colors.Length)
        {
            return colors[colorIndex];
        }

        return Color.white;
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
