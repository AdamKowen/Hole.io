using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [Header("UI")]
    [SerializeField] private GameObject playCanvas;

    [Header("References")]
    [SerializeField] private HoleController hole;           // drag your hole object here

    public HoleController Hole => hole;
    [SerializeField] private CameraFollowZoom2D camCtrl;   // drag Main Camera with CameraFollowZoom2D

    [Header("Menu Settings")]
    [SerializeField] private float menuOrthoSize = 12f;
    [SerializeField] private bool showMenuOnStart = true;

    void Start()
    {
        Application.targetFrameRate = 120;
        QualitySettings.vSyncCount = 1;

        bool skipMenu = PlayerPrefs.GetInt("SkipMenuOnce", 0) == 1;
        if (skipMenu)
        {
            PlayerPrefs.SetInt("SkipMenuOnce", 0); // consume flag
            PlayerPrefs.Save();
            StartGame();                
        }
        else
        {
            if (showMenuOnStart) EnterMenu(true);  
            else StartGame();
        }
    }


    // Hook this to the Button's OnClick
    public void OnPlayButton()
    {
        StartGame();
    }

    public void EnterMenu(bool snapZoom = false)
    {
        if (playCanvas) playCanvas.SetActive(true);

        if (hole)
        {
            hole.enabled = false;
            var rb = hole.GetComponent<Rigidbody2D>();
            if (rb) rb.linearVelocity = Vector2.zero;
        }

        if (camCtrl) camCtrl.EnterMenuMode(menuOrthoSize, snapZoom);
        // Do NOT change Time.timeScale so NPCs keep moving
    }

    public void StartGame()
    {
        if (playCanvas) playCanvas.SetActive(false);
        if (hole)
        {
            hole.Freeze(false);   
            hole.enabled = true;
        }
        if (camCtrl) camCtrl.ExitMenuMode();

        if (UIManager.Instance)
            UIManager.Instance.StartGame();
    }



    public void GameOver()
    {
        if (hole)
        {
            hole.Freeze(true);    
            hole.enabled = false;
        }
        if (UIManager.Instance)
            UIManager.Instance.EndGame();
    }
}