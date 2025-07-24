using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    [SerializeField] private Button playButton;
    private TextMeshProUGUI _levelText;
    private TextMeshProUGUI _timeText;

    private GameObject _pausePanel;
    private GameObject _gameoverPanel;
    private GameObject _completePanel;
    private Button _resume;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Scene loaded: {scene.name}");

        switch (scene.name)
        {
            case "MenuGame":
                SetupMenuSceneUI();
                break;
            case "Gameplay":
                SetupGameplaySceneUI();
                break;
        }
    }

    private void SetupMenuSceneUI()
    {
        Button playBtn = GameObject.Find("PlayButton")?.GetComponent<Button>();
        if (playBtn != null)
        {
            playBtn.onClick.RemoveAllListeners();
            playBtn.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("Gameplay");
            });
        }
    }

    private void SetupGameplaySceneUI()
    {
        SetupPausePanel();
        SetupGameOverPanel();
        SetupCompletePanel();

        Button pauseBtn = GameObject.Find("PauseButton")?.GetComponent<Button>();
        if (pauseBtn != null && _pausePanel != null)
        {
            pauseBtn.onClick.RemoveAllListeners();
            pauseBtn.onClick.AddListener(() =>
            {
                _pausePanel.SetActive(true);
                Time.timeScale = 0f;
            });
        }

        GameObject levelObj = GameObject.Find("LevelText");
        if (levelObj != null)
        {
            _levelText = levelObj.GetComponent<TextMeshProUGUI>();
            _levelText.text = "1";
        }

        GameObject timeObj = GameObject.Find("TimeText");
        if (timeObj != null)
        {
            _timeText = timeObj.GetComponent<TextMeshProUGUI>();
            _timeText.text = "60";
        }
    }
    private void SetupPausePanel()
    {
        _pausePanel = GameObject.Find("PausePanel");

        Button resumeBtn = GameObject.Find("ResumeButton")?.GetComponent<Button>();
        if (resumeBtn != null && _pausePanel != null)
        {
            resumeBtn.onClick.RemoveAllListeners();
            resumeBtn.onClick.AddListener(() =>
            {
                _pausePanel.SetActive(false);
                Time.timeScale = 1.0f;
            });
        }

        Button backBtn = GameObject.Find("MenuButton_1")?.GetComponent<Button>();
        if (backBtn != null)
        {
            backBtn.onClick.RemoveAllListeners();
            backBtn.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("MenuGame");
            });
        }
    
        if (_pausePanel != null)
            _pausePanel.SetActive(false);
    }
    private void SetupGameOverPanel()
    {
        _gameoverPanel = GameObject.Find("GameOverPanel");

        Button backBtn = GameObject.Find("MenuButton_2")?.GetComponent<Button>();
        if (backBtn != null)
        {
            backBtn.onClick.RemoveAllListeners();
            backBtn.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("MenuGame");
            });
        }
        
        if (_gameoverPanel != null)
        {
            _gameoverPanel.SetActive(false);
        }
    }
    public void ShowGameOverPanel()
    {
        if (_gameoverPanel != null)
        {
            _gameoverPanel.SetActive(true);
            Time.timeScale = 0f;
        }
    }
    private void SetupCompletePanel()
    {
        _completePanel = GameObject.Find("CompletePanel");

        Button nextBtn = GameObject.Find("NextLevel")?.GetComponent<Button>();
        if (nextBtn != null)
        {
            nextBtn.onClick.RemoveAllListeners();
            nextBtn.onClick.AddListener(() =>
            {
                _completePanel.SetActive(false);
                Time.timeScale = 1f;
            });
        }

        Button backBtn = GameObject.Find("MenuButton_3")?.GetComponent<Button>();
        if (backBtn != null)
        {
            backBtn.onClick.RemoveAllListeners();
            backBtn.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("MenuGame");
            });
        }

        if (_completePanel != null)
        {
            _completePanel.SetActive(false);
        }
    }
    public void ShowCompletePanel()
    {
        if (_completePanel != null)
        {
            _completePanel.SetActive(true);
            Time.timeScale = 0f;
        }
    }
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    public void UpdateLevelText(int text)
    {
        if (_levelText != null)
        {
            _levelText.text = text.ToString();
        }
    }
    public void UpdateTimeText(int text)
    {
        if (_timeText != null)
        {
            _timeText.text = text.ToString();
        }
    }
}
