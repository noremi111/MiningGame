using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Player Visual")]
    [Tooltip("Assign the SpriteRenderer that displays the miner.")]
    [SerializeField] private SpriteRenderer playerSpriteRenderer;

    [Header("Idle Sprites - 8 Directions")]
    [SerializeField] private Sprite idleDown;
    [SerializeField] private Sprite idleDownLeft;
    [SerializeField] private Sprite idleLeft;
    [SerializeField] private Sprite idleUpLeft;
    [SerializeField] private Sprite idleUp;
    [SerializeField] private Sprite idleUpRight;
    [SerializeField] private Sprite idleRight;
    [SerializeField] private Sprite idleDownRight;

    [Header("Walk Animation")]
    [Min(1f)]
    [SerializeField] private float walkFramesPerSecond = 8f;
    [Tooltip("Movement speed at which the animation plays at the configured FPS.")]
    [Min(0.01f)]
    [SerializeField] private float animationReferenceSpeed = 5f;

    [Header("Walk Sprites - Assign Frames 01 to 06 in Order")]
    [SerializeField] private Sprite[] walkDown = new Sprite[6];
    [SerializeField] private Sprite[] walkDownLeft = new Sprite[6];
    [SerializeField] private Sprite[] walkLeft = new Sprite[6];
    [SerializeField] private Sprite[] walkUpLeft = new Sprite[6];
    [SerializeField] private Sprite[] walkUp = new Sprite[6];
    [SerializeField] private Sprite[] walkUpRight = new Sprite[6];
    [SerializeField] private Sprite[] walkRight = new Sprite[6];
    [SerializeField] private Sprite[] walkDownRight = new Sprite[6];

    // Shared cycle phase keeps direction changes from restarting every step.
    private float walkCycle;
    private bool wasWalking;

    private bool IsWalking => moveInput.sqrMagnitude > 0f && moveSpeed > 0f;

    private Vector2 lastLookDirection = Vector2.down;
    private Rigidbody2D rb;
    private Vector2 moveInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (playerSpriteRenderer == null)
            playerSpriteRenderer = GetComponent<SpriteRenderer>();

        if (playerSpriteRenderer == null)
            Debug.LogWarning("PlayerMovement: Assign the miner SpriteRenderer in the Inspector.", this);

        UpdateFacingSprite();
        ApplyPermanentUpgrades();
    }

    private void Update()
    {
        // Keep the current pose while the game is paused.
        if (Time.timeScale == 0f)
        {
            moveInput = Vector2.zero;
            return;
        }

        ReadInput();

        if (moveInput.sqrMagnitude > 0f)
        {
            lastLookDirection = moveInput;
        }

        if (IsWalking)
        {
            Sprite[] frames = GetWalkFrames();
            if (wasWalking && frames != null && frames.Length > 0)
            {
                float speedRatio = moveSpeed / Mathf.Max(0.01f, animationReferenceSpeed);
                walkCycle = Mathf.Repeat(walkCycle + Time.deltaTime
                    * Mathf.Max(1f, walkFramesPerSecond) * speedRatio / frames.Length, 1f);
            }
        }
        else
        {
            walkCycle = 0f;
        }

        UpdateFacingSprite();
        wasWalking = IsWalking;
    }

    private void FixedUpdate()
    {
        rb.linearVelocity =
            moveInput * moveSpeed;
    }

    private void ReadInput()
    {
        if (Keyboard.current == null)
        {
            moveInput = Vector2.zero;
            return;
        }

        Vector2 input = Vector2.zero;

        if (Keyboard.current.wKey.isPressed)
            input.y += 1f;

        if (Keyboard.current.sKey.isPressed)
            input.y -= 1f;

        if (Keyboard.current.aKey.isPressed)
            input.x -= 1f;

        if (Keyboard.current.dKey.isPressed)
            input.x += 1f;

        moveInput = input.normalized;
    }

    private void UpdateFacingSprite()
    {
        if (playerSpriteRenderer == null)
            return;

        Sprite nextSprite;

        if (lastLookDirection.y > 0f)
        {
            nextSprite = lastLookDirection.x < 0f ? idleUpLeft
                : lastLookDirection.x > 0f ? idleUpRight : idleUp;
        }
        else if (lastLookDirection.y < 0f)
        {
            nextSprite = lastLookDirection.x < 0f ? idleDownLeft
                : lastLookDirection.x > 0f ? idleDownRight : idleDown;
        }
        else
        {
            nextSprite = lastLookDirection.x < 0f ? idleLeft : idleRight;
        }

        if (IsWalking)
        {
            Sprite[] frames = GetWalkFrames();
            if (frames != null && frames.Length > 0)
            {
                int index = Mathf.Min(Mathf.FloorToInt(walkCycle * frames.Length), frames.Length - 1);
                if (frames[index] != null)
                    nextSprite = frames[index];
            }
        }

        // Missing assignments fall back to the idle sprite or current image.
        if (nextSprite != null)
        {
            playerSpriteRenderer.sprite = nextSprite;
            playerSpriteRenderer.flipX = false;
            playerSpriteRenderer.flipY = false;
        }
    }

    private Sprite[] GetWalkFrames()
    {
        if (lastLookDirection.y > 0f)
            return lastLookDirection.x < 0f ? walkUpLeft
                : lastLookDirection.x > 0f ? walkUpRight : walkUp;

        if (lastLookDirection.y < 0f)
            return lastLookDirection.x < 0f ? walkDownLeft
                : lastLookDirection.x > 0f ? walkDownRight : walkDown;

        return lastLookDirection.x < 0f ? walkLeft : walkRight;
    }

    // Run-Perk: Bewegungsgeschwindigkeit erhöhen
    public void AddMoveSpeed(float amount)
    {
        moveSpeed += amount;
    }

    private void ApplyPermanentUpgrades()
    {
        if (PermanentUpgradeManager.Instance == null)
            return;

        moveSpeed *=
            PermanentUpgradeManager.Instance
                .GetMoveSpeedMultiplier();
    }

    private void OnDisable()
    {
        moveInput = Vector2.zero;
        walkCycle = 0f;
        wasWalking = false;
        UpdateFacingSprite();

        if (rb != null)
        {
            rb.linearVelocity =
                Vector2.zero;
        }
    }
}
