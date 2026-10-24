using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{

    [SerializeField] public InputActionAsset playerControls;
    [SerializeField] public string actionMapName = "Player Movement";

    [Header("Input Names")]
    [SerializeField] private string movement = "Move";
    [SerializeField] private string shoot = "Shooting";
    [SerializeField] private string focus = "Focus";
    [SerializeField] private string escape = "ESC";


    [Header("Input Actions")]
    public InputAction moveAction { get; private set; }
    public InputAction shootAction { get; private set; }
    public InputAction focusAction { get; private set; }
    public InputAction escapeAction { get; private set; }

    /*
    [Header("Input Action References")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference lookAction;
    [SerializeField] private InputActionReference sprintAction;
    [SerializeField] private InputActionReference jumpAction;
    [SerializeField] private InputActionReference clickAction;
    */

    public Vector2 MoveInput { get; private set; }
    public bool shootInput { get; private set; }
    public bool focusInput { get; private set; }
    public bool EscapeTriggered { get; private set; }

    public static InputHandler Instance { get; private set; }

    private void Awake()
    {
        //LB: Handles instance related stuff, still unsure where and when the destroy code runs? It never seems necessary
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        RegisterActionMapping();
        RegisterInputActions();
    }


    private void RegisterActionMapping()
    {
        InputActionMap map = playerControls.FindActionMap(actionMapName);

        moveAction = map.FindAction(movement);
        shootAction = map.FindAction(shoot);
        focusAction = map.FindAction(focus);
        escapeAction = map.FindAction(escape);
    }

    private void RegisterInputActions()
    {
        moveAction.performed += context => MoveInput = context.ReadValue<Vector2>();
        moveAction.canceled += context => MoveInput = Vector2.zero;

        shootAction.performed += context => shootInput = true;
        shootAction.canceled += context => shootInput = false;

        focusAction.performed += context => focusInput = true;
        focusAction.canceled += context => focusInput = false;

        escapeAction.performed += context => EscapeTriggered = true;
        escapeAction.canceled += context => EscapeTriggered = false;
    }

    private void OnEnable()
    {
        moveAction.Enable();
        shootAction.Enable();
        focusAction.Enable();
        escapeAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
        shootAction.Disable();
        focusAction.Disable();
        escapeAction.Disable();
    }
}