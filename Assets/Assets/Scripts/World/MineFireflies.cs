using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// Add to the MineGenerator GameObject. No prefab or imported sprite is needed.
[DisallowMultipleComponent]
public class MineFireflies : MonoBehaviour
{
    [Header("Appearance")]
    [Tooltip("Assign a material using Universal Render Pipeline/2D/Sprite-Unlit-Default.")]
    [SerializeField] private Material glowMaterial;
    [SerializeField] private Color glowColor = new Color(0.85f, 1f, 0.3f, 1f);
    [Min(0.01f)] [SerializeField] private float glowDiameter = 0.24f;
    [SerializeField] private string sortingLayerName = "Default";
    [SerializeField] private int sortingOrder = 8;

    [Header("Distribution")]
    [SerializeField] private bool spawnFireflies = true;
    [Range(0, 100)] [SerializeField] private int maxFireflies = 24;
    [Range(0f, 1f)] [SerializeField] private float firefliesPerGrassTile = 0.15f;
    [Min(0f)] [SerializeField] private float minimumSpacing = 0.8f;

    [Header("Movement and Pulse")]
    [Min(0f)] [SerializeField] private float flightRadius = 0.3f;
    [Min(0f)] [SerializeField] private float flightSpeed = 0.65f;
    [Min(0f)] [SerializeField] private float pulseSpeed = 1.8f;
    [Range(0f, 1f)] [SerializeField] private float minimumBrightness = 0.15f;

    private class Firefly
    {
        public SpriteRenderer renderer;
        public Vector3 origin;
        public float phase;
        public float speed;
        public float size;
    }

    private readonly List<Firefly> flies = new List<Firefly>();
    private GameObject root;
    private Texture2D glowTexture;
    private Sprite glowSprite;
    private Tilemap ground;
    private Tilemap background;
    private float elapsed;

    public void ClearFireflies()
    {
        flies.Clear();
        elapsed = 0f;
        if (root != null)
        {
            root.SetActive(false);
            Destroy(root);
            root = null;
        }
    }

    public void Rebuild(Tilemap grass, Tilemap solidGround, Tilemap caveFloor, int floorSeed)
    {
        ClearFireflies();
        ground = solidGround;
        background = caveFloor;
        if (!isActiveAndEnabled || !spawnFireflies || maxFireflies <= 0 ||
            firefliesPerGrassTile <= 0f || grass == null || ground == null || background == null)
            return;
        if (glowMaterial == null)
        {
            Debug.LogWarning("MineFireflies: Assign an unlit sprite material to Glow Material.", this);
            return;
        }

        var candidates = new List<Vector3>();
        foreach (Vector3Int cell in grass.cellBounds.allPositionsWithin)
        {
            if (!grass.HasTile(cell)) continue;
            Vector3 position = grass.GetCellCenterWorld(cell);
            if (IsOpen(position)) candidates.Add(position);
        }
        if (candidates.Count == 0) return;

        // Independent RNG: decoration does not consume the mining/gameplay random stream.
        var rng = new System.Random(unchecked(floorSeed ^ 0x51F1E));
        for (int i = candidates.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            Vector3 swap = candidates[i];
            candidates[i] = candidates[j];
            candidates[j] = swap;
        }
        int target = Mathf.Min(Mathf.Clamp(maxFireflies, 0, 100),
            Mathf.CeilToInt(candidates.Count * Mathf.Clamp01(firefliesPerGrassTile)));
        EnsureSprite();
        root = new GameObject("Fireflies");
        root.transform.SetParent(transform, false);
        float spacingSquared = minimumSpacing * minimumSpacing;
        foreach (Vector3 position in candidates)
        {
            bool tooClose = false;
            foreach (Firefly existing in flies)
                if ((existing.origin - position).sqrMagnitude < spacingSquared)
                {
                    tooClose = true;
                    break;
                }
            if (tooClose) continue;

            var item = new GameObject("Firefly_" + (flies.Count + 1));
            item.transform.SetParent(root.transform, false);
            item.transform.position = position;
            var sr = item.AddComponent<SpriteRenderer>();
            sr.sprite = glowSprite;
            sr.sharedMaterial = glowMaterial;
            sr.sortingLayerName = sortingLayerName;
            sr.sortingOrder = sortingOrder;
            flies.Add(new Firefly
            {
                renderer = sr,
                origin = position,
                phase = (float)rng.NextDouble() * Mathf.PI * 2f,
                speed = Mathf.Lerp(0.7f, 1.3f, (float)rng.NextDouble()),
                size = Mathf.Lerp(0.8f, 1.2f, (float)rng.NextDouble())
            });
            if (flies.Count >= target) break;
        }
        UpdateVisuals();
    }

