using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;

public class CablesManager : MonoBehaviour
{
    [Header("Hovering")]
    public float maxDistance = 5f;

    [SerializeField]
    public GameObject lastHoveredObject;

    public CableConnectionType? pendingConnectionType = null;

    public Material lineMaterial;

    private bool lineDone = false;

    private LineRenderer line;
    private Interactable outletStart;
    private Interactable outletEnd;
    private InputActions inputActions;

    [SerializeField]
    private List<Vector3> points = new List<Vector3>();

    public static CablesManager Instance
    {
        get; private set;
    }

    public void OnEnable()
    {
        inputActions = new InputActions();
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }


    private void Awake()
    {
        if (Instance != null)
        {
            Debug.Log("There is more than one CablesManager instance");
        }
        Instance = this;

        line = GetComponent<LineRenderer>();
        if (line == null) line = gameObject.AddComponent<LineRenderer>();
        line.startWidth = 0.15f;
        line.endWidth = 0.15f;
        line.alignment = LineAlignment.View;
        line.textureMode = LineTextureMode.Static;
        line.material = lineMaterial;
        line.generateLightingData = true;
        line.numCornerVertices = 8;
    }

    void Update()
    {
        if (inputActions.Player.Attack.triggered)
        {
            OnLeftClick();
        }

        Ray rayCamera = Player.Instance.cameraPlayer.GetComponent<Camera>().ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        // Debug.DrawRay(rayCamera.origin, rayCamera.direction * 10f, Color.red);
        RaycastHit hitCamera;

        // Debug.DrawRay(ray.origin, ray.direction * 5f, Color.green);
        // Cast a ray forward from the camera
        if (Physics.Raycast(rayCamera, out hitCamera, maxDistance))
        {
            GameObject hitObj = hitCamera.collider.gameObject;

            // If new object hovered
            if (hitObj != lastHoveredObject)
            {
                // if previously hovering something, call OnHoverExit
                if (lastHoveredObject != null)
                {
                    Interactable prevHover = lastHoveredObject.GetComponent<Interactable>();
                    if (prevHover != null)
                        prevHover.OnHoverExit();
                }

                // call OnHoverEnter on the new one
                Interactable hover = hitObj.GetComponent<Interactable>();
                if (hover != null)
                {
                    hover.OnHoverEnter();
                }

                lastHoveredObject = hitObj;
            }
        }
        else
        {
            // if ray hits nothing, stop hovering
            if (lastHoveredObject != null)
            {
                Interactable prevHover = lastHoveredObject.GetComponent<Interactable>();
                if (prevHover != null)
                    prevHover.OnHoverExit();
                lastHoveredObject = null;
            }
        }

        if (points.Count == 0)
        {
            return;
        }

        if (lineDone)
        {
            line.positionCount = points.Count;
        }
        else
        {
            line.positionCount = points.Count + 1;
        }

        for (int i = 0; i < points.Count; i++)
        {
            line.SetPosition(i, points[i]);
        }


        if (!lineDone)
        {

            if (Physics.Raycast(rayCamera, out hitCamera, 10f))
            {
                line.SetPosition(points.Count, hitCamera.point + Vector3.up * 0.1f);
            }

        }

    }

    public void FixFinalLineDirection()
    {
        BatteryManager battery = outletEnd.GetComponentInParent<BatteryManager>();
        // final point is a battery
        if (battery != null)
        {
            points.Reverse();
            battery.SetEnergyInput(outletStart.GetComponent<OutletInteractable>());
            return;
        }

        battery = outletStart.GetComponentInParent<BatteryManager>();
        // start point is a battery, so swap the points order
        if (battery != null)
        {
            battery.SetEnergyInput(outletEnd.GetComponent<OutletInteractable>());
        }
    }

    public void OnLeftClick()
    {
        if (!lineDone)
        {
            Ray ray = new Ray(Player.Instance.cameraPlayer.transform.position, Player.Instance.cameraPlayer.transform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 10f))
            {
                GameObject hitObj = hit.collider.gameObject;

                // If new object hovered
                if (hitObj != null)
                {
                    Interactable outlet = hitObj.GetComponent<Interactable>();
                    if (outlet != null)
                    {
                        // ADD FINAL POINT
                        if (points.Count > 0)
                        {
                            points.Add(hitObj.transform.position);
                            lineDone = true;
                            outletEnd = outlet;
                            FixFinalLineDirection();
                            return;
                        }

                        // ADD FIRST POINT
                        outletStart = outlet;
                        points.Add(hitObj.transform.position);
                    }
                    else
                    {
                        // ADD MIDDLE POINTS
                        if (points.Count > 0)
                        {
                            points.Add(hit.point + Vector3.up * 0.1f);
                        }
                    }
                }
            }
        }
    }
}
