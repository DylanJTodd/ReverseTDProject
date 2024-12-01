using UnityEngine;

public class SignboardHandler : MonoBehaviour
{
    public int mainPath;
    public int altPath;
    private bool isFlipped = false;
    private LayerMask raycastLayerMask;

    public SignboardHandler child;

    // Store initial values
    private int initialMainPath;
    private int initialAltPath;

    private void Start()
    {
        int ignoreLayer = LayerMask.NameToLayer("Ignore Raycast");
        raycastLayerMask = ~(1 << ignoreLayer);

        // Store initial values
        initialMainPath = mainPath;
        initialAltPath = altPath;
    }

    public void Flip()
    {
        isFlipped = !isFlipped;

        Vector3 scale = transform.localScale;
        scale.z = -scale.z;
        transform.localScale = scale;

        if (child != null)
        {
            child.Flip();
        }
    }

    public int GetNextPath()
    {
        return isFlipped ? altPath : mainPath;
    }

    private void OnMouseDown()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, raycastLayerMask))
        {
            if (hit.transform == transform)
            {
                Flip();
            }
        }
    }

    public void ResetOrientation()
    {
        if (!isFlipped) 
        {
            Flip();
        }
    }
}
