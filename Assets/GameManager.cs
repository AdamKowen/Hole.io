using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject playCanvas;

    [Header("References")]
    public HoleController hole;           // drag your hole object here
    public CameraFollowZoom2D camCtrl;   // drag Main Camera with CameraFollowZoom2D

    [Header("Menu Settings")]
    public float menuOrthoSize = 12f;
    public bool showMenuOnStart = true;

    private void Start()
    {

        if (showMenuOnStart) EnterMenu(true);
        else StartGame();
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

        if (hole) hole.enabled = true;

        if (camCtrl) camCtrl.ExitMenuMode();
    }

    // Call this when you implement "game over" later
    public void GameOver()
    {
        EnterMenu(false);
    }
}