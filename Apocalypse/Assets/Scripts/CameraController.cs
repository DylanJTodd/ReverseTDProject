using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class CameraController : MonoBehaviour
{
    public LayerMask uiLayerMask;
    public static CameraController instance;
    public Transform followTransform;
    public Transform cameraTransform;

    [Header("Movement Settings")]
    public float movementSpeed = 10f;
    public float movementTime = 5f;
    public float rotationAmount = 1f;
    public Vector3 zoomAmount = new Vector3(0, -5, 5);

    [Header("Zoom Limits")]
    public float minZoom = 10f;
    public float maxZoom = 100f;
    public float soundTriggerZoom = 30f;

    [Header("Sound Settings")]
    public float monsterSoundRadius = 10f;
    public float soundCheckInterval = 0.5f;
    public int maxSimultaneousSounds = 5;
    public float minVolume = 0.1f;
    public float maxVolume = 1f;

    [Header("Boundary Settings")]
    public float boundaryWidth = 500f;

    [Header("Monster Display")]
    public MonsterDisplayHandler monsterDisplayHandler;

    private Vector3 newPosition;
    private Quaternion newRotation;
    private Vector3 newZoom;
    private Vector3 dragStartPosition;
    private Vector3 dragCurrentPosition;
    private Vector3 rotateStartPosition;
    private Vector3 rotateCurrentPosition;
    private float currentZoomMagnitude;
    private float nextSoundCheck;

    private Dictionary<Monster, AudioSource> activeAudioSources = new Dictionary<Monster, AudioSource>();
    private Queue<AudioSource> audioSourcePool = new Queue<AudioSource>();

    private void Start()
    {
        instance = this;
        InitializeValues();
        InitializeAudioPool();
    }

    private void InitializeValues()
    {
        newPosition = transform.position;
        newRotation = transform.rotation;
        newZoom = cameraTransform.localPosition;
        currentZoomMagnitude = newZoom.magnitude;
        nextSoundCheck = Time.time;

        if (monsterDisplayHandler == null)
        {
            monsterDisplayHandler = GameObject.Find("MonsterDisplay").GetComponent<MonsterDisplayHandler>();
        }
    }

    private void InitializeAudioPool()
    {
        GameObject audioPool = new GameObject("AudioSourcePool");
        audioPool.transform.parent = transform;

        for (int i = 0; i < maxSimultaneousSounds; i++)
        {
            GameObject audioObj = new GameObject($"AudioSource_{i}");
            audioObj.transform.parent = audioPool.transform;
            AudioSource source = audioObj.AddComponent<AudioSource>();
            source.spatialBlend = 1f;
            source.loop = true;
            source.playOnAwake = false;
            audioSourcePool.Enqueue(source);
        }
    }

    void LateUpdate()
    {
        if (followTransform != null)
        {
            HandleTrackingMode();
        }
        else
        {
            HandleFreeMode();
        }

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, ~uiLayerMask))
            {
                if (hit.transform.CompareTag("Monster"))
                {
                    FocusOnMonster(hit.transform);
                }
                else
                {
                    followTransform = null;
                    monsterDisplayHandler.HideMonsterDisplay();
                }
            }
        }

        ApplyTransformation();
        UpdateMonsterSounds();
    }

    void UpdateMonsterSounds()
    {
        if (Time.time < nextSoundCheck) return;
        nextSoundCheck = Time.time + soundCheckInterval;

        UpdateExistingAudioSources();
        CheckForNewMonsterSounds();
    }

    void UpdateExistingAudioSources()
    {
        List<Monster> monstersToRemove = new List<Monster>();

        foreach (var kvp in activeAudioSources)
        {
            Monster monster = kvp.Key;
            AudioSource source = kvp.Value;

            if (monster == null || !IsMonsterInRange(monster.transform.position))
            {
                StopMonsterSound(monster);
                monstersToRemove.Add(monster);
                continue;
            }

            UpdateAudioSourceVolume(source, monster.transform.position);
        }

        foreach (var monster in monstersToRemove)
        {
            activeAudioSources.Remove(monster);
        }
    }

    void CheckForNewMonsterSounds()
    {
        if (currentZoomMagnitude > soundTriggerZoom) return;

        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, monsterSoundRadius);

        foreach (Collider collider in nearbyColliders)
        {
            if (!collider.CompareTag("Monster")) continue;

            Monster monster = collider.GetComponent<Monster>();
            if (monster == null || monster.movementSound == null || activeAudioSources.ContainsKey(monster)) continue;

            StartMonsterSound(monster);
        }
    }

    bool IsMonsterInRange(Vector3 monsterPosition)
    {
        return Vector3.Distance(transform.position, monsterPosition) <= monsterSoundRadius;
    }

    void StartMonsterSound(Monster monster)
    {
        if (audioSourcePool.Count == 0) return;

        AudioSource source = audioSourcePool.Dequeue();
        source.clip = monster.movementSound;
        source.transform.position = monster.transform.position;
        UpdateAudioSourceVolume(source, monster.transform.position);
        source.Play();

        activeAudioSources[monster] = source;
    }

    void StopMonsterSound(Monster monster)
    {
        if (!activeAudioSources.TryGetValue(monster, out AudioSource source)) return;

        source.Stop();
        source.clip = null;
        audioSourcePool.Enqueue(source);
    }

    void UpdateAudioSourceVolume(AudioSource source, Vector3 position)
    {
        float zoomPercentage = 1 - ((currentZoomMagnitude - minZoom) / (soundTriggerZoom - minZoom));
        float distancePercentage = 1 - (Vector3.Distance(transform.position, position) / monsterSoundRadius);

        float volume = Mathf.Lerp(minVolume, maxVolume, zoomPercentage * distancePercentage);
        source.volume = Mathf.Clamp01(volume);
        source.transform.position = position;
    }

    void HandleTrackingMode()
    {
        newPosition = followTransform.position;
        HandleZooming();
        HandleRotation();

        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if ((uiLayerMask & (1 << hit.transform.gameObject.layer)) != 0)
                {
                    return;
                }

                if (hit.transform != followTransform)
                {
                    ExitTrackingMode();
                }
            }
            ExitTrackingMode();
        }
    }

    void ExitTrackingMode()
    {
        followTransform = null;
        monsterDisplayHandler.HideMonsterDisplay();
        newPosition = transform.position;

        Plane plane = new Plane(Vector3.up, Vector3.zero);
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float entry;
        if (plane.Raycast(ray, out entry))
        {
            dragStartPosition = ray.GetPoint(entry);
            dragCurrentPosition = dragStartPosition;
        }
    }

    void HandleFreeMode()
    {
        HandleMouseInput();
        HandleZooming();
    }

    void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Plane plane = new Plane(Vector3.up, Vector3.zero);
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            float entry;

            if (plane.Raycast(ray, out entry))
            {
                dragStartPosition = ray.GetPoint(entry);
            }
        }

        if (Input.GetMouseButton(0))
        {
            Plane plane = new Plane(Vector3.up, Vector3.zero);
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            float entry;

            if (plane.Raycast(ray, out entry))
            {
                dragCurrentPosition = ray.GetPoint(entry);
                Vector3 dragDelta = dragStartPosition - dragCurrentPosition;
                float zoomFactor = Mathf.Log(currentZoomMagnitude, 2) * 0.2f;
                newPosition = transform.position + dragDelta * zoomFactor;
                newPosition = ClampPositionToBoundary(newPosition);
            }
        }

        HandleRotation();
    }

    void HandleZooming()
    {
        if (Input.mouseScrollDelta.y != 0)
        {
            float zoomFactor = 1f - Input.mouseScrollDelta.y * 0.1f;
            Vector3 zoomDelta = Vector3.Scale(newZoom, new Vector3(zoomFactor, zoomFactor, zoomFactor)) - newZoom;
            float zoomSpeed = Mathf.Log(currentZoomMagnitude, 2) * 0.2f;
            newZoom += zoomDelta * zoomSpeed;

            currentZoomMagnitude = newZoom.magnitude;
            currentZoomMagnitude = Mathf.Clamp(currentZoomMagnitude, minZoom, maxZoom);
            newZoom = newZoom.normalized * currentZoomMagnitude;
        }
    }

    void HandleRotation()
    {
        if (Input.GetMouseButtonDown(1))
        {
            rotateStartPosition = Input.mousePosition;
        }

        if (Input.GetMouseButton(1))
        {
            rotateCurrentPosition = Input.mousePosition;
            Vector3 difference = rotateStartPosition - rotateCurrentPosition;
            rotateStartPosition = rotateCurrentPosition;
            newRotation *= Quaternion.Euler(Vector3.up * (-difference.x / 5f));
        }
    }

    Vector3 ClampPositionToBoundary(Vector3 position)
    {
        float halfWidth = boundaryWidth * 0.5f;
        return new Vector3(
            Mathf.Clamp(position.x, -halfWidth, halfWidth),
            position.y,
            Mathf.Clamp(position.z, -halfWidth, halfWidth)
        );
    }

    void ApplyTransformation()
    {
        transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * movementTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, Time.deltaTime * movementTime);
        cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, newZoom, Time.deltaTime * movementTime);
    }

    public void FocusOnMonster(Transform monster)
    {
        followTransform = monster;
        Monster myMonster = monster.GetComponent<Monster>();
        monsterDisplayHandler.ShowMonsterDisplay(myMonster);
    }
}