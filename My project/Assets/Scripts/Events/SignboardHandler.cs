using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SignboardHandler : MonoBehaviour
{
    public int mainPath;
    public int altPath;
    private bool isFlipped = false;

    public void Flip()
    {
        isFlipped = !isFlipped;

        Vector3 scale = transform.localScale;
        scale.z = -scale.z;
        transform.localScale = scale;
    }

    public int GetNextPath()
    {
        return isFlipped ? altPath : mainPath;
    }
    private void OnMouseDown()
    {
        Flip();
        Debug.Log("Signboard flipped!");
    }
}
