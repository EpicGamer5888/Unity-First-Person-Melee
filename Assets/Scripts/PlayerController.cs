using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    PlayerInput playerInput;
    PlayerInput.MainActions input;
    public Transform player;

    CharacterController controller;
    Animator animator;
    AudioSource audioSource;

    [Header("Controls")]
    public float sprintspeed;
    public float jogSpeed;
    public float currentSpeed = 12f;
    public float gravity = -9.81f;
    public float jumpHeight = 3f;
    public float defualtFOV;
    public float sprintFOV;
    public float FOVChangeSpeed;
    public bool isSprinting;

    [Header("Crouch")]
    public float playerWidth;
    public float currentPlayerHeight;
    public float deflautPlayerHeight;
    public float crouchPlayerHeight;
    public float crouchSpeed;
    public bool isCrouching;

    [Header("KeyCodes")]
    public KeyCode sprintKeyCode;
    public KeyCode crouchKeyCode;
    public KeyCode otherCrouchKeyCode;

    [Header("Player Stats")]
    public bool takenDamage = false;
    public float damageCoolDown = 0.5f;
    public int maxHealth = 5;
    public int health = 10;
    public int healthPickUp = 5; // For future referance

    Vector3 velocity;



    Vector3 _PlayerVelocity;

    bool isGrounded;

    [Header("Camera")]
    public Camera cam;
    public float sensitivity;

    float xRotation = 0f;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
        audioSource = GetComponent<AudioSource>();

        deflautPlayerHeight = 1f;

        playerInput = new PlayerInput();
        input = playerInput.Main;
        AssignInputs();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        isGrounded = controller.isGrounded;

        if (Input.GetKey(sprintKeyCode))
        {
            currentSpeed = sprintspeed;
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, sprintFOV, FOVChangeSpeed * Time.deltaTime);
        }
        else
        {
            currentSpeed = jogSpeed;
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, defualtFOV, FOVChangeSpeed * Time.deltaTime);
        }

        if (Input.GetKey(crouchKeyCode) || Input.GetKey(otherCrouchKeyCode) && isGrounded == true)
        {
            currentPlayerHeight = Mathf.Lerp(currentPlayerHeight, crouchPlayerHeight, crouchSpeed * Time.deltaTime);
            player.localScale = new Vector3(playerWidth, currentPlayerHeight, playerWidth);
            isCrouching = true;
        }
        else
        {
            currentPlayerHeight = deflautPlayerHeight;
            player.localScale = new Vector3(playerWidth, currentPlayerHeight, playerWidth);
            isCrouching = false;
        }
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float x = Input.GetAxis("Horizontal") * currentSpeed;
        float z = Input.GetAxis("Vertical") * currentSpeed;

        Vector3 move = transform.right * x + transform.forward * z;

        controller.Move(move * currentSpeed * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && isGrounded && isCrouching == false)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }


        // Repeat Inputs
        if (input.Attack.IsPressed())
        { Attack(); }

        SetAnimations();
    }

    void FixedUpdate()
    { MoveInput(input.Movement.ReadValue<Vector2>()); }

    void LateUpdate() => LookInput(input.Look.ReadValue<Vector2>());

    void MoveInput(Vector2 input)
    {
        Vector3 moveDirection = Vector3.zero;
        moveDirection.x = input.x;
        moveDirection.z = input.y;

        controller.Move(transform.TransformDirection(moveDirection) * currentSpeed * Time.deltaTime);
        _PlayerVelocity.y += gravity * Time.deltaTime;
        if (isGrounded && _PlayerVelocity.y < 0)
            _PlayerVelocity.y = -2f;
        controller.Move(_PlayerVelocity * Time.deltaTime);
    }

    void LookInput(Vector3 input)
    {
        float mouseX = input.x;
        float mouseY = input.y;

        xRotation -= (mouseY * Time.deltaTime * sensitivity);
        xRotation = Mathf.Clamp(xRotation, -80, 80);

        cam.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);

        transform.Rotate(Vector3.up * (mouseX * Time.deltaTime * sensitivity));
    }

    void OnEnable()
    { input.Enable(); }

    void OnDisable()
    { input.Disable(); }



    void AssignInputs()
    {
        input.Attack.started += ctx => Attack();
    }

    // ---------- //
    // ANIMATIONS //
    // ---------- //

    public const string IDLE = "Idle";
    public const string WALK = "Walk";
    public const string ATTACK1 = "Attack 1";
    public const string ATTACK2 = "Attack 2";

    string currentAnimationState;

    public void ChangeAnimationState(string newState)
    {
        // STOP THE SAME ANIMATION FROM INTERRUPTING WITH ITSELF //
        if (currentAnimationState == newState) return;

        // PLAY THE ANIMATION //
        currentAnimationState = newState;
        animator.CrossFadeInFixedTime(currentAnimationState, 0.2f);
    }

    void SetAnimations()
    {
        // If player is not attacking
        if (!attacking)
        {
            if (_PlayerVelocity.x == 0 && _PlayerVelocity.z == 0)
            { ChangeAnimationState(IDLE); }
            else
            { ChangeAnimationState(WALK); }
        }
    }

    // ------------------- //
    // ATTACKING BEHAVIOUR //
    // ------------------- //

    [Header("Attacking")]
    public float attackDistance = 3f;
    public float attackDelay = 0.4f;
    public float attackSpeed = 1f;
    public int attackDamage = 1;
    public LayerMask attackLayer;

    public GameObject hitEffect;
    public AudioClip swordSwing;
    public AudioClip hitSound;

    bool attacking = false;
    bool readyToAttack = true;
    int attackCount;

    public void Attack()
    {
        if (!readyToAttack || attacking) return;

        readyToAttack = false;
        attacking = true;

        Invoke(nameof(ResetAttack), attackSpeed);
        Invoke(nameof(AttackRaycast), attackDelay);

        audioSource.pitch = Random.Range(0.9f, 1.1f);
        audioSource.PlayOneShot(swordSwing);

        if (attackCount == 0)
        {
            ChangeAnimationState(ATTACK1);
            attackCount++;
        }
        else
        {
            ChangeAnimationState(ATTACK2);
            attackCount = 0;
        }
    }

    void ResetAttack()
    {
        attacking = false;
        readyToAttack = true;
    }

    void AttackRaycast()
    {
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, attackDistance, attackLayer))
        {
            HitTarget(hit.point);

            if (hit.transform.TryGetComponent<Actor>(out Actor T))
            { T.TakeDamage(attackDamage); }
        }
    }

    void HitTarget(Vector3 pos)
    {
        audioSource.pitch = 1;
        audioSource.PlayOneShot(hitSound);

        GameObject GO = Instantiate(hitEffect, pos, Quaternion.identity);
        Destroy(GO, 20);
    }
}