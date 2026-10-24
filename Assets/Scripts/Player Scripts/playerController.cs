using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float focusSpeed = 5f;

    private InputHandler inputHandler;
    private PlayerShooter playerShooter;

    private Rigidbody2D rb;
    private Vector2 currentMovement;
    private float verticalVelocity;    

    private void Awake()
    {
        // Lock and hide cursor for standard first-person gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Start()
    {
        // InputHandler is a singleton on its own object, grab it here so its Awake has already run
        inputHandler = InputHandler.Instance;
        rb = GetComponent<Rigidbody2D>();
        playerShooter = GetComponent<PlayerShooter>();
    }

    private void Update()
    {
        if (inputHandler == null) return;

        HandleMovement();
        HandleShoot();

    }

    private void HandleShoot()
    {
        if (inputHandler.shootInput)
        {
            playerShooter.Shoot();
        }
    }

    private void HandleMovement()
    {
        currentMovement.x = inputHandler.MoveInput.x;
        currentMovement.y = inputHandler.MoveInput.y;

        //Normalize diagonal speed so it isn't faster than normal speed
        if(currentMovement.magnitude > 1){
            currentMovement.Normalize();
        }
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + currentMovement * ( inputHandler.focusInput ? focusSpeed : moveSpeed) * Time.fixedDeltaTime);
    }

}