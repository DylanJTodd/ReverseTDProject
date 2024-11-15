using UnityEngine;
using UnityEngine.UI;

public class CameraResetButton : MonoBehaviour
{
    private Button button;
    private CameraController cameraController;

    void Start()
    {
        button = GetComponent<Button>();
        
        // Find CameraRig and get its CameraController
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            Transform cameraRig = mainCamera.transform.parent;
            if (cameraRig != null)
            {
                cameraController = cameraRig.GetComponent<CameraController>();
            }
        }

        if (cameraController == null)
        {
            Debug.LogError("CameraController not found on CameraRig");
            button.interactable = false;
            return;
        }

        if (button != null)
        {
            button.onClick.AddListener(OnButtonClick);
        }
    }

    private void OnButtonClick()
    {
        cameraController.ResetCameraPosition();
    }

    private void OnDestroy()
    {
        button.onClick.RemoveListener(OnButtonClick);
    }
}
