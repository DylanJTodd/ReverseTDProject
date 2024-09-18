using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterMovement : MonoBehaviour
{
    private float interpolate;
    private Monster myMonster;
    private float movementSpeed;
    private List<Transform[]> waylines;
    private int currentWaylineIndex = 0;

    public void Initialize(float interpolateValue, Monster monster)
    {
        interpolate = interpolateValue;
        myMonster = monster;
        movementSpeed = myMonster.movementSpeed;

        InitializeWaylines();
    }

    void InitializeWaylines()
    {
        waylines = new List<Transform[]>();

        GameObject floor = GameObject.Find("Floor");
        Transform pathParent = floor.transform.Find("Path");

        foreach (Transform path in pathParent)
        {
            Transform startPoint = path.Find("Start");
            Transform endPoint = path.Find("End");

            waylines.Add(new Transform[] { startPoint, endPoint });
        }
    }

    void Update()
    {
        if (currentWaylineIndex < waylines.Count)
        {
            MoveAlongWayline();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Move the monster along the current wayline
    void MoveAlongWayline()
    {
        Transform startPoint = waylines[currentWaylineIndex][0];
        Transform endPoint = waylines[currentWaylineIndex][1];

        Vector3 targetPosition = Vector3.Lerp(startPoint.position, endPoint.position, interpolate);

        RotateTowards(targetPosition);
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, movementSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            currentWaylineIndex++;
        }
    }

    // Rotate the monster to face the target position
    void RotateTowards(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * (movementSpeed*4));
    }
}
