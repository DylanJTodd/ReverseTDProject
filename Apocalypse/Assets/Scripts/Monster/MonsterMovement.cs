using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class MonsterMovement : MonoBehaviour
{
    private float interpolate;
    private Monster myMonster;
    private float movementSpeed;
    private Transform currentStartPoint;
    private Transform currentEndPoint;
    public int currentPathNumber = 1;
    public int endPathNumber;
    public CastleHealth castleHealth;

    public void Initialize(float interpolateValue, GameObject monsterObj, int endPath)
    {
        interpolate = interpolateValue;

        Monster monster = monsterObj.GetComponent<Monster>();
        myMonster = monster;
        movementSpeed = myMonster.movementSpeed;
        endPathNumber = endPath;

        InitializeWayline(currentPathNumber);
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
        movementSpeed = myMonster.movementSpeed;
        if (currentEndPoint != null)
        {
            MoveAlongWayline();
        }
    }

    void MoveAlongWayline()
    {
        movementSpeed = myMonster.movementSpeed;
        Vector3 targetPosition = Vector3.Lerp(currentStartPoint.position, currentEndPoint.position, interpolate);
        RotateTowards(targetPosition);
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, movementSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            if (currentPathNumber == endPathNumber)
            {
                castleHealth.RemoveHealth((int)((myMonster.damage) * (myMonster.currentHealth / myMonster.health)));
                Destroy(gameObject);
            }
            else
            {
                if (!CheckForSignboard())
                {
                    string nextPathName = (currentPathNumber + 1).ToString();
                    Transform nextPath = GameObject.Find("Path").transform.Find(nextPathName.ToString());

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
        Transform pathObject = GameObject.Find("Path").transform.Find(currentPathNumber.ToString());
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

    public void KillMonstersOnPath(int pathNumber)
    {

        if (pathNumber < 0)
        {
            pathNumber *= -1;
            if (currentPathNumber >= pathNumber)
            {
                Destroy(gameObject);
            }                
        }
        if (currentPathNumber == pathNumber)
        {
            Destroy(gameObject);
        }
    }
}
