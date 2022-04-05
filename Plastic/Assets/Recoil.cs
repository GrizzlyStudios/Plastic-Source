using UnityEngine;

public class Recoil : MonoBehaviour
{
    //references
    public GunData gunData;
    [ReadOnly]
    public BaseGun gun;
    [ReadOnly]
    public PlayerController playerController;

    //rotations
    public Vector2 currentRotation;
    public Vector2 targetRotation;
    private Vector2 _mouseRotation;
    [SerializeField] private Vector2 _startingMouseRotation;

    //hipfire Recoil
    [ReadOnly]
    [SerializeField] private float recoilX;
    [ReadOnly]
    [SerializeField] private float recoilY;

    //settings
    [ReadOnly]
    [SerializeField] private float snappiness;
    [ReadOnly]
    [SerializeField] private float returnSpeed;
    [ReadOnly]
    [SerializeField] private float recoilCounterSpeed;

    private void Awake()
    {
        //set variables
        recoilX = gunData.recoilX;
        recoilY = gunData.recoilY;
        snappiness = gunData.snappiness;
        returnSpeed = gunData.returnSpeed;
        recoilCounterSpeed = gunData.recoilCounterSpeed;

        //set references
        gun = GameObject.Find("Gun").GetComponent<BaseGun>();
        playerController = GameObject.Find("PlayerBody").GetComponent<PlayerController>();
    }

    private void Update()
    {
        

        //reset recoil
        if (gun._shooting == false)
        {
            //return to position after shooting
            targetRotation = Vector2.Lerp(targetRotation, Vector2.zero, returnSpeed * Time.deltaTime);
            //set starting rotation to rotation

        }
        else
        {
            //make recoil more snappy by slightly counteracting the recoil while shooting
            targetRotation = Vector2.MoveTowards(targetRotation, Vector2.zero, recoilCounterSpeed * Time.deltaTime);

        }

        //rotate
        currentRotation = Vector2.Lerp(currentRotation, targetRotation, snappiness * Time.fixedDeltaTime);
    }

    public void RecoilFire()
    {
        targetRotation += new Vector2(recoilX, Random.Range(-recoilY, recoilY));
        Debug.Log("recoil");
    }
}
