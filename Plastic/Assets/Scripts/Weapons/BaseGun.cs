using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BaseGun : MonoBehaviour
{
    [Header("Weapon")]
    [SerializeReference] GunData gunData;

    [Header("References")]
    [ReadOnly]
    [SerializeReference] private Camera cam;
    [ReadOnly]
    [SerializeReference] private Transform muzzle;
    [ReadOnly]
    [SerializeReference] private Toggle HoldToADS;
    [ReadOnly]
    [SerializeReference] private PauseMenu pauseMenu;
    [ReadOnly]
    [SerializeReference] private Camera weaponCam;
    [ReadOnly]
    [SerializeReference] private PlayerController playerController;
    [ReadOnly]
    [SerializeReference] private Recoil recoilScript;

    [Header("Input System")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private PlayerInput weaponInput;
    private InputAction shootInput;
    private InputAction reloadInput;
    private InputAction ADSInput;

    [Header("Aiming")]
    [ReadOnly]
    [SerializeReference] private Transform ADSPosition;
    [ReadOnly]
    [SerializeReference] private Transform HipPosition;
    [ReadOnly]
    public float sightOffset;
    [ReadOnly]
    public float ADSTime;
    [ReadOnly]
    public float aimOutTime;

    [Header("Dynamic Variables")]
    [ReadOnly]
    public int _bulletsLeftInMag;
    [ReadOnly]
    public bool _pressingShoot, _pressingADS, _readyToShoot, _reloading, _shooting;
    [ReadOnly]
    public float _waitFire;
    [ReadOnly]
    public int _reserveAmmo;
    [ReadOnly]
    public Vector3 _targetPosition;
    [ReadOnly]
    public Quaternion _targetRotation;
    [ReadOnly]
    public Vector3 _aimTargetPosition;
    [ReadOnly]
    public Quaternion _aimTargetRotation;
    [ReadOnly]
    public float _targetWeaponFOV;
    [ReadOnly]
    public float _targetFOV;

    private void Awake()
    {
        //input system
        shootInput = weaponInput.actions["Shoot"];
        reloadInput = weaponInput.actions["Reload"];
        ADSInput = weaponInput.actions["ADS"];

        //set references
        cam = GameObject.Find("Player Cam").GetComponent<Camera>();
        weaponCam = GameObject.Find("Weapon Cam").GetComponent<Camera>();
        HoldToADS = GameObject.Find("Hold To ADS").GetComponent<Toggle>();
        ADSPosition = GameObject.Find("Sight Target").GetComponent<Transform>();
        HipPosition = GameObject.Find("Hip Target").GetComponent<Transform>();
        pauseMenu = GameObject.Find("Menu").GetComponent<PauseMenu>();
        playerController = GameObject.Find("PlayerBody").GetComponent<PlayerController>();
        recoilScript = GameObject.Find("CameraHolder").GetComponent<Recoil>();

        //set variables
        _bulletsLeftInMag = gunData.magSize;
        _reserveAmmo = gunData.maxReserveAmmo;
        ADSTime = gunData.ironAimInTime;
        aimOutTime = gunData.aimOutTime;
        _targetFOV = playerController.cameraFOV;
    }

    private void Update()
    {
        if (!pauseMenu.GameIsPaused)
        {
            Input();
            CalculateAim();
        }
    }

    private void CalculateAim()
    {
        if (_pressingADS)
        {
            _aimTargetPosition = Vector3.Lerp(transform.position, ADSPosition.position, ADSTime * Time.deltaTime);
            _aimTargetRotation = Quaternion.Lerp(transform.rotation, ADSPosition.rotation, ADSTime * Time.deltaTime);

            _targetWeaponFOV = Mathf.Lerp(_targetWeaponFOV, gunData.aimWeaponFOV, ADSTime * Time.deltaTime);
            _targetFOV = Mathf.Lerp(_targetFOV, (gunData.aimFOV / 60) * playerController.cameraFOV, ADSTime * Time.deltaTime);
        }
        else
        {
            _aimTargetPosition = Vector3.Lerp(transform.position, HipPosition.position, ADSTime * Time.deltaTime);
            _aimTargetRotation = Quaternion.Lerp(transform.rotation, HipPosition.rotation, ADSTime * Time.deltaTime);

            _targetWeaponFOV = Mathf.Lerp(_targetWeaponFOV, gunData.weaponFOV, ADSTime * Time.deltaTime);
            _targetFOV = Mathf.Lerp(_targetFOV, playerController.cameraFOV, aimOutTime * Time.deltaTime);
        }
        _targetPosition = _aimTargetPosition;
        _targetRotation = _aimTargetRotation;

        transform.position = _targetPosition;
        transform.rotation = _targetRotation;

        weaponCam.fieldOfView = _targetWeaponFOV;
        cam.fieldOfView = _targetFOV;
    }

    private float TimeBetweenShots()
    {
        //determine time between shots based on firerate
        return 1f / (gunData.fireRate / 60f);
    }

    private void Shoot(int bulletsPerShoot)
    {
        _readyToShoot = false;
        //subtract how many bullets we shoot
        _bulletsLeftInMag -= bulletsPerShoot;
        RaycastHit whatIHit;
        //repeat the shoot function for the number of bullets per shoot
        for (int i = 0; i < bulletsPerShoot; i++)
        {
            //shoot raycast
            if (Physics.Raycast(cam.transform.position, cam.transform.forward, out whatIHit, gunData.maxDistance))
            {
                IDamageable damageable = whatIHit.collider.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.DealDamage(gunData.damage);
                }
            }

            //call recoil function
            recoilScript.RecoilFire();
        }
    }

    private void Input()
    {
        //if we can hold to shoot then set pressing shoot to true when holding shoot
        //if we can't hold to shoot then set pressing shoot to true if pressed shoot this frame
        _pressingShoot = gunData.automatic? shootInput.IsPressed() : shootInput.WasPressedThisFrame();

        if (HoldToADS.isOn == true)
        {
            _pressingADS = ADSInput.IsPressed();
        }
        else
        {
            if (ADSInput.WasPressedThisFrame())
            {
                if (_pressingADS == true)
                {
                    _pressingADS = false;
                }
                else
                {
                    _pressingADS = true;
                }
            }
        }

        //reload if all requirements to reload are true
        if (reloadInput.WasPressedThisFrame() && _bulletsLeftInMag < gunData.magSize && _reserveAmmo > 0 && !_reloading)
        {
            _bulletsLeftInMag = gunData.magSize;
        }

        //Shoot        
        if (_pressingShoot && !_reloading && _bulletsLeftInMag > 0)
        {
            //set shooting state
            _shooting = true;

            _waitFire += Time.deltaTime;//adds how much time has passed each frame

            if (_waitFire > TimeBetweenShots()) //Fires gun everytime timer exceeds firerate
            {
                //set the amount of bullets we shoot to waitfire divided by time between shots minus the decimal
                //we do this because you can't shoot a decmal amount of times
                int bulletsPerShoot = Mathf.FloorToInt(_waitFire / TimeBetweenShots());

                //subtract the amount of times we shot times the time between shots from wait fire
                _waitFire -= TimeBetweenShots() * Mathf.FloorToInt(_waitFire / TimeBetweenShots());
                Shoot(bulletsPerShoot);
            }
        }
        else
        {
            //set shooting state
            _shooting = false;
        }
    }
}
