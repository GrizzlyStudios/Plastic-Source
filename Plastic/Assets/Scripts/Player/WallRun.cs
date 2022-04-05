using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class WallRun : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform RaycastPoint;
    public Rigidbody rb;
    public PlayerController Player;
    public Camera cam;

    [Header("Wallrunning")]
    public float wallDistance;
    public float StickToWallForce;
    
    public float MaxUpVelocity;
    public float MaxDownVelocity;
    public float UpForce;
    public float Downforce;
    public float maxWallRunCameraTilt;

    public float wallRunJumpForce;
    public float WallRunJumpUpForceMultiplier;
    public float wallrunForce;
    public float maxComboForce;
    public float maxRegWallVelocity;
    public float maxComboVelocity;
    public float minimumWallVelocity;
    public float minimumWallBoostForce;
    public float wallCameraTiltSpeed;
    public float slippingCameraTiltSpeed;
    public float verticalRegulateWaitTime;
    public float wallEnterUpForce;
    public float wallEnterForwardForce;
    public float waitBetweenWallRunTime;


    [Header("Input System")]
    //new input system references
    public PlayerInput playerInput;
    public InputAction moveInput;
    public InputAction lookInput;
    public InputAction sprintInput;
    public InputAction jumpInput;
    public InputAction crouchInput;

    //Dynamic variables
    [Header("WallDirection")]
    public float leftWallDistance;
    public float rightWallDistance;
    public bool wallRight;
    public bool wallLeft;
    public RaycastHit leftWallHit;
    public RaycastHit rightWallHit;
    public Vector3 wallNormalVector;
    RaycastHit wallNear;

    public int wallsInARow;
    public bool enterWallRun = false;
    public bool CanWallRun;
    public bool isWallrunning;
    public float wallRunCameraTilt;
    public bool regulateVerticalSpeed;
    public float wallRunGravity;
    public float wallRunCurveSpeed;

    public float _wallRunGravity;
    public float _MaxUpVelocity;
    public float _MaxDownVelocity;
    public float _UpForce;
    public float _Downforce;

    public Vector3 leftNormal;
    public Vector3 rightNormal;
    public Vector3 bannedNormal;
    public bool _slippingOffWall;

    private void Awake()
    {
        //set inputs
        moveInput = playerInput.actions["Move"];
        lookInput = playerInput.actions["Look"];
        sprintInput = playerInput.actions["Sprint"];
        jumpInput = playerInput.actions["Jump"];
        crouchInput = playerInput.actions["Crouch"];

        _slippingOffWall = false;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Debugger();
        CheckForWall();
        AllowWallRun();

        if (Player._grounded == true)
        {
            wallsInARow = 0;
        }

        leftNormal = leftWallHit.normal;
        rightNormal = rightWallHit.normal;

        //While Wallrunning Camera Tilt
        if (System.Math.Abs(wallRunCameraTilt) < maxWallRunCameraTilt && isWallrunning == true && (wallRight) && _slippingOffWall == false)
        {
            wallRunCameraTilt = Mathf.Lerp(wallRunCameraTilt, maxWallRunCameraTilt, Time.deltaTime * wallCameraTiltSpeed);
        }

        if (System.Math.Abs(wallRunCameraTilt) < maxWallRunCameraTilt && isWallrunning == true && (wallLeft) && _slippingOffWall == false)
        {
            wallRunCameraTilt = Mathf.Lerp(wallRunCameraTilt, -maxWallRunCameraTilt, Time.deltaTime * wallCameraTiltSpeed);
        }

        cam.transform.localRotation = Quaternion.Euler(0, 0, wallRunCameraTilt);

        //Tilts camera back again
        if (wallRunCameraTilt > 0 && isWallrunning == false)
        {
            wallRunCameraTilt = Mathf.Lerp(wallRunCameraTilt, 0, Time.deltaTime * wallCameraTiltSpeed);
        }

        if (wallRunCameraTilt < 0 && isWallrunning == false)
        {
            wallRunCameraTilt = Mathf.Lerp(wallRunCameraTilt, 0, Time.deltaTime * wallCameraTiltSpeed);
        }

        
    }

    //checks if all wallrun dependencies are true then starts wallrun if they are and stops them if they are false
    //also checks for walljump
    void AllowWallRun()
    {
        if (CanWallRun == true && ((wallLeft == true && leftNormal != bannedNormal) || (wallRight == true && rightNormal != bannedNormal)) && moveInput.ReadValue<Vector2>().y > 0 && Player._grounded == false)
        {
            if (!jumpInput.WasPressedThisFrame())
            {
                StartwallRun();
            }
            else
            {
                StopWallRun();
                WallJump();
            }
        }
        else
        {
            StopWallRun();
        }
    }

    //check if touching wall
    private void OnCollisionStay(Collision collision)
    {
        if (collision.transform.CompareTag("RunnableWall"))
        {
            CanWallRun = true;
            if (isWallrunning)
            {
                if (enterWallRun == false)
                {
                    regulateVerticalSpeed = false;
                    _UpForce = UpForce;
                    _MaxUpVelocity = MaxUpVelocity;
                    _MaxDownVelocity = MaxDownVelocity;
                    _Downforce = Downforce;
                    _wallRunGravity = wallRunGravity;

                    if (rb.velocity.magnitude - rb.velocity.y < maxComboVelocity)
                    {
                        rb.velocity += orientation.forward * wallEnterForwardForce;
                    }

                    enterWallRun = true;
                    wallsInARow += 1;

                    if (Mathf.Abs(leftWallHit.normal.z) < 0 || Mathf.Abs(rightWallHit.normal.z) < 0)
                    {
                        //tilted wall
                    }
                    else
                    {
                        //vertical wall
                        rb.AddForce(orientation.up * wallEnterUpForce, ForceMode.VelocityChange);
                    }
                }

                if (regulateVerticalSpeed == false)
                {
                    StartCoroutine(WaitToRegulateVerticalSpeed());
                }
            }
        }
    }

    //check if not touching wall
    private void OnCollisionExit(Collision collision)
    {
        if (collision.transform.CompareTag("RunnableWall"))
        {
            CanWallRun = false;
        }
    }

    //while wall running
    void StartwallRun()
    {
        if (regulateVerticalSpeed == true)
        {
            if (rb.velocity.y > _MaxUpVelocity)
            {
                rb.velocity += orientation.up * -UpForce * Mathf.Abs(rb.velocity.y) * Time.deltaTime;
            }

            if (rb.velocity.y < _MaxDownVelocity)
            {
                rb.velocity += orientation.up * Downforce * Mathf.Abs(-rb.velocity.y) * Time.deltaTime;
            }
        }

        if (rb.velocity.magnitude - Math.Abs(rb.velocity.y) < minimumWallVelocity)
        {
            rb.velocity += orientation.forward * minimumWallBoostForce * Time.deltaTime;
        }

        if (rb.velocity.magnitude - Math.Abs(rb.velocity.y) < (maxRegWallVelocity + (wallsInARow / 3)))
        {
            rb.velocity += orientation.forward * Mathf.Clamp(wallrunForce + (wallsInARow / 3),0, maxComboForce) * Time.deltaTime;
        }

        rb.useGravity = false;
        isWallrunning = true;

        if (_MaxUpVelocity == 0 && _wallRunGravity < 0.1f)
        {
            _wallRunGravity = Mathf.MoveTowards(_wallRunGravity, 0.1f, wallRunCurveSpeed * 0.02f * Time.deltaTime);
        }

        if (_wallRunGravity >= 0.1f && _wallRunGravity < 0.15f)
        {
            _wallRunGravity = Mathf.MoveTowards(_wallRunGravity, 0.15f, wallRunCurveSpeed * 0.02f * Time.deltaTime);
        }

        if (_wallRunGravity >= 0.15f && _wallRunGravity < 0.2f)
        {
            _wallRunGravity = Mathf.MoveTowards(_wallRunGravity, 0.2f, wallRunCurveSpeed * 0.05f * Time.deltaTime);
        }

        if (_wallRunGravity >= 0.2f && _wallRunGravity < 1)
        {
            _wallRunGravity = Mathf.MoveTowards(_wallRunGravity, 1, wallRunCurveSpeed * 0.5f * Time.deltaTime);
        }

        if (_wallRunGravity >= 1)
        {
            _wallRunGravity = Mathf.MoveTowards(_wallRunGravity, 1.5f, wallRunCurveSpeed * 0.5f * Time.deltaTime);
        }

        if (_wallRunGravity >= 1.5f && isWallrunning)
        {
            _slippingOffWall = true;
            wallRunCameraTilt = Mathf.MoveTowards(wallRunCameraTilt, 10, Time.deltaTime * slippingCameraTiltSpeed);
        }

        if (Mathf.Abs(wallRunCameraTilt) <= 10 && isWallrunning && _slippingOffWall)
        {
            StopWallRun();
        }

        _MaxUpVelocity = Mathf.MoveTowards(_MaxUpVelocity, 0, wallRunCurveSpeed * 10 * Time.deltaTime);
        _MaxDownVelocity = Mathf.MoveTowards(_MaxDownVelocity, 7, wallRunCurveSpeed * Time.deltaTime);
        _UpForce = Mathf.MoveTowards(_UpForce, 7, wallRunCurveSpeed * 10 * Time.deltaTime);
        _Downforce = Mathf.MoveTowards(_Downforce, 0, wallRunCurveSpeed * Time.deltaTime);
        rb.AddForce(Vector3.down * _wallRunGravity, ForceMode.Force);

        if (wallRight)
        {
            rb.velocity += (StickToWallForce * Time.deltaTime * orientation.right);
            if (moveInput.ReadValue<Vector2>().x < 0)
            {
                StopWallRun();
            }
        }

        if (wallLeft)
        {
            rb.velocity += (StickToWallForce * Time.deltaTime * -orientation.right);
            if (moveInput.ReadValue<Vector2>().x > 0)
            {
                StopWallRun();
            }
        }
    }

    IEnumerator WaitToRegulateVerticalSpeed()
    {
        regulateVerticalSpeed = false;
        yield return new WaitForSeconds(verticalRegulateWaitTime);
        if (isWallrunning)
        {
            regulateVerticalSpeed = true;
        }
    }

    void StopWallRun()
    {
        if (isWallrunning)
        {
            bannedNormal = wallLeft ? leftNormal : rightNormal;
        }
        else
        {
            if (Player._grounded)
            {
                bannedNormal = Vector3.zero;
            }
        }
        _slippingOffWall = false;
        _UpForce = UpForce;
        _MaxUpVelocity = MaxUpVelocity;
        _MaxDownVelocity = MaxDownVelocity;
        _Downforce = Downforce;
        _wallRunGravity = wallRunGravity;

        regulateVerticalSpeed = false;
        rb.useGravity = true;
        isWallrunning = false;
        CanWallRun = false;
        if (enterWallRun == true)
        {
            StartCoroutine(waitBetweenWallRunCo());
        }
    }

    IEnumerator waitBetweenWallRunCo()
    {
        yield return new WaitForSeconds(waitBetweenWallRunTime);
        if (isWallrunning == false)
        {
            enterWallRun = false;
        }
    }

    void CheckForWall()
    {
        Physics.Raycast(RaycastPoint.position, -orientation.right, out leftWallHit, wallDistance);
        Physics.Raycast(RaycastPoint.position, orientation.right, out rightWallHit, wallDistance);

        Physics.Raycast(RaycastPoint.position, orientation.right, out wallNear, 5);

        if (leftWallHit.distance == 0f)
        {
            leftWallDistance = 5f;
        }
        else
        {
            leftWallDistance = leftWallHit.distance;
        }

        if(leftWallDistance < rightWallDistance)
        {
            wallLeft = true;
        }
        else
        {
            wallLeft = false;
        }

        if (rightWallHit.distance == 0f)
        {
            rightWallDistance = 5f;
        }
        else
        {
            rightWallDistance = rightWallHit.distance;
        }

        if (rightWallDistance < leftWallDistance)
        {
            wallRight = true;
        }
        else
        {
            wallRight = false;
        }
    }

    void WallJump()
    {
        if (wallLeft)
        {
            Vector3 wallRunJumpDirection = RaycastPoint.up + leftWallHit.normal;
            rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            rb.AddForce(wallRunJumpDirection * wallRunJumpForce * 100, ForceMode.Force);
            Vector3 vel = rb.velocity;
            vel.y = WallRunJumpUpForceMultiplier;
            rb.velocity = vel;
            Player._airJumpsleft = Player.airJumps;
        }
        else 
        { 
            if (wallRight)
            {
                Vector3 wallRunJumpDirection = RaycastPoint.up + rightWallHit.normal;
                rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
                rb.AddForce(wallRunJumpDirection * wallRunJumpForce * 100, ForceMode.Force);
                Vector3 vel = rb.velocity;
                vel.y = WallRunJumpUpForceMultiplier;
                rb.velocity = vel;
                Player._airJumpsleft = Player.airJumps;
            }
        }
        Debug.Log("jump");
    }

    void Debugger()
    {
    }

    public void prepareWallrun()
    {

    }
}



