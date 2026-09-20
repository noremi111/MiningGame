using UnityEngine;
using UnityEngine.SceneManagement;

public class RunManager : MonoBehaviour
{
    public enum RunEndReason { None, SafeReturn, EnergyDepleted, TimeExpired }

    [Header("Run Timer")]
    [SerializeField] private bool useTimeLimit = true;
    [SerializeField, Min(1f)] private float runDurationSeconds = 300f;
    [SerializeField] private float remainingSeconds;
    public float RemainingSeconds => remainingSeconds;
    public bool TimeLimitEnabled => useTimeLimit;
    public RunEndReason LastEndReason { get; private set; }
    public int LastGoldKept { get; private set; }
    public int LastGoldLost { get; private set; }
    private ArtifactFoundUI activeFindUI;
    private bool returningToSurface;
    public bool ReturningToSurface => returningToSurface;
    private MiningAudio soundEffects;

    public static RunManager Instance { get; private set; }

    [SerializeField] private int bankGold = 0;
    [SerializeField] private int runGold = 0;
    [SerializeField] private bool runActive = false;
    [Range(0f, 1f)]
    [SerializeField] private float goldKeepPercentageOnFailure = 0.5f;
    [SerializeField] private string mineSceneName = "Mine";
    [SerializeField] private string surfaceSceneName = "Surface";

    [Header("Shop Status (Startwert und Live-Schalter)")]
    [Tooltip("Bestimmt den Zustand bei jedem Spielstart: an = aufgebaut, aus = Baustelle. Überschreibt gespeicherten Shop- und Toolbox-Fortschritt. Auch im Play Mode umschaltbar.")]
    [SerializeField] private bool shopBuilt;

    [Header("Shop Unlock")]
    [Range(0f, 1f)]
    [SerializeField] private float toolboxDropChance = 0.05f;
    [Min(1)]
    [SerializeField] private int guaranteedToolboxAfterBlocks = 30;
    [SerializeField] private bool keepToolboxOnFailure = false;

    [Header("Magierturm - Testschalter")]
    [SerializeField, Tooltip("An: beim Spielstart Mage Tower Present verwenden und Kupferfortschritt überschreiben. Aus: gespeicherten Fortschritt laden.")]
    private bool overrideMageTowerOnStart;
    [SerializeField, Tooltip("An = Turm vorhanden (50 Kupfer), aus = nicht vorhanden (0 Kupfer). Startwert nur mit Override Mage Tower On Start; im Play Mode direkt umschaltbar.")]
    private bool mageTowerPresent;

    [Header("Magierturm (gespeicherter Fortschritt)")]
    [SerializeField, Tooltip("Laufzeit-Anzeige. Zählt bis zum Freischalten bei 50.")]
    private int copperMinedForTower;
    public const int CopperRequiredForTower = 50;
    private const string TowerCopperKey = "MiningGame.MageTower.Copper.v1";
    public int CopperMinedForTower => copperMinedForTower;
    public bool MageTowerUnlocked => copperMinedForTower >= CopperRequiredForTower;

    // 0 = missing, 1 = secured, 2 = shop rebuilt. Run loot is never saved.
    private const string ShopStateKey = "MiningGame.ShopUnlock.State.v1";
    private const string ShopBlocksKey = "MiningGame.ShopUnlock.Blocks.v1";
    private int shopUnlockState;
    private int blocksWithoutToolbox;
    private bool toolboxInRun;

    // Notifies scene objects after a shop state change; no visual polling needed.
    public event System.Action ShopStateChanged;
    public event System.Action MageTowerStateChanged;

    public bool ShopUnlocked => shopUnlockState == 2;
    public bool HasSecuredToolbox => shopUnlockState == 1;
    public bool ToolboxInRun => toolboxInRun;

