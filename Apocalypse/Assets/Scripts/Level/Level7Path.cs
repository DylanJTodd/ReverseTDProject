using System.Collections;
using UnityEngine;

public class Level7Path : MonoBehaviour
{
    public int pathNumber;

    public AudioSource rockRaiseAudio;

    public GameObject sign;
    public GameObject path;
    public GameObject MonsterParent;

    public SignboardHandler raisingSignboard;
    public GameObject signParent;

    private Vector3 signOriginalPosition;
    private Vector3 pathOriginalPosition;
    private Vector3 signDownPosition;
    private Vector3 pathDownPosition;
    private Transform signOriginalParent;

    void Start()
    {
        signOriginalPosition = sign.transform.position;
        pathOriginalPosition = path.transform.position;
        signDownPosition = signOriginalPosition - new Vector3(0, 4, 0);
        pathDownPosition = pathOriginalPosition - new Vector3(0, 4, 0);

        signOriginalParent = sign.transform.parent;
        sign.transform.position = signDownPosition;

        StartCoroutine(PathAnimationCycle());
    }

    IEnumerator PathAnimationCycle()
    {
        while (true)
        {
            yield return new WaitForSeconds(30f);

            rockRaiseAudio.Play();
            raisingSignboard.ResetOrientation();
            sign.transform.SetParent(signParent.transform);
            yield return new WaitForSeconds(2f);
            yield return MoveBothToPosition(signOriginalPosition, pathDownPosition, 2.0f);

            yield return new WaitForSeconds(60f);

            sign.transform.SetParent(signOriginalParent);
            rockRaiseAudio.Play();
            yield return new WaitForSeconds(2f);
            KillMonstersOnPath();
            yield return MoveBothToPosition(signDownPosition, pathOriginalPosition, 2.0f);

            yield return new WaitForSeconds(1.0f);
        }
    }

    IEnumerator MoveBothToPosition(Vector3 signTarget, Vector3 pathTarget, float duration)
    {
        Vector3 signStart = sign.transform.position;
        Vector3 pathStart = path.transform.position;
        float timeElapsed = 0;

        while (timeElapsed < duration)
        {
            sign.transform.position = Vector3.Lerp(signStart, signTarget, timeElapsed / duration);
            path.transform.position = Vector3.Lerp(pathStart, pathTarget, timeElapsed / duration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        sign.transform.position = signTarget;
        path.transform.position = pathTarget;
    }

    void KillMonstersOnPath()
    {
        foreach (Transform monster in MonsterParent.transform)
        {
            MonsterMovement monsterMovement = monster.GetComponent<MonsterMovement>();
            if (monsterMovement != null)
            {
                monsterMovement.KillMonstersOnPath(pathNumber);
            }
        }
    }
}
