using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSystemTest : MonoBehaviour
{
    [SerializeField] private Unit unit;

    private GridSystem<GridObject> gridSystem;

    private void Start()
    {
        // gridSystem = new GridSystem(10, 10, 2f);
        // gridSystem.CreateDebugObjects(gridDebugObjectPrefab);
    }

    private void Update()
    {
        /*
        if (Input.GetKeyDown(KeyCode.T))
        {
            GridSystemVisual.Instance.HideAllGridPosition();
            GridSystemVisual.Instance.ShowGridPositionList(unit.GetAction<MoveAction>().GetValidActionGridPositionList());
        }
        */
    }
}
