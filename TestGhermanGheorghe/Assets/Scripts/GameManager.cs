using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; 

    public TextMeshProUGUI scoreText;    
    public TextMeshProUGUI timerText;    

    public GameObject pauseMenuPanel;    
    public GameObject lostPanel;
    public GameObject Shelfs; 
    public GameObject PauseButton;

    public int levelIndex; 
    public float timer;    
    public int targetScore; 

    private int score;
    private int highScore;
    private bool isPaused;
    private bool hasLost;

    private bool hasReachedQuarterTime; 
    private float originalTimer;        
    private Vector3 originalTimerScale;   
    private Coroutine popCoroutine;      

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(Instance.gameObject); 
            Instance = this;
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateUI();
        hasReachedQuarterTime = false;
    }

    private void Start()
    {
        UpdateUI();
        pauseMenuPanel.SetActive(false); 
        lostPanel.SetActive(false);     

        originalTimer = timer; 
        originalTimerScale = timerText.transform.localScale; 
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }

        if (!isPaused && !hasLost)
        {
            UpdateTimer(); 
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        pauseMenuPanel.SetActive(isPaused);
        Time.timeScale = isPaused ? 0 : 1; 
    }

    public void ResumeGame()
    {
        TogglePause(); 
    }

    public void PreviousLevel()
    {
        if (levelIndex > 0)
        {
            TogglePause();
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(currentSceneIndex - 1);
        }
    }

    public void NextLevel()
    {
        TogglePause();
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex + 1);
    }

    public void RestartGame()
    {
        TogglePause();
        RestartLevel();
    }

    public void RestartLevel()
    {
        UpdateUI();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); 
    }

    public void AddScore(int amount)
    {
        score += amount;
        CheckTargetScore(); 
        UpdateUI();
    }

    private void CheckTargetScore()
    {
        if (score >= targetScore)
        {
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(currentSceneIndex + 1);
        }
    }

    private void UpdateHighScore()
    {
        if (score > highScore)
        {
            highScore = score;
        }
    }

    private void UpdateUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score.ToString();
        }

        if (timerText != null)
        {
            timerText.text = "Time: " + Mathf.CeilToInt(timer).ToString(); 
        }
    }

    private void UpdateTimer()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime; 

           
            float quarterTime = originalTimer / 4;

            
            if (!hasReachedQuarterTime && timer <= quarterTime)
            {
                hasReachedQuarterTime = true; 
                AnimateTimerText();
            }
        }
        else if (!hasLost)
        {
            hasLost = true;
            ActivateLostPanel(); 
        }

        UpdateUI(); 
    }

    private void AnimateTimerText()
    {
        
        timerText.color = Color.red;

        
        if (popCoroutine == null)
        {
            popCoroutine = StartCoroutine(ContinuousPopText());
        }
    }

    private System.Collections.IEnumerator ContinuousPopText()
    {
        while (true) 
        {
            float duration = 0.5f; 
            float time = 0;
            Vector3 originalScale = timerText.transform.localScale;

            
            while (time < duration / 2)
            {
                time += Time.deltaTime;
                float scaleFactor = Mathf.Lerp(1, 1.5f, time / (duration / 2));
                timerText.transform.localScale = originalScale * scaleFactor;
                yield return null;
            }

            time = 0;

            
            while (time < duration / 2)
            {
                time += Time.deltaTime;
                float scaleFactor = Mathf.Lerp(1.5f, 1, time / (duration / 2));
                timerText.transform.localScale = originalScale * scaleFactor;
                yield return null;
            }

            
            timerText.transform.localScale = originalScale;

            yield return null; 
        }
    }

    private void ActivateLostPanel()
    {
        Shelfs.SetActive(false);
        PauseButton.SetActive(false);
        lostPanel.SetActive(true); 
         

        
        if (popCoroutine != null)
        {
            StopCoroutine(popCoroutine);
            popCoroutine = null;
        }
    }
}
