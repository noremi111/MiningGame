using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Tilemaps;

// Add to the same root GameObject as RunManager. It survives scene changes with it.
[DisallowMultipleComponent]
public class MiningAudio : MonoBehaviour
{
    [Serializable]
    public class Sound
    {
        public AudioClip[] clips = new AudioClip[0];
        [Range(0f, 1f)] public float volume = 0.6f;
        [Range(0f, 0.2f)] public float pitchVariation = 0.04f;
        [Min(0f)] public float cooldown = 0.08f;
        [NonSerialized] public float nextTime;
        [NonSerialized] public AudioClip lastClip;
        [NonSerialized] public float lastDuration;
    }

    [Serializable]
    public class MaterialSounds
    {
        public string label = "Earth / Stone / Obsidian";
        public TileBase[] tiles = new TileBase[0];
        public Sound hit = new Sound();
        public Sound breaking = new Sound();
    }

    public enum Cue { Gold, Critical, LuckyCoin, Artifact, SpecialFind, StairsRevealed,
        SafeReturn, EnergyDepleted, TimeExpired, LevelUp, PerkSelect, StairsUse }

    [Header("Output")]
    [Range(0f, 1f)] public float masterVolume = 0.8f;
    [Tooltip("Optional: Audio-Mixer-Gruppe. None funktioniert ohne Mixer. Kein AudioListener zuweisen; Unity verwendet den aktiven Listener der Szene automatisch.")]
    [SerializeField] private AudioMixerGroup outputGroup;
    [Header("Mining - fallback for all tiles")]
    [SerializeField] private Sound defaultHit = new Sound();
    [SerializeField] private Sound defaultBreak = new Sound();
    [Header("Optional tile-specific sounds")]
    [SerializeField] private MaterialSounds[] materials = new MaterialSounds[0];
    [Header("Rewards")]
    [SerializeField] private Sound gold = new Sound();
    [SerializeField] private Sound critical = new Sound();
    [SerializeField] private Sound luckyCoin = new Sound();
    [SerializeField] private Sound artifact = new Sound();
    [SerializeField] private Sound specialFind = new Sound();
    [SerializeField] private Sound stairsRevealed = new Sound();
    [Header("Level, perks and stairs")]
    [SerializeField] private Sound levelUp = new Sound { pitchVariation = 0f };
    [SerializeField] private Sound perkSelect = new Sound { pitchVariation = 0f };
    [SerializeField] private Sound stairsUse = new Sound();
    [Header("Run end")]
    [SerializeField] private Sound safeReturn = new Sound();
    [SerializeField] private Sound energyDepleted = new Sound();
    [SerializeField] private Sound timeExpired = new Sound();

    // Audio randomness does not consume the gameplay RNG used for loot.
    private readonly System.Random random = new System.Random();
    private AudioSource[] sources;
    private readonly float[] baseVolumes = new float[12];
    private readonly int[] startedFrames = new int[12];
    private float nextHit, nextBreak;
    private bool stairsReturnPending;
    private readonly Queue<Sound> feedbackQueue = new Queue<Sound>();
    private Coroutine feedbackRoutine;

    public static MiningAudio Current
    {
        get { return RunManager.Instance != null ? RunManager.Instance.GetComponent<MiningAudio>() : null; }
    }

    private void EnsureSources()
    {
        if (sources != null) return;
        sources = new AudioSource[12];
        for (int i = 0; i < sources.Length; i++)
        {
            GameObject voice = new GameObject("SFX Voice " + i);
            voice.transform.SetParent(transform, false);
            sources[i] = voice.AddComponent<AudioSource>();
            sources[i].playOnAwake = false;
            sources[i].loop = false;
            sources[i].spatialBlend = 0f;
            startedFrames[i] = -1;
        }
    }

    private void Update()
    {
        if (sources == null) return;
        for (int i = 0; i < sources.Length; i++)
            sources[i].volume = baseVolumes[i] * Mathf.Clamp01(masterVolume);
    }

    public void PlayHit(TileBase tile) { PlayMaterial(tile, false); }
    public void PlayBreak(TileBase tile) { PlayMaterial(tile, true); }

    private void PlayMaterial(TileBase tile, bool breaking)
    {
        float now = Time.unscaledTime;
        if (now < (breaking ? nextBreak : nextHit)) return;
        Sound sound = breaking ? defaultBreak : defaultHit;
        if (materials != null)
        {
            foreach (MaterialSounds material in materials)
            {
                if (material == null || material.tiles == null || tile == null) continue;
                if (Array.IndexOf(material.tiles, tile) < 0) continue;
                Sound candidate = breaking ? material.breaking : material.hit;
                if (HasClips(candidate)) sound = candidate;
                break;
            }
        }
        if (PlaySound(sound, false))
        {
            // Also rate-limit across different material profiles (Seismic Pick).
            if (breaking) nextBreak = now + 0.06f;
            else nextHit = now + 0.03f;
        }
    }

