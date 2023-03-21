using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSystemVisual : MonoBehaviour
{
    public static GridSystemVisual Instance { get; private set; }

    [Serializable]
    public struct GridVisualColorMaterial
    {
        public GridVisualColor gridVisualColor;
        public Material material;
    }

    public enum GridVisualColor
    {
        White,
        Blue,
        Red,
        Yellow
    }

    [SerializeField] private Transform gridSystemVisualSinglePrefab;
    [SerializeField] private List<GridVisualColorMaterial> gridVisualColorMaterialList;
    
    private GridSystemVisualSingle[,] gridSystemVisualSingleArray;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError($"Only a single instance of GridSystemVisual can exist at a time. ({transform} - {Instance})");
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start() 
    {
        gridSystemVisualSingleArray = new GridSystemVisualSingle[
            LevelGrid.Instance.GetWidth(),
            LevelGrid.Instance.GetHeight()
        ];
        for (int x = 0; x < LevelGrid.Instance.GetWidth(); x++)
        {
            for (int z = 0; z < LevelGrid.Instance.GetHeight(); z++)
            {
                GridPosition gridPosition = new GridPosition(x, z);
                Transform gridSystemVisualSingleTransform = Instantiate(
                    gridSystemVisualSinglePrefab, 
                    LevelGrid.Instance.GetWorldPosition(gridPosition), 
                    Quaternion.identity
                );
                gridSystemVisualSingleArray[x, z] = gridSystemVisualSingleTransform.GetComponent<GridSystemVisualSingle>();
            }
        }
        
        UnitActionSystem.Instance.OnSelectedActionChanged += UnitActionSystem_OnSelectedActionChanged;
        LevelGrid.Instance.OnAnyUnitMovedGridPosition += LevelGrid_OnAnyUnitMovedGridPosition;
        UpdateGridVisual();
    }

    public void HideAllGridPosition()
    {
        for (int x = 0; x < LevelGrid.Instance.GetWidth(); x++)
        {
            for (int z = 0; z < LevelGrid.Instance.GetHeight(); z++)
            {
                gridSystemVisualSingleArray[x, z].Hide();
            }
        }
    }

    private void ShowGridPoisitionRange(GridPosition gridPosition, int range, GridVisualColor gridVisualColor)
    {
        List<GridPosition> gridPositionList = new List<GridPosition>();
        for (int x = -range; x <= range; x++)
        {
            for (int z = -range; z <= range; z++)
            {
                GridPosition testGridPosition = gridPosition + new GridPosition(x, z);
                if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition)) continue;
                int testDistance = Mathf.Abs(x) + Mathf.Abs(z);
                if (testDistance > range) continue;
                gridPositionList.Add(testGridPosition);
            }
        }
        ShowGridPositionList(gridPositionList, gridVisualColor);
    }

    public void ShowGridPositionList(List<GridPosition> gridPositionList, GridVisualColor gridVisualColor)
    {
        foreach (GridPosition gridPosition in gridPositionList)
        {
            gridSystemVisualSingleArray[gridPosition.x, gridPosition.z].Show(GetGridVisualColor(gridVisualColor));
        }
    }

    private void UpdateGridVisual()
    {
        HideAllGridPosition();

        Unit selectedUnit = UnitActionSystem.Instance.GetSelectedUnit();
        BaseAction selectedAction = UnitActionSystem.Instance.GetSelectedAction();
        if (selectedAction == null) return;

        GridVisualColor gridVisualColor = GridVisualColor.White;
        switch (selectedAction)
        {
            case MoveAction moveAction:
                gridVisualColor = GridVisualColor.White;
                break;
            case ShootAction shootAction:
                gridVisualColor = GridVisualColor.Red;
                ShowGridPoisitionRange(selectedUnit.GetGridPosition(), shootAction.GetMaxShootDistance(), GridVisualColor.Yellow);
                break;
        }
        ShowGridPositionList(selectedAction.GetValidActionGridPositionList(), gridVisualColor);
    }

    private void UnitActionSystem_OnSelectedActionChanged(object sender, EventArgs e)
    {
        UpdateGridVisual();
    }

    private void LevelGrid_OnAnyUnitMovedGridPosition(object sender, EventArgs e)
    {
        UpdateGridVisual();
    }

    private Material GetGridVisualColor(GridVisualColor gridVisualColor)
    {
        foreach (GridVisualColorMaterial gridVisualColorMaterial in gridVisualColorMaterialList)
        {
            if (gridVisualColorMaterial.gridVisualColor == gridVisualColor) return gridVisualColorMaterial.material;
        }
        Debug.LogError("Could not find GridVisualColorMaterial for GridVisualColor " + gridVisualColor);
        return null;
    }

}
