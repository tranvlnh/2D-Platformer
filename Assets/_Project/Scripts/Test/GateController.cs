using UnityEngine;
using UnityEngine.InputSystem;

public class GateController : MonoBehaviour
{
    [Header("Animation")] [SerializeField] private float openDistance = 1.25f;

    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private bool startOpen;
    private Vector3[] closedPositions;
    private bool initialized;
    private Vector3[] openPositions;
    private float openProgress;
    private Transform[] parts;
    private float targetProgress;

    private InputAction toggleAction;

    private void Awake()
    {
        Initialize();
    }

    private void Start()
    {
        Initialize();
    }

    private void Update()
    {
        if (!initialized) return;

        openProgress = Mathf.MoveTowards(
            openProgress,
            targetProgress,
            moveSpeed * Time.deltaTime);

        ApplyPositions();
    }

    private void OnEnable()
    {
        toggleAction ??= new InputAction(
            "Toggle Gate",
            InputActionType.Button,
            "<Keyboard>/space");

        toggleAction.performed += OnTogglePerformed;
        toggleAction.Enable();
    }

    private void OnDisable()
    {
        if (toggleAction == null) return;

        toggleAction.performed -= OnTogglePerformed;
        toggleAction.Disable();
    }

    public void Toggle()
    {
        Initialize();
        targetProgress = targetProgress > 0.5f ? 0f : 1f;
    }

    public void Open()
    {
        Initialize();
        targetProgress = 1f;
    }

    public void Close()
    {
        Initialize();
        targetProgress = 0f;
    }

    private void OnTogglePerformed(InputAction.CallbackContext context)
    {
        Toggle();
    }

    private void Initialize()
    {
        if (initialized) return;

        parts = new[]
        {
            transform.Find("TopLeft"),
            transform.Find("TopRight"),
            transform.Find("BottomLeft"),
            transform.Find("BottomRight")
        };

        if (HasMissingPart())
        {
            Debug.LogError(
                "GateController requires TopLeft, TopRight, BottomLeft and BottomRight child objects.",
                this);
            return;
        }

        closedPositions = new Vector3[parts.Length];
        openPositions = new Vector3[parts.Length];

        for (var i = 0; i < parts.Length; i++)
        {
            closedPositions[i] = parts[i].localPosition;

            var direction = new Vector3(
                Mathf.Sign(closedPositions[i].x),
                Mathf.Sign(closedPositions[i].y),
                0f);

            openPositions[i] = closedPositions[i] + direction * openDistance;
        }

        openProgress = startOpen ? 1f : 0f;
        targetProgress = openProgress;
        initialized = true;
        ApplyPositions();
    }

    private bool HasMissingPart()
    {
        for (var i = 0; i < parts.Length; i++)
            if (parts[i] == null)
                return true;

        return false;
    }

    private void ApplyPositions()
    {
        for (var i = 0; i < parts.Length; i++)
            parts[i].localPosition = Vector3.Lerp(
                closedPositions[i],
                openPositions[i],
                openProgress);
    }
}