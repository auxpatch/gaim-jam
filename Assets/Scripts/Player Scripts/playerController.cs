using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float focusSpeed = 5f;
    [SerializeField] private float attackCooldown = 0.1f;

    [Header("Bounds")]
    [SerializeField] private Vector2 boundsPadding = new Vector2(0.4f, 0.4f); // roughly half your sprite size

    private Camera cam;

    private float attackTimer = 0f;
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
        cam = Camera.main; // cache once, merge into your existing Awake
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
        HandleTimers();

    }

    private Vector2 ClampToCamera(Vector2 p)
    {
        cam = Camera.main;
        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;   
        Vector2 c = cam.transform.position;

        p.x = Mathf.Clamp(p.x, c.x - halfW + boundsPadding.x, c.x + halfW - boundsPadding.x);
        p.y = Mathf.Clamp(p.y, c.y - halfH + boundsPadding.y, c.y + halfH - boundsPadding.y);
        return p;
    }

    private void HandleShoot()
    {
        if (inputHandler.shootInput && attackTimer <= 0 )
        {
            attackTimer = attackCooldown;
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

    private void HandleTimers()
    {
        attackTimer -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
        Vector2 next = rb.position + currentMovement * (inputHandler.focusInput ? focusSpeed : moveSpeed) * Time.fixedDeltaTime;
        rb.MovePosition(ClampToCamera(next));
    }

}