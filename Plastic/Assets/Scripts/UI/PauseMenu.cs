using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    public bool GameIsPaused = false;
    public GameObject pauseMenuUI;
    public GameObject Crosshairs;
    //FOV
    public Text FOV;
    public Slider FOVSlider;
    //Sensitivity
    public Text Sensitivity;
    public Slider SensitivitySlider;

    private void Start()
    {
        Resume();
    }

    private void OnGUI()
    {
        FOV.text = FOVSlider.value.ToString("0.00");
        Sensitivity.text = SensitivitySlider.value.ToString("0.00");
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (GameIsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        GameIsPaused = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true);
        GameIsPaused = true;
        Cursor.lockState = CursorLockMode.None;
    }
}
