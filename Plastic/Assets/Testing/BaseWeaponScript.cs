using UnityEngine;
using UnityEngine.InputSystem;

public class BaseWeaponScript : MonoBehaviour
{
    //watch dapper dinos vid for the raycast stuff and daves for the customization

    [Header("Gun Stats")]

    public int damage;
    public float fireRate;
    public float spread;
    public float range;
    public float reloadTime;
    public int magazineSize;
    public bool allowButtonHold;

    [Header("Dependencies")]
    public Camera cam;
    public Transform attackPoint;
    public RaycastHit rayHit;
    public LayerMask Enemy;
    //input system
    public PlayerInput playerInput;
    public InputAction shootInput;
    public InputAction reloadInput;

    [Header("Animation")]
    //animation


    //animator parameters
    private int animIDShooting;
    private int animIDReloading;

    [Header("Dynamic Variables")]
    public int _bulletsLeft;
    public bool _pressingShoot, _readyToShoot, _reloading;
    public float _waitFire;
    public float delete;

    private void Awake()
    {
        //input system
        shootInput = playerInput.actions["Shoot"];

        //reset variables
        _bulletsLeft = magazineSize;
        _readyToShoot = true;

        //set animation states
        animIDShooting = Animator.StringToHash("Shooting");
        animIDReloading = Animator.StringToHash("Reloading");
    }

    private void Update()
    {
        Input();
    }

    private void Input()
    {
        //shootInput
        _pressingShoot = allowButtonHold ? shootInput.IsPressed() : shootInput.WasPressedThisFrame();

        //reload
        if (reloadInput.WasPressedThisFrame() && _bulletsLeft < magazineSize && !_reloading)
        {
            Reload();
        }


        //Shoot        
        if (_pressingShoot && !_reloading && _bulletsLeft > 0)
        {
            _waitFire += Time.deltaTime;
            if (_waitFire > fireRate) //Fires gun everytime timer exceeds firerate
            {
                int bulletsPerShoot = Mathf.FloorToInt(_waitFire / fireRate);
                _waitFire -= fireRate * Mathf.FloorToInt(_waitFire / fireRate);               
                Shoot(bulletsPerShoot);
                delete += bulletsPerShoot;
            }
        }
    }

    private void Reload()
    {
        _reloading = true;
        Invoke("ReloadFinished", reloadTime);
    }

    private void ReloadFinished()
    {
        _bulletsLeft = magazineSize;
        _reloading = false;
    }

    private void Shoot(int bulletsPerShoot)
    {
        _readyToShoot = false;
        _bulletsLeft -= bulletsPerShoot;
        Debug.Log(bulletsPerShoot);
        RaycastHit whatIHit;
        for (int i = 0; i < bulletsPerShoot; i++)
        {
            if (Physics.Raycast(cam.transform.position, cam.transform.forward, out whatIHit, Mathf.Infinity))
            {
                IDamageable damageable = whatIHit.collider.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.DealDamage(damage);
                }
            }
        }
    }

}
