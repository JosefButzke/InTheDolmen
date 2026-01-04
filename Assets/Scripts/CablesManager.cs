using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;

public class CablesManager : MonoBehaviour
{
    public CableConnectionType? pendingConnectionType = null;

    public GameObject interactableManager;
    public Material lineMaterial;

    private bool lineDone = false;

    private LineRenderer line;
    private Interactable outletStart;
    private Interactable outletEnd;

    [SerializeField]
    private List<Vector3> points = new List<Vector3>();

    public static CablesManager Instance
    {
        get; private set;
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

    public void Enable()
    {
        interactableManager.SetActive(true);
    }

    public void Unable()
    {
        interactableManager.SetActive(false);
    }

    void Update()
    {
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
            Ray ray = new Ray(Player.Instance.cameraPlayer.transform.position, Player.Instance.cameraPlayer.transform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 10f))
            {
                line.SetPosition(points.Count, hit.point + Vector3.up * 0.1f);
            }

        }
    }

    public void FixFinalLineDirection()
    {
        Battery battery = outletEnd.GetComponentInParent<Battery>();
        // final point is a battery
        if (battery != null)
        {
            points.Reverse();
            battery.SetEnergyInput(outletStart.GetComponent<OutletInteractable>());
            return;
        }

        battery = outletStart.GetComponentInParent<Battery>();
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
