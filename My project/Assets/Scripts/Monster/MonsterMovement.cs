using System.Collections.Generic;
using UnityEngine;

public class MonsterMovement : MonoBehaviour
{
    private float interpolate;
    private Monster myMonster;
    private float movementSpeed;
    private Transform currentStartPoint;
    private Transform currentEndPoint;
    private int currentPathNumber = 1; // Start from path 1
    public int endPathNumber; // Indicates the final path

    public void Initialize(float interpolateValue, Monster monster, int endPath)
    {
        interpolate = interpolateValue;
        myMonster = monster;
        movementSpeed = myMonster.movementSpeed;
        endPathNumber = endPath;

        InitializeWayline(currentPathNumber); // Initialize first wayline
    }

    void InitializeWayline(int pathNumber)
    {
        GameObject floor = GameObject.Find("Floor");
        Transform pathParent = floor.transform.Find("Path");

        Transform path = pathParent.Find(pathNumber.ToString());
        if (path != null)
        {
            currentStartPoint = path.Find("Start");
            currentEndPoint = path.Find("End");
        }
    }

    public void OnMouseDown()
    {
        CameraController.instance.followTransform = transform;
    }

    void Update()
    {
        if (currentEndPoint != null)
        {
            MoveAlongWayline();
        }
    }

    // Move the monster along the current wayline
    void MoveAlongWayline()
    {
        Vector3 targetPosition = Vector3.Lerp(currentStartPoint.position, currentEndPoint.position, interpolate);
        RotateTowards(targetPosition);
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, movementSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            if (currentPathNumber == endPathNumber)
            {
                Destroy(gameObject);
            }
            else
            {
                if (!CheckForSignboard()) 
                {
                    string nextPathName = (currentPathNumber + 1).ToString();
                    GameObject nextPath = GameObject.Find(nextPathName);

                    if (nextPath == null || nextPath.transform.parent == null || nextPath.transform.parent.name != "Path")
                    {
                        currentPathNumber = endPathNumber;
                        InitializeWayline(currentPathNumber);
                    }
                    else
                    {
                        currentPathNumber++;
                        InitializeWayline(currentPathNumber);
                    }

                }
            }
        }
    }

    bool CheckForSignboard()
    {
        GameObject pathObject = GameObject.Find(currentPathNumber.ToString());
        if (pathObject != null)
        {
            SignboardHandler signboard = pathObject.GetComponentInChildren<SignboardHandler>();
            if (signboard != null)
            {
                currentPathNumber = signboard.GetNextPath();
                InitializeWayline(currentPathNumber);
                return true;
            }
        }
        return false;
    }

    void RotateTowards(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * (movementSpeed * 4));
    }
}