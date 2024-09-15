using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragAndDrop : MonoBehaviour
{
    private Vector3 initialPosition;
    private Transform initialParent;
    private bool isDragging = false;

    
    public float snapDistance = 1.0f;

    
    public List<Transform> snapPoints;

    
    public Spawner spawner;

    
    public bool isFromFrontPosition;

    
    public bool canDragToBack = true;

    
    private Transform currentSnapPoint;

    
    public string objectTag;

    
    private Animator animator;

    private void Start()
    {
        initialPosition = transform.position;
        initialParent = transform.parent;
        objectTag = gameObject.tag; 

        
        animator = GetComponent<Animator>();
    }

    private void OnMouseDown()
    {
        
        if (isFromFrontPosition || canDragToBack)
        {
            isDragging = true;

            
            if (animator != null)
            {
                animator.SetBool("isDragging", true); 
            }
        }
    }

    private void OnMouseDrag()
    {
        
        if (isDragging)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0f;
            transform.position = mousePosition;
        }
    }

    private void OnMouseUp()
    {
        
        if (isDragging)
        {
            isDragging = false;

            
            if (animator != null)
            {
                animator.SetBool("isDragging", false); 
            }

            Transform closestSnapPoint = null;
            float closestDistance = Mathf.Infinity;

            foreach (Transform snapPoint in snapPoints)
            {
                float distanceToSnapPoint = Vector3.Distance(transform.position, snapPoint.position);

                if (distanceToSnapPoint < closestDistance)
                {
                    closestDistance = distanceToSnapPoint;
                    closestSnapPoint = snapPoint;
                }
            }

            
            if (closestDistance <= snapDistance && closestSnapPoint != null)
            {
                if (closestSnapPoint.childCount == 0 || closestSnapPoint == currentSnapPoint)
                {
                    
                    if (canDragToBack || IsFrontSnapPoint(closestSnapPoint))
                    {
                        
                        transform.position = closestSnapPoint.position;
                        transform.parent = closestSnapPoint;

                        
                        initialPosition = closestSnapPoint.position;
                        initialParent = closestSnapPoint;

                        
                        if (spawner != null)
                        {
                            spawner.UpdateFrontPositionOccupied();
                            spawner.CheckFrontPositionsForMatchingTag();
                            spawner.MoveBackToFrontIfFrontEmpty(); 
                        }
                    }
                    else
                    {
                       
                        transform.position = initialPosition;
                        transform.parent = initialParent;
                    }
                }
                else
                {
                    
                    transform.position = initialPosition;
                    transform.parent = initialParent;
                }
            }
            else
            {
               
                transform.position = initialPosition;
                transform.parent = initialParent;
            }
        }
    }

   
    private bool IsFrontSnapPoint(Transform snapPoint)
    {
        foreach (Shelf shelf in spawner.shelves)
        {
            if (System.Array.Exists(shelf.frontPositions, position => position == snapPoint))
            {
                return true;
            }
        }
        return false;
    }
}
