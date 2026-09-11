using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RTSController : MonoBehaviour
{
    [Header("Layer")]
    [SerializeField]
    private LayerMask unitLayer;

    [SerializeField]
    private LayerMask groundLayer;

    [Header("Selection")]
    [SerializeField]
    private float dragThreshold = 10f;

    [Header("Formation")]
    [SerializeField]
    private float formationSpacing = 2f;

    private Camera mainCamera;

    private List<RTSUnit> selectedUnits = new List<RTSUnit>();

    private Vector2 dragStartPosition;
    private Vector2 dragCurrentPosition;

    private bool isDragging;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        HandleSelection();

    }
    private void HandleSelection()
    {
        if (Input.GetMouseButtonDown(0))
        {
            dragStartPosition = Input.mousePosition;
            dragCurrentPosition = dragStartPosition;
            isDragging = false;
        }

        if (Input.GetMouseButton(0))
        {
            dragCurrentPosition = Input.mousePosition;

            float distance =
                Vector2.Distance(dragStartPosition, dragCurrentPosition);

            if (distance > dragThreshold)
            {
                isDragging = true;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (isDragging)
            {
                SelectUnitsInBox();
            }
            else
            {
                SelectSingleUnit();
            }

            isDragging = false;
        }
    }

    private void SelectSingleUnit()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit unitHit, 1000f, unitLayer))
        {
            RTSUnit unit = unitHit.collider.GetComponent<RTSUnit>();

            if (unit != null)
            {
                ClearSelection();

                selectedUnits.Add(unit);
                unit.Select();

                return;
            }
        }

        if (selectedUnits.Count > 0)
        {
            if (Physics.Raycast(ray, out RaycastHit groundHit, 1000f, groundLayer))
            {
                MoveFormation(groundHit.point);

                return;
            }
        }

        ClearSelection();
    }


    private void SelectUnitsInBox()
    {
        ClearSelection();

        Rect selectionRect =
            GetSelectionRect(
                dragStartPosition,
                dragCurrentPosition
            );

        RTSUnit[] allUnits =
            FindObjectsByType<RTSUnit>(
                FindObjectsSortMode.None
            );

        foreach (RTSUnit unit in allUnits)
        {
            Vector3 screenPosition =
                mainCamera.WorldToScreenPoint(
                    unit.transform.position
                );

            if (screenPosition.z < 0)
                continue;

            if (selectionRect.Contains(
                new Vector2(
                    screenPosition.x,
                    screenPosition.y
                )))
            {
                selectedUnits.Add(unit);
                unit.Select();
            }
        }
    }


    private void ClearSelection()
    {
        foreach (RTSUnit unit in selectedUnits)
        {
            if (unit != null)
            {
                unit.Deselect();
            }
        }

        selectedUnits.Clear();
    }


    private void MoveFormation(Vector3 center)
    {
        int unitCount = selectedUnits.Count;

        int columns =
            Mathf.CeilToInt(
                Mathf.Sqrt(unitCount)
            );

        int rows =
            Mathf.CeilToInt(
                (float)unitCount / columns
            );

        for (int i = 0; i < unitCount; i++)
        {
            int column = i % columns;
            int row = i / columns;

            float xOffset =
                (column - (columns - 1) / 2f)
                * formationSpacing;

            float zOffset =
                (row - (rows - 1) / 2f)
                * formationSpacing;

            Vector3 targetPosition =
                center +
                new Vector3(
                    xOffset,
                    0f,
                    zOffset
                );

            if (NavMesh.SamplePosition(
                targetPosition,
                out NavMeshHit navHit,
                3f,
                NavMesh.AllAreas))
            {
                selectedUnits[i]
                    .MoveTo(navHit.position);
            }
        }
    }


    private Rect GetSelectionRect(
        Vector2 start,
        Vector2 end)
    {
        float x =
            Mathf.Min(start.x, end.x);

        float y =
            Mathf.Min(start.y, end.y);

        float width =
            Mathf.Abs(start.x - end.x);

        float height =
            Mathf.Abs(start.y - end.y);

        return new Rect(
            x,
            y,
            width,
            height
        );
    }


    private void OnGUI()
    {
        if (!isDragging)
            return;

        Rect rect =
            GetSelectionRect(
                dragStartPosition,
                dragCurrentPosition
            );

        rect.y =
            Screen.height
            - rect.y
            - rect.height;

        GUI.Box(rect, "");
    }
}