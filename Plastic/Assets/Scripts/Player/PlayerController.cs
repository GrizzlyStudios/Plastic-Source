using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour {

    [Header("== Static Variables ==")]

    [Header("- Settings -")]

    [Header("Movement Speeds")]
    //base speeds
    public float movementSpeed;//grounded movement speed before multipliers and additions (ex. sprinting)    
    public float sprintSpeed;//movement speed at max sprint    
    public float sprintUpSpeed;//speed at which you accelerate to sprint speed
    public float airMovementSpeed;//ungrounded movement speed    
    //max speeds
    public float maxSpeed;//max speed all movement (except wallrun)

    [Header("Sliding")]
    public float slideForce;//force added when sliding with all sliding requirements met.
    public float slideWaitTime;//the amount of time that must go by after sliding to be able to slide again
    public float slideSlopeForce;//down force applied on slopes to make sliding on slopes feel like sliding on butter
    public float crouchSpeed;
    //y change for different actions
    public float crouchY;
    public float normalY;

    //collider scales for different actions
    public float crouchScale;
    public float playerScale;

    //cam positions for different actions
    public Vector3 crouchCamPos;
    public Vector3 standCamPos;

    [Header("Friction")]
    public float movingFriction;//friction while moving and grounded
    public float standingFriction;//friction while standing and grounded
    public float airFriction;//ungrounded friction
    public float slopeSlidingFriction;//friction while sliding on slopes
    public float slidingFriction;//friction while sliding on flat surfaces
    public float wallFriction;//friction while wallrunning

    [Header("Rotation")]
    public float rotationSensitivity;
    public float rotationBounds;//limits the amount of degrees the player can rotate their head. (note: the negative of this value will also be applied)

    [Header("Jumping")]
    //grounded jumping
    public float jumpForce;
    public float jumpCooldown;
    //ungrounded jumping
    public float airJumpForce;//up force
    public int airJumps;//amount of times the player can jump mid air before landing
    public float airJumpForwardForce;

    [Header("- Camera -")]
    [Header("FOV")]
    public float cameraFOV;
    public float sprintingFOVAdd;//amount of FOV added when sprinting    
    public float sprintingFOVSpeed;//speed at which the FOV increases to sprinting FOV

    [Header("- Dependencies -")]
    [Header("Input System")]
    public PlayerInput playerInput;
    public InputAction moveInput;
    public InputAction lookInput;
    public InputAction sprintInput;
    public InputAction jumpInput;
    public InputAction crouchInput;

    [Header("Pause Menu")]
    [ReadOnly]
    public PauseMenu pause;
    [ReadOnly]
    public Slider sensitivitySlider;
    [ReadOnly]
    public Slider FOVSlider;
    [ReadOnly]
    public Toggle HoldToCrouch;

    [Header("Physics Materials")]
    //physics materials for different actions like sliding and running etc.
    public PhysicMaterial NoFriction; //no friction for sliding etc.
    public PhysicMaterial Player; //standard friction for walking, sprinting etc.
    public PhysicMaterial StandingFriction;

    [Header("Animation")]
    //animation
    public Animator animator;
    public GameObject Avatar;
    //animator parameters
    private int animIDSpeed;
    private int animIDGrounded;
    private int animIDCrouching;

    [Header("Basic Components")]
    public CapsuleCollider PlayerCollider;
    public Transform cameraHolder;
    public Transform cameraPosition;
    public Camera cam;
    public Rigidbody rb;
    public Transform orientation;

    [Header("Scripts")]
    public WallRun wallrun;
    public Recoil recoil;

    [Header("Layers")]
    public LayerMask groundLayer;

    [Header("== Dynamic Variables ==")]
    [Header("Slope and Ground")]
    RaycastHit _slopeHit;
    RaycastHit _HitGround;
    public bool _grounded;
    public float _groundedTime;
    public Vector3 _slopeMoveDirection;
    public float _ycurrent;//current amount being added or subtracted to the players y to crouch or stand up

    [Header("Jumping")]
    public bool _canJump;
    public int _airJumpsleft;

    [Header("Speed and Sprint")]
    public float _speedAddition;
    public float _speed;
    public float _sprintLevel;

    [Header("Actions")]
    public bool _walling;
    public bool _walking;
    public bool _crouching;
    public bool _sliding;
    public bool _sprinting;

    [Header("Sliding")]
    public bool _slideReset;
    public bool _pressingSlide;
    public bool _canSlide;
    public float _initialSlideSpeed;//Sprint level before sliding, because sprint level is set to zero before slide friction is calculated.

    [Header("Camera")]
    public float _fieldOfView;
    //player rotation before bounds
    public float _xRotation;
    public float _yRotation;
    //player rotation after bounds
    public float _xRotationFinal;
    public float _yRotationFinal;

    private bool checkit;

    public bool _OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out _slopeHit, PlayerCollider.height / 2 + 0.5f))
        {
            if (_slopeHit.normal != Vector3.up)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        return false;
    }

    private void Awake()
    {
        //sets scale and jump cool down parameters to defualt
        _groundedTime = jumpCooldown;

        //set inputs
        moveInput = playerInput.actions["Move"];
        lookInput = playerInput.actions["Look"];
        sprintInput = playerInput.actions["Sprint"];
        jumpInput = playerInput.actions["Jump"];
        crouchInput = playerInput.actions["Crouch"];

        //sets FOV
        _fieldOfView = cameraFOV;
        
        //set animation IDs
        animIDSpeed = Animator.StringToHash("Speed");
        animIDGrounded = Animator.StringToHash("Grounded");
        animIDCrouching = Animator.StringToHash("Crouching");

        //set dynamic variables to defualt state
        _speedAddition = 0;
        _sliding = false;
        _canSlide = true;
        checkit = false;

        //set menu references
        pause = GameObject.Find("Menu").GetComponent<PauseMenu>();
        sensitivitySlider = GameObject.Find("Sensitivity Slider").GetComponent<Slider>();
        FOVSlider = GameObject.Find("FOV Slider").GetComponent<Slider>();
        HoldToCrouch = GameObject.Find("Hold To Crouch").GetComponent<Toggle>();

        //set script references
        wallrun = GameObject.Find("PlayerBody").GetComponent<WallRun>();
        recoil = GameObject.Find("CameraHolder").GetComponent<Recoil>();
    }

    private void FixedUpdate() 
    {
        ApplyMovement();
        ApplyFriction();
        Slide();
    }

    private void Update() 
    {
        Debug.DrawRay(transform.position, Vector3.ProjectOnPlane(transform.forward, new Vector3(Mathf.Abs(_slopeHit.normal.x), Mathf.Abs(_slopeHit.normal.y), Mathf.Abs(_slopeHit.normal.z))));
        Debug.DrawRay(transform.position, Vector3.down);

        _OnSlope();

        //updates FOV and Sensitivity settings based on slider values
        rotationSensitivity = sensitivitySlider.value;
        cameraFOV = FOVSlider.value;   

        Sprint();
        GroundCheck();
        SetAnimationState();

        SetHeight();

        _slopeMoveDirection = Vector3.ProjectOnPlane(orientation.forward, _slopeHit.normal);

        if (pause.GameIsPaused == false)
        {
            Rotation();
        }
       
        if (jumpInput.WasPressedThisFrame() && _airJumpsleft > 0 && _grounded == false && !_walling)
        {
            AirJump();
        }

        if (jumpInput.IsPressed() && _grounded == true && _groundedTime >= jumpCooldown)
        {
            Jump();
        }

        if (jumpInput.IsPressed() || sprintInput.WasPressedThisFrame())
        {
            _pressingSlide = false;
        }

        if (HoldToCrouch.isOn == true)
        {
            if (crouchInput.IsPressed())
            {
                _pressingSlide = true;
            }
            else
            {
                _pressingSlide = false;
            }
        }
        else
        {
            if (crouchInput.WasPressedThisFrame())
            {
                if (_pressingSlide == true)
                {
                    _pressingSlide = false;
                }
                else
                {
                    _pressingSlide = true;
                }
            }
        }

        //sets the wallrunning bool to true if wallrunning and false if not wallrunning
        if (wallrun.isWallrunning == true)
        {
            _walling = true;
        }
        else
        {
            _walling = false;
        }
    }

    private void GroundCheck() 
    {
      if (Physics.CheckSphere(transform.position, 0.25f, groundLayer))
        {
            _grounded = true;
            _airJumpsleft = airJumps;
            _groundedTime += Time.deltaTime;
        }
      else
        {
            _grounded = false;
            _groundedTime = 0;
        }
    }

    private void SetAnimationState()
    {
        animator.SetBool(animIDGrounded, _grounded);

        animator.SetBool(animIDCrouching, _crouching);

        animator.SetFloat(animIDSpeed, _speed);

        _speed = rb.velocity.magnitude - Mathf.Abs(rb.velocity.y);
    }

    void ApplyMovement() 
    {
        if (moveInput.ReadValue<Vector2>() != Vector2.zero)
        {
            _walking = true;
        }
        else
        {
            _walking = false;
        }

        if (_walling == false)
        {
            if (_sliding == false)
            {
                var axis = moveInput.ReadValue<Vector2>();
                var speed = _grounded ? movementSpeed : airMovementSpeed;
                var vertical = axis.y * speed * orientation.forward;
                var horizontal = axis.x * speed * orientation.right;
                rb.AddForce(horizontal + vertical, ForceMode.Acceleration);
            }

            if (_sprinting && _grounded && !_sliding)
            {
                //speed addition is gradually increased while sprinting until it reaches the sprint speed
                _speedAddition = Mathf.MoveTowards(_speedAddition, sprintSpeed, Time.deltaTime * sprintUpSpeed);
                //Sprint level is increased until it reaches the speed addition.
                _sprintLevel = Mathf.Clamp(Mathf.MoveTowards(_sprintLevel, _speedAddition, Time.deltaTime * sprintUpSpeed), 0, 10);
                _initialSlideSpeed = _sprintLevel;
            }
            else
            {
                //sprint level will always reset if not grounded or not sprinting or is sliding
                _sprintLevel = 0;

                //speed addition only resets if the sprint stops being pressed
                if (!_sprinting)
                {
                    _speedAddition = 0;
                }
            }

            if (_grounded == true && !_sliding)
            {
                //apply speed addition
                rb.AddForce(_slopeMoveDirection * (_speedAddition * moveInput.ReadValue<Vector2>().y));
                rb.AddForce(_slopeMoveDirection * (_speedAddition * Mathf.Abs(moveInput.ReadValue<Vector2>().x)));
            }

            if (!_sliding && !_sprinting)
            {
                _initialSlideSpeed = 0;//reset in case stops sprinting mid slide.
            }
        }

    }

    private void ApplyFriction() {
        if (wallrun.isWallrunning == false)
        {
            if (_sliding == false)
            {
                var vel = rb.velocity;
                var target = _grounded ? movingFriction : airFriction;
                vel.x = Mathf.Lerp(vel.x, 0f, target * Time.fixedDeltaTime);
                vel.z = Mathf.Lerp(vel.z, 0f, target * Time.fixedDeltaTime);
                rb.velocity = vel;
            }

            if (_sliding == true && _slopeMoveDirection.y >= 0f)
            {
                var vel = rb.velocity;
                var target = slidingFriction;
                vel.x = Mathf.Lerp(vel.x, 0f, target * Time.fixedDeltaTime);
                vel.z = Mathf.Lerp(vel.z, 0f, target * Time.fixedDeltaTime);
                rb.velocity = vel;
            }

            if (_sliding && _slopeMoveDirection.y < 0f)
            {
                var vel = rb.velocity;
                var target = slopeSlidingFriction;
                vel.x = Mathf.Lerp(vel.x, 0f, target * Time.fixedDeltaTime);
                vel.z = Mathf.Lerp(vel.z, 0f, target * Time.fixedDeltaTime);
                rb.velocity = vel;
            }

            if (moveInput.ReadValue<Vector2>() == Vector2.zero && !_sliding)
            {
                PlayerCollider.material = StandingFriction;
            }
            else
            {
                if (!_sliding)
                {
                    PlayerCollider.material = Player;
                }
                else
                {
                    if (_slopeMoveDirection.y >= 0)
                    {
                        PlayerCollider.material = Player;
                    }
                }               
            }
            
        }
        else
        {
            var vel = rb.velocity;
            var target = wallFriction;
            vel.x = Mathf.Lerp(vel.x, 0f, target * Time.fixedDeltaTime);
            vel.z = Mathf.Lerp(vel.z, 0f, target * Time.fixedDeltaTime);
            rb.velocity = vel;
        }
        
    }

    private void Rotation() {

        if (Cursor.lockState != CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        var mouseDelta = new Vector2((lookInput.ReadValue<Vector2>().x * 0.05f), (lookInput.ReadValue<Vector2>().y * 0.05f));
        _yRotation -= mouseDelta.y * rotationSensitivity;
        _xRotation += mouseDelta.x * rotationSensitivity;
        _yRotation = Mathf.Clamp(_yRotation, -rotationBounds, rotationBounds);
        _yRotationFinal = _yRotation + recoil.currentRotation.x;
        _xRotationFinal = _xRotation;


        if (wallrun.isWallrunning == false)
        {
            orientation.rotation = Quaternion.Euler(0, _xRotationFinal, 0);
            cameraHolder.localRotation = Quaternion.Euler(_yRotationFinal, _xRotationFinal, 0);
        }
        if (wallrun.isWallrunning == true)
        {
            cameraHolder.rotation = Quaternion.Euler(_yRotationFinal, _xRotationFinal, 0);

            if (wallrun.wallLeft)
            {
                orientation.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(orientation.forward, wallrun.leftWallHit.normal));
            }
            else
            {
                orientation.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(orientation.forward, wallrun.rightWallHit.normal));
            }
            
        }
    }

    private void AirJump() 
    {
        var vel = rb.velocity;
        vel.y = airJumpForce;
        rb.velocity = vel;
        _airJumpsleft--;
        rb.AddForce(orientation.forward * airJumpForwardForce, ForceMode.VelocityChange);
    }

    private void Jump()
    {
        var vel = rb.velocity;
        vel.y = jumpForce;
        rb.velocity = vel;
    }

    private void Sprint()
    {
        if (sprintInput.IsPressed() && moveInput.ReadValue<Vector2>().y > 0)
        {
            _sprinting = true;
        }
        else
        {
            _sprinting = false;
        }
    }



    void Slide()
    {
        if (_pressingSlide)
        {
            if (_canSlide == true && _sprinting)
            {
                if (_grounded == true)
                {
                    rb.velocity += _slopeMoveDirection * slideForce;
                }
            }
        }       

        if (_pressingSlide)
        {
            if (checkit == false)
            {
                checkit = true;

                if (_slopeMoveDirection.y < 0)
                {
                    PlayerCollider.material = NoFriction;
                }
                else
                {
                    PlayerCollider.material = Player;
                }
            }


            if (_grounded == true)
            {
                if ((_sprinting || _sliding) && rb.velocity.magnitude > 3 && _grounded)
                {
                    _sliding = true;
                    rb.AddForce(Vector3.down * slideSlopeForce);
                }
                else
                {
                    _crouching = true;
                    _sliding = false;
                }
            }

        }
        else
        {
            if (checkit == true)
            {
                checkit = false;
                _ycurrent = 0;
            }

            _sliding = false;
            _crouching = false;
        }


        if (_pressingSlide == true)
        {
            _slideReset = true;
            _canSlide = false;          
        }
        else
        {            
            if (_slideReset == true)
            {                
                if (!_sliding && _grounded)
                {
                    _slideReset = false;
                    StartCoroutine(Wait_For_Time());
                }
            } 
        }
    }

    private void SetHeight()
    {
        if (_sliding || _crouching)
        {
            PlayerCollider.height = Mathf.Lerp(PlayerCollider.height, crouchScale, crouchSpeed * Time.deltaTime);
            PlayerCollider.center = Vector3.Lerp(PlayerCollider.center, new Vector3(0, crouchScale / 2, 0), crouchSpeed * Time.deltaTime);
            cameraPosition.localPosition = Vector3.Lerp(cameraPosition.localPosition, crouchCamPos, crouchSpeed * Time.deltaTime);
        }
        else
        {
            PlayerCollider.height = Mathf.Lerp(PlayerCollider.height, playerScale, crouchSpeed * Time.deltaTime);
            PlayerCollider.center = Vector3.Lerp(PlayerCollider.center, new Vector3(0, playerScale / 2, 0), crouchSpeed * Time.deltaTime);
            cameraPosition.localPosition = Vector3.Lerp(cameraPosition.localPosition, standCamPos, crouchSpeed * Time.deltaTime);
        }
    }

    IEnumerator Wait_For_Time()
    {
        if (_grounded)
        {
            _canSlide = false;
            yield return new WaitForSeconds(slideWaitTime);
            _canSlide = true;
        }
        else
        {
            _slideReset = true;
        }
    }
}