    public void Play(Cue cue)
    {
        switch (cue)
        {
            case Cue.Gold: PlaySound(gold, false); break;
            case Cue.Critical: PlaySound(critical, false); break;
            case Cue.LuckyCoin: PlaySound(luckyCoin, true); break;
            case Cue.Artifact: PlaySound(artifact, true); break;
            case Cue.SpecialFind: PlaySound(specialFind, true); break;
            case Cue.StairsRevealed: PlaySound(stairsRevealed, false); break;
            case Cue.SafeReturn:
                if (stairsReturnPending)
                {
                    stairsReturnPending = false;
                    EnqueueFeedback(stairsUse);
                    EnqueueFeedback(safeReturn);
                }
                else PlaySound(safeReturn, true);
                break;
            case Cue.EnergyDepleted: PlaySound(energyDepleted, true); break;
            case Cue.TimeExpired: PlaySound(timeExpired, true); break;
            case Cue.LevelUp: EnqueueFeedback(levelUp); break;
            case Cue.PerkSelect: PlaySound(perkSelect, true); break;
            case Cue.StairsUse: PlaySound(stairsUse, false); break;
        }
    }

    // Consumed by SafeReturn after RunManager has loaded Surface.
    // The pending marker deliberately survives StopAllSounds during this transition.
    public void PrepareStairsReturn()
    {
        stairsReturnPending = true;
    }

    private void EnqueueFeedback(Sound sound)
    {
        if (!isActiveAndEnabled || !HasClips(sound)) return;
        feedbackQueue.Enqueue(sound);
        if (feedbackRoutine == null) feedbackRoutine = StartCoroutine(PlayFeedbackQueue());
    }

    private IEnumerator PlayFeedbackQueue()
    {
        // Yield first so the coroutine handle is assigned before completion.
        yield return null;
        while (feedbackQueue.Count > 0)
        {
            Sound sound = feedbackQueue.Dequeue();
            // Wait for cooldown/free voice, including during the perk UI pause.
            while (HasClips(sound) && !PlaySound(sound, true)) yield return null;
            if (HasClips(sound))
                yield return new WaitForSecondsRealtime(Mathf.Max(sound.lastDuration, sound.cooldown) + 0.04f);
        }
        feedbackRoutine = null;
    }

    private static bool HasClips(Sound sound)
    {
        if (sound == null || sound.clips == null) return false;
        foreach (AudioClip clip in sound.clips) if (clip != null) return true;
        return false;
    }

    private bool PlaySound(Sound sound, bool important)
    {
        if (!isActiveAndEnabled || !HasClips(sound) || Time.unscaledTime < sound.nextTime) return false;
        EnsureSources();
        // Ten regular voices, two reserved for finds and run-end feedback.
        int voice = -1;
        int first = important ? 10 : 0;
        int end = important ? 12 : 10;
        for (int i = first; i < end; i++)
            if (!sources[i].isPlaying && startedFrames[i] != Time.frameCount) { voice = i; break; }
        if (voice < 0 && important)
            for (int i = 0; i < 10; i++)
                if (!sources[i].isPlaying && startedFrames[i] != Time.frameCount) { voice = i; break; }
        if (voice < 0) return false;

        int count = 0;
        foreach (AudioClip clip in sound.clips)
            if (clip != null && clip != sound.lastClip) count++;
        AudioClip selected = sound.lastClip;
        if (count > 0)
        {
            int choice = random.Next(count);
            foreach (AudioClip clip in sound.clips)
                if (clip != null && clip != sound.lastClip && choice-- == 0) { selected = clip; break; }
        }
        if (selected == null) return false;
        AudioSource source = sources[voice];
        source.outputAudioMixerGroup = outputGroup;
        source.ignoreListenerPause = important;
        source.clip = selected;
        source.pitch = 1f + ((float)random.NextDouble() * 2f - 1f) * Mathf.Clamp(sound.pitchVariation, 0f, 0.2f);
        baseVolumes[voice] = Mathf.Clamp01(sound.volume);
        source.volume = baseVolumes[voice] * Mathf.Clamp01(masterVolume);
        source.Play();
        sound.lastDuration = selected.length / source.pitch;
        startedFrames[voice] = Time.frameCount;
        sound.lastClip = selected;
        sound.nextTime = Time.unscaledTime + Mathf.Max(0.01f, sound.cooldown);
        return true;
    }

    public void StopAllSounds()
    {
        if (feedbackRoutine != null) StopCoroutine(feedbackRoutine);
        feedbackRoutine = null;
        feedbackQueue.Clear();
        if (sources != null)
            for (int i = 0; i < sources.Length; i++) { sources[i].Stop(); startedFrames[i] = -1; }
        nextHit = nextBreak = 0f;
    }

    private void OnDisable() { StopAllSounds(); stairsReturnPending = false; }
}
