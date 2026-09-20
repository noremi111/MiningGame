using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

// Attach to a separate root GameObject in Surface (and optionally Mine
// for direct editor testing). Assign both clips before entering Play Mode.
[DisallowMultipleComponent]
public sealed class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("Music Clips")]
    [SerializeField] private AudioClip surfaceMusic;
    [SerializeField] private AudioClip mineMusic;

    [Header("Scene Names - match RunManager")]
    [SerializeField] private string surfaceSceneName = "Surface";
    [SerializeField] private string mineSceneName = "Mine";

    [Header("Playback")]
    [SerializeField, Range(0f, 1f)] private float musicVolume = 0.3f;
    [SerializeField, Min(0f)] private float crossfadeSeconds = 1.5f;
    [SerializeField, Tooltip("Optional. Leave empty to play directly.")]
    private AudioMixerGroup outputMixerGroup;

    private readonly AudioSource[] sources = new AudioSource[2];
    private readonly float[] gains = new float[2];
    private readonly float[] startGains = new float[2];
    private int targetIndex = -1;
    private float fadeElapsed;
    private float fadeDuration;
    private bool fading;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
        for (int i = 0; i < sources.Length; i++)
        {
            GameObject child = new GameObject("Music Source " + (i + 1));
            child.transform.SetParent(transform, false);
            AudioSource source = child.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = true;
            source.spatialBlend = 0f;
            source.volume = 0f;
            source.outputAudioMixerGroup = outputMixerGroup;
            sources[i] = source;
        }
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        if (Instance == this)
            PlayForScene(SceneManager.GetActiveScene().name);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Additive scenery/UI scenes should not switch the soundtrack.
        if (mode == LoadSceneMode.Single)
            PlayForScene(scene.name);
    }

    private void PlayForScene(string sceneName)
    {
        if (sceneName == surfaceSceneName) PlayMusic(surfaceMusic);
        else if (sceneName == mineSceneName) PlayMusic(mineMusic);
        else PlayMusic(null);
    }

    public void PlayMusic(AudioClip clip)
    {
        // Re-entering the same scene/floor must not restart the track.
        if (targetIndex >= 0 && sources[targetIndex].clip == clip) return;
        if (targetIndex < 0 && clip == null) return;

        int next = -1;
        if (clip != null)
        {
            // Reuse a still-playing outgoing track if a transition is reversed.
            for (int i = 0; i < sources.Length; i++)
                if (sources[i].clip == clip && sources[i].isPlaying) next = i;

            if (next < 0)
            {
                next = targetIndex == 0 ? 1 : 0;
                sources[next].Stop();
                sources[next].clip = clip;
                gains[next] = 0f;
                sources[next].volume = 0f;
                sources[next].Play();
            }
        }

        for (int i = 0; i < gains.Length; i++) startGains[i] = gains[i];
        targetIndex = next;
        fadeElapsed = 0f;
        fadeDuration = Mathf.Max(0f, crossfadeSeconds);
        fading = true;
    }

    private void Update()
    {
        if (Instance != this) return;
        if (fading)
        {
            fadeElapsed += Time.unscaledDeltaTime;
            float t = fadeDuration <= 0f ? 1f : Mathf.Clamp01(fadeElapsed / fadeDuration);
            for (int i = 0; i < gains.Length; i++)
                gains[i] = Mathf.Lerp(startGains[i], i == targetIndex ? 1f : 0f, t);
            if (t >= 1f)
            {
                fading = false;
                for (int i = 0; i < sources.Length; i++)
                {
                    if (i == targetIndex) continue;
                    sources[i].Stop();
                    sources[i].clip = null;
                }
            }
        }

        for (int i = 0; i < sources.Length; i++)
            sources[i].volume = gains[i] * Mathf.Clamp01(musicVolume);
    }

    // Can be connected to a UI Slider (0 to 1).
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
    }

    private void OnDestroy()
    {
        if (Instance != this) return;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        Instance = null;
    }
}
