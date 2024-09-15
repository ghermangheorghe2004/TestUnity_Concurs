using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SnapManager : MonoBehaviour
{
    public static SnapManager Instance { get; private set; }

    public List<Transform> snapPoints = new List<Transform>();

    private void Awake()
    {
        
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        
        snapPoints.Clear();
    }

    public void RegisterSnapPoint(Transform snapPoint)
    {
        if (!snapPoints.Contains(snapPoint))
        {
            snapPoints.Add(snapPoint);
        }
    }

    public void UnregisterSnapPoint(Transform snapPoint)
    {
        if (snapPoints.Contains(snapPoint))
        {
            snapPoints.Remove(snapPoint);
        }
    }
}