    public int BankGold => bankGold;
    public int RunGold => runGold;
    public bool RunActive => runActive;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        if (GetComponent<BuildingProgressManager>() == null)
            gameObject.AddComponent<BuildingProgressManager>();
        if (GetComponent<BuildingEffectManager>() == null)
            gameObject.AddComponent<BuildingEffectManager>();
        soundEffects = GetComponent<MiningAudio>();
        if (soundEffects == null) soundEffects = gameObject.AddComponent<MiningAudio>();
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
        // Der Inspector-Wert ist beim Start maßgeblich.
        // Nur der erste RunManager initialisiert: Szenenwechsel behalten
        // dank DontDestroyOnLoad den aktuellen Fortschritt.
        SetShopBuilt(shopBuilt);
        if (overrideMageTowerOnStart)
            SetMageTowerPresent(mageTowerPresent);
        else
        {
            copperMinedForTower = Mathf.Clamp(
                PlayerPrefs.GetInt(TowerCopperKey, 0), 0, CopperRequiredForTower);
            mageTowerPresent = MageTowerUnlocked;
        }
    }

    private void Update()
    {
        // Inspector-Änderungen auf dem Hauptthread übernehmen, auch bei Spielpause.
        if (Instance == this && shopBuilt != ShopUnlocked)
            SetShopBuilt(shopBuilt);
        if (Instance == this && mageTowerPresent != MageTowerUnlocked)
            SetMageTowerPresent(mageTowerPresent);
    }

    private void OnDestroy()
    {
        if (Instance != this) return;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        Instance = null;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == mineSceneName)
            activeFindUI = FindFirstObjectByType<ArtifactFoundUI>(FindObjectsInactive.Include);
        if (returningToSurface && scene.name == surfaceSceneName)
        {
            returningToSurface = false;
            Time.timeScale = 1f;
            if (soundEffects != null)
            {
                MiningAudio.Cue cue = LastEndReason == RunEndReason.SafeReturn
                    ? MiningAudio.Cue.SafeReturn
                    : LastEndReason == RunEndReason.TimeExpired
                        ? MiningAudio.Cue.TimeExpired : MiningAudio.Cue.EnergyDepleted;
                soundEffects.Play(cue);
            }
        }
    }

    private void LateUpdate()
    {
        // Einmal pro Frame nach Mining/Level-Up prüfen. Kein Reset beim Etagenwechsel.
        if (!runActive || !useTimeLimit || Time.timeScale <= 0f ||
            SceneManager.GetActiveScene().name != mineSceneName ||
            (activeFindUI != null && activeFindUI.HasPendingFinds)) return;

        remainingSeconds = Mathf.Max(0f, remainingSeconds - Time.deltaTime);
        if (remainingSeconds <= 0f)
            EndRun(RunEndReason.TimeExpired);
    }

    // Öffentlicher Switch für Scripts, Buttons und UnityEvents.
    public void SetMageTowerPresent(bool present)
    {
        if (!Application.isPlaying || Instance != this) return;
        copperMinedForTower = present ? CopperRequiredForTower : 0;
        SaveMageTowerProgress();
    }

    private void SaveMageTowerProgress()
    {
        mageTowerPresent = MageTowerUnlocked;
        PlayerPrefs.SetInt(TowerCopperKey, copperMinedForTower);
        PlayerPrefs.Save();
        MageTowerStateChanged?.Invoke();
    }

    // Pro vollständig zerstörtem Kupferblock einmal aufrufen.
    // true nur beim erstmaligen Erreichen von 50.
    public bool RegisterCopperMinedForTower()
    {
        if (!runActive || MageTowerUnlocked) return false;
        copperMinedForTower++;
        SaveMageTowerProgress();
        return MageTowerUnlocked;
    }

    [ContextMenu("Magierturm/Fortschritt zurücksetzen")]
    public void ResetMageTowerProgress()
    {
        if (!Application.isPlaying || Instance != this) return;
        SetMageTowerPresent(false);
    }

    public void StartNewRun()
    {
        if (runActive || returningToSurface) return;
        Time.timeScale = 1f;
        soundEffects?.StopAllSounds();
        remainingSeconds = Mathf.Max(1f, runDurationSeconds);
        LastEndReason = RunEndReason.None;
        LastGoldKept = 0;
        LastGoldLost = 0;
        runGold = 0;
        toolboxInRun = false;
        runActive = true;
        BuildingEffectManager.Instance?.BeginRun();
        SceneManager.LoadScene(mineSceneName);
    }

    public void AddRunGold(int amount)
    {
        if (runActive && amount > 0)
        {
            runGold += amount;
            soundEffects?.Play(MiningAudio.Cue.Gold);
        }
    }

    public void ReturnSafely()
    {
        EndRun(RunEndReason.SafeReturn);
    }

    public void FailRun()
    {
        EndRun(RunEndReason.EnergyDepleted);
    }

    private void EndRun(RunEndReason reason)
    {
        if (!runActive) return;
        // Vor Auszahlung sperren: gleichzeitige Endbedingungen zählen nur einmal.
        runActive = false;
        returningToSurface = true;
        BuildingProgressManager.Instance?.RegisterCompletedRun();
        soundEffects?.StopAllSounds();
        bool safe = reason == RunEndReason.SafeReturn;
        LastEndReason = reason;
        LastGoldKept = safe ? runGold :
            Mathf.FloorToInt(runGold * Mathf.Clamp01(goldKeepPercentageOnFailure));
        LastGoldLost = runGold - LastGoldKept;
        FinishToolboxRun(safe || keepToolboxOnFailure);
        bankGold += LastGoldKept;
        runGold = 0;
        Time.timeScale = 1f;
        SceneManager.LoadScene(surfaceSceneName);
    }

    // Call exactly once per destroyed stone/ore, including Seismic Pick stones.
    public bool RegisterMinedBlockForShop()
    {
        if (!runActive || shopUnlockState != 0 || toolboxInRun) return false;

        int guarantee = Mathf.Max(1, guaranteedToolboxAfterBlocks);
        if (blocksWithoutToolbox < guarantee) blocksWithoutToolbox++;
        if (blocksWithoutToolbox < guarantee &&
            Random.value >= Mathf.Clamp01(toolboxDropChance)) return false;

        toolboxInRun = true;
        blocksWithoutToolbox = 0;
        return true;
    }

    private void FinishToolboxRun(bool keepToolbox)
    {
        if (toolboxInRun && keepToolbox && shopUnlockState == 0)
            shopUnlockState = 1;
        toolboxInRun = false;
        SaveShopProgress();
    }

    public bool TryRebuildShop()
    {
        if (runActive || !HasSecuredToolbox) return false;
        shopUnlockState = 2;
        SaveShopProgress();
        return true;
    }

    // Öffentlicher Einstieg für UnityEvents, UI-Buttons oder andere Scripts.
    // false setzt auch den Werkzeugkisten-Fortschritt zurück.
    public void SetShopBuilt(bool built)
    {
        if (!Application.isPlaying || Instance != this)
        {
            Debug.LogWarning("Shop-Schalter bitte im Play Mode am aktiven RunManager verwenden.", this);
            return;
        }

        shopUnlockState = built ? 2 : 0;
        blocksWithoutToolbox = 0;
        toolboxInRun = false;
        SaveShopProgress();
    }

    [ContextMenu("Shop/Aufgebaut")]
    public void BuildShop()
    {
        SetShopBuilt(true);
    }

    [ContextMenu("Shop/Nicht aufgebaut (Fund zurücksetzen)")]
    public void UnbuildShop()
    {
        SetShopBuilt(false);
    }

    private void SaveShopProgress()
    {
        // Auch regulärer Wiederaufbau und Reset aktualisieren die Checkbox.
        shopBuilt = ShopUnlocked;
        PlayerPrefs.SetInt(ShopStateKey, shopUnlockState);
        PlayerPrefs.SetInt(ShopBlocksKey, blocksWithoutToolbox);
        PlayerPrefs.Save();
        ShopStateChanged?.Invoke();
    }

    // Call from the future New Game flow; does not reset gold or upgrades.
    [ContextMenu("Reset Shop Unlock Progress")]
    public void ResetShopUnlockProgress()
    {
        shopUnlockState = 0;
        blocksWithoutToolbox = 0;
        toolboxInRun = false;
        SaveShopProgress();
    }

    public bool CanAfford(int amount) => amount <= 0 || bankGold >= amount;

    public bool SpendBankGold(int amount)
    {
        if (amount <= 0 || !CanAfford(amount)) return false;
        bankGold -= amount;
        return true;
    }

    public void AddBankGold(int amount)
    {
        if (amount > 0) bankGold += amount;
    }
}
