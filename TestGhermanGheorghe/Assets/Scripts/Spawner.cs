using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Shelf
{
    public Transform[] backPositions;  
    public Transform[] frontPositions; 
    public bool[] frontPositionOccupied; 
    public HashSet<string> usedPrefabTags = new HashSet<string>(); 
}

[System.Serializable]
public class PrefabData
{
    public GameObject prefab;  
    public int spawnCount;     
}

public class Spawner : MonoBehaviour
{
    public PrefabData[] prefabDataArray;
    public Shelf[] shelves;
    public int frontFreeCount;  

    void Start()
    {
        foreach (Shelf shelf in shelves)
        {
            foreach (Transform frontPosition in shelf.frontPositions)
            {
                SnapManager.Instance.RegisterSnapPoint(frontPosition);
            }

            shelf.frontPositionOccupied = new bool[shelf.frontPositions.Length];
        }

        SpawnPrefabs();
    }

    void SpawnPrefabs()
    {
        
        List<int> freeFrontPositions = new List<int>();

        
        int totalFrontPositions = shelves.Length * shelves[0].frontPositions.Length;
        int totalFreeFrontPositions = Mathf.Min(totalFrontPositions, frontFreeCount);

        
        for (int i = 0; i < totalFreeFrontPositions; i++)
        {
            int randomFreeFrontPosition = Random.Range(0, totalFrontPositions);
            while (freeFrontPositions.Contains(randomFreeFrontPosition))
            {
                randomFreeFrontPosition = Random.Range(0, totalFrontPositions);
            }
            freeFrontPositions.Add(randomFreeFrontPosition);
        }

        
        foreach (PrefabData prefabData in prefabDataArray)
        {
            for (int i = 0; i < prefabData.spawnCount; i++)
            {
                bool spawnInBack = false;

                
                Transform position = null;
                Shelf selectedShelf = null;

                foreach (Shelf shelf in shelves)
                {
                    
                    if (shelf.usedPrefabTags.Contains(prefabData.prefab.tag)) continue;

                    for (int j = 0; j < shelf.frontPositions.Length; j++)
                    {
                        int globalFrontIndex = System.Array.IndexOf(shelves, shelf) * shelf.frontPositions.Length + j;

                        
                        if (!shelf.frontPositionOccupied[j] && !freeFrontPositions.Contains(globalFrontIndex))
                        {
                            position = shelf.frontPositions[j];
                            selectedShelf = shelf;
                            break;
                        }
                    }
                    if (position != null) break;
                }

                if (position == null)
                {
                    
                    spawnInBack = true;
                    int randomShelfIndex = Random.Range(0, shelves.Length);
                    Shelf shelf = shelves[randomShelfIndex];

                    if (shelf.backPositions.Length > 0)
                    {
                        int randomBackIndex = Random.Range(0, shelf.backPositions.Length);
                        position = shelf.backPositions[randomBackIndex];
                        selectedShelf = shelf;
                    }
                }

                if (position != null && selectedShelf != null)
                {
                    GameObject spawnedPrefab = Instantiate(prefabData.prefab, position.position, Quaternion.identity, position);
                    spawnedPrefab.transform.localScale = Vector3.one * 0.8f;

                    SpriteRenderer spriteRenderer = spawnedPrefab.GetComponent<SpriteRenderer>();
                    if (spriteRenderer != null)
                    {
                        
                        spriteRenderer.sortingOrder = spawnInBack ? 2 : 4;

                        
                        spriteRenderer.color = spawnInBack ? new Color(0.5f, 0.5f, 0.5f, 1f) : Color.white;
                    }

                    DragAndDrop dragAndDrop = spawnedPrefab.GetComponent<DragAndDrop>();
                    if (dragAndDrop != null)
                    {
                        dragAndDrop.snapPoints = SnapManager.Instance.snapPoints;
                        dragAndDrop.spawner = this;
                        dragAndDrop.isFromFrontPosition = !spawnInBack;
                        dragAndDrop.objectTag = prefabData.prefab.tag;

                        
                        dragAndDrop.canDragToBack = false;

                        
                        Collider2D collider = spawnedPrefab.GetComponent<Collider2D>();
                        if (collider != null)
                        {
                            collider.enabled = !spawnInBack;
                        }

                        
                        if (!spawnInBack)
                        {
                            int frontPositionIndex = System.Array.IndexOf(selectedShelf.frontPositions, position);
                            if (frontPositionIndex >= 0)
                            {
                                selectedShelf.frontPositionOccupied[frontPositionIndex] = true;
                            }

                            
                            selectedShelf.usedPrefabTags.Add(prefabData.prefab.tag);
                        }
                    }
                }
            }
        }

        
        MoveBackToFrontIfFrontEmpty();
    }