    private bool IsOpen(Vector3 position)
    {
        return ground != null && background != null &&
            !ground.HasTile(ground.WorldToCell(position)) &&
            background.HasTile(background.WorldToCell(position));
    }

    private void Update()
    {
        if (flies.Count == 0) return;
        elapsed += Time.deltaTime; // Follows normal game pause via Time.timeScale.
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        foreach (Firefly fly in flies)
        {
            if (fly.renderer == null) continue;
            float t = elapsed * Mathf.Max(0f, flightSpeed) * fly.speed;
            // Smooth bounded wandering; hold the previous position if a wall blocks the target.
            Vector3 offset = new Vector3(
                Mathf.Sin(t + fly.phase),
                Mathf.Sin(t * 0.73f + fly.phase * 1.7f), 0f);
            Vector3 target = fly.origin + offset * (Mathf.Max(0f, flightRadius) * 0.7071f);
            if (IsOpen(target)) fly.renderer.transform.position = target;
            float wave = 0.5f + 0.5f * Mathf.Sin(
                elapsed * Mathf.Max(0f, pulseSpeed) * fly.speed + fly.phase);
            Color color = glowColor;
            color.a *= Mathf.Lerp(Mathf.Clamp01(minimumBrightness), 1f, wave * wave);
            fly.renderer.color = color;
            fly.renderer.transform.localScale = Vector3.one *
                (Mathf.Max(0.01f, glowDiameter) * fly.size * Mathf.Lerp(0.9f, 1.05f, wave));
        }
    }

    private void EnsureSprite()
    {
        if (glowSprite != null) return;
        const int size = 64;
        glowTexture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        glowTexture.name = "FireflyGlow_Runtime";
        glowTexture.wrapMode = TextureWrapMode.Clamp;
        glowTexture.filterMode = FilterMode.Bilinear;
        var pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dx = (x + 0.5f - size * 0.5f) / (size * 0.5f);
                float dy = (y + 0.5f - size * 0.5f) / (size * 0.5f);
                float r = Mathf.Sqrt(dx * dx + dy * dy);
                // SmoothStep interpolates output values; normalize the radius first.
                float core = 1f - Mathf.SmoothStep(0f, 1f,
                    Mathf.InverseLerp(0f, 0.24f, r));
                float halo = 0.28f * Mathf.Exp(-5f * r * r) *
                    (1f - Mathf.SmoothStep(0f, 1f,
                        Mathf.InverseLerp(0.7f, 0.95f, r)));
                pixels[y * size + x] = new Color(1f, 1f, 1f, Mathf.Clamp01(core + halo));
            }
        glowTexture.SetPixels(pixels);
        glowTexture.Apply(false, true);
        glowSprite = Sprite.Create(glowTexture, new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f), size, 0, SpriteMeshType.FullRect);
        glowSprite.name = "FireflyGlow_Runtime";
    }

    private void OnDisable()
    {
        if (root != null) root.SetActive(false);
    }

    private void OnEnable()
    {
        if (root != null) root.SetActive(true);
    }

    private void OnDestroy()
    {
        ClearFireflies();
        if (glowSprite != null) Destroy(glowSprite);
        if (glowTexture != null) Destroy(glowTexture);
    }
}