    public void MoveBackToFrontIfFrontEmpty()
    {
        foreach (Shelf shelf in shelves)
        {
            bool allFrontEmpty = true;

            for (int i = 0; i < shelf.frontPositions.Length; i++)
            {
                if (shelf.frontPositions[i].childCount > 0)
                {
                    allFrontEmpty = false;
                    break;
                }
            }

            if (allFrontEmpty)
            {
                for (int i = 0; i < shelf.backPositions.Length; i++)
                {
                    Transform backPosition = shelf.backPositions[i];
                    if (backPosition.childCount > 0)
                    {
                        Transform frontPosition = shelf.frontPositions[i];
                        if (frontPosition.childCount == 0)
                        {
                            GameObject backObject = backPosition.GetChild(0).gameObject;
                            backObject.transform.SetParent(frontPosition);
                            backObject.transform.position = frontPosition.position;

                            Collider2D collider = backObject.GetComponent<Collider2D>();
                            if (collider != null)
                            {
                                collider.enabled = true;
                            }

                            int frontPositionIndex = System.Array.IndexOf(shelf.frontPositions, frontPosition);
                            if (frontPositionIndex >= 0)
                            {
                                shelf.frontPositionOccupied[frontPositionIndex] = true;
                            }

                            DragAndDrop dragAndDrop = backObject.GetComponent<DragAndDrop>();
                            if (dragAndDrop != null)
                            {
                                dragAndDrop.isFromFrontPosition = true;
                                
                                dragAndDrop.canDragToBack = false;
                            }

                            
                            SpriteRenderer spriteRenderer = backObject.GetComponent<SpriteRenderer>();
                            if (spriteRenderer != null)
                            {
                                UpdateObjectColorAndSorting(spriteRenderer, 0); 
                            }
                        }
                    }
                }
                UpdateFrontPositionOccupied();
            }
        }
    }

    public void UpdateFrontPositionOccupied()
    {
        foreach (Shelf shelf in shelves)
        {
            for (int i = 0; i < shelf.frontPositions.Length; i++)
            {
                if (shelf.frontPositions[i].childCount > 0)
                {
                    shelf.frontPositionOccupied[i] = true;
                }
                else
                {
                    shelf.frontPositionOccupied[i] = false;
                }
            }
        }
    }

    public void CheckFrontPositionsForMatchingTag()
    {
        foreach (Shelf shelf in shelves)
        {
            bool allSameTag = true;
            string tag = null;

            for (int i = 0; i < shelf.frontPositions.Length; i++)
            {
                Transform frontPosition = shelf.frontPositions[i];
                if (frontPosition.childCount > 0)
                {
                    GameObject child = frontPosition.GetChild(0).gameObject;
                    if (tag == null)
                    {
                        tag = child.tag;
                    }
                    else if (child.tag != tag)
                    {
                        allSameTag = false;
                        break;
                    }
                }
                else
                {
                    allSameTag = false;
                    break;
                }
            }

            if (allSameTag && tag != null)
            {
                List<int> emptyFrontPositions = new List<int>();
                StartCoroutine(DestroyFrontObjectsWithDelay(shelf, emptyFrontPositions));
                StartCoroutine(MoveBackToFrontWithDelay(shelf, emptyFrontPositions));
                
                shelf.usedPrefabTags.Clear();
                GameManager.Instance.AddScore(1);
            }
        }
    }

    IEnumerator DestroyFrontObjectsWithDelay(Shelf shelf, List<int> emptyFrontPositions)
    {
        yield return new WaitForSeconds(0.5f);
        for (int i = 0; i < shelf.frontPositions.Length; i++)
        {
            Transform frontPosition = shelf.frontPositions[i];
            if (frontPosition.childCount > 0)
            {
                Destroy(frontPosition.GetChild(0).gameObject);
                emptyFrontPositions.Add(i);
                shelf.frontPositionOccupied[i] = false;
            }
        }
    }

    IEnumerator MoveBackToFrontWithDelay(Shelf shelf, List<int> emptyFrontPositions)
    {
        yield return new WaitForSeconds(0.5f);
        for (int i = 0; i < emptyFrontPositions.Count; i++)
        {
            int emptyIndex = emptyFrontPositions[i];
            if (emptyIndex >= 0 && emptyIndex < shelf.backPositions.Length)
            {
                Transform backPosition = shelf.backPositions[emptyIndex];
                if (backPosition.childCount > 0)
                {
                    Transform frontPosition = shelf.frontPositions[emptyIndex];
                    GameObject backObject = backPosition.GetChild(0).gameObject;
                    backObject.transform.SetParent(frontPosition);
                    backObject.transform.position = frontPosition.position;

                    Collider2D collider = backObject.GetComponent<Collider2D>();
                    if (collider != null)
                    {
                        collider.enabled = true;
                    }

                    SpriteRenderer spriteRenderer = backObject.GetComponent<SpriteRenderer>();
                    if (spriteRenderer != null)
                    {
                        UpdateObjectColorAndSorting(spriteRenderer, emptyIndex);
                    }

                    DragAndDrop dragAndDrop = backObject.GetComponent<DragAndDrop>();
                    if (dragAndDrop != null)
                    {
                        dragAndDrop.isFromFrontPosition = true;
                        dragAndDrop.canDragToBack = false;
                    }
                    shelf.frontPositionOccupied[emptyIndex] = true;
                }
            }
        }
    }

    public void UpdateObjectColorAndSorting(SpriteRenderer spriteRenderer, int index)
    {
        spriteRenderer.color = Color.white;
        spriteRenderer.sortingOrder = 4;
    }
}
