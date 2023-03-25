using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootAction : BaseAction
{

    public static event EventHandler<OnShootEventArgs> OnAnyShoot;
    public event EventHandler<OnShootEventArgs> OnShoot;

    public class OnShootEventArgs : EventArgs
    {
        public Unit targetUnit;
        public Unit shootingUnit;
    }

    private enum State
    {
        PreShooting,
        Shooting,
        PostShooting
    }

    private State state;
    private float stateTimer;
    private Unit targetUnit;
    private bool canShoot;

    [SerializeField] private LayerMask obstacleLayerMask;
    [SerializeField] private int maxShootDistance = 7;
    [SerializeField] private int damageAmount = 40;
    [SerializeField] private float rotateSpeed = 10f;
    private float preShootingStateTime = 1f;
    private float shootingStateTime = 0.1f;
    private float postShootingStateTime = 0.5f;

    private void Update()
    {
        if (!isActive) return;

        stateTimer -= Time.deltaTime;

        switch (state)
        {
            case State.PreShooting:
                Vector3 aimDirection = (targetUnit.GetWorldPosition() - unit.GetWorldPosition()).normalized;
                transform.forward = Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime * rotateSpeed);
                break;
            case State.Shooting:
                if (canShoot) {
                    Shoot();
                    canShoot = false;
                }
                break;
            case State.PostShooting:
                break;
        }

        if (stateTimer <= 0f) NextState();

    }

    private void NextState ()
    {
        switch (state)
        {
            case State.PreShooting:
                state = State.Shooting;
                stateTimer = shootingStateTime;
                break;
            case State.Shooting:
                state = State.PostShooting;
                stateTimer = postShootingStateTime;
                break;
            case State.PostShooting:
                ActionComplete();
                break;
        }
    }

    private void Shoot()
    {
        OnAnyShoot?.Invoke(this, new OnShootEventArgs {
            targetUnit = targetUnit,
            shootingUnit = unit
        });
        OnShoot?.Invoke(this, new OnShootEventArgs {
            targetUnit = targetUnit,
            shootingUnit = unit
        });
        targetUnit.Damage(damageAmount);
    }

    public override string GetActionName()
    {
        return "Shoot";
    }

    public override List<GridPosition> GetValidActionGridPositionList()
    {
        GridPosition unitGridPosition = unit.GetGridPosition();
        return GetValidActionGridPositionList(unitGridPosition);
    }

    public List<GridPosition> GetValidActionGridPositionList(GridPosition unitGridPosition)
    {
        List<GridPosition> validGridPositionList = new List<GridPosition>();
        for (int x = -maxShootDistance; x <= maxShootDistance; x++)
        {
            for (int z = -maxShootDistance; z <= maxShootDistance; z++)
            {
                GridPosition offsetGridPosition = new GridPosition(x , z);
                GridPosition testGridPosition = unitGridPosition + offsetGridPosition;
                if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition)) continue;

                int testDistance = Mathf.Abs(x) + Mathf.Abs(z);
                if (testDistance > maxShootDistance) continue;

                if (!LevelGrid.Instance.HasAnyUnitOnGridPosition(testGridPosition)) continue;
                Unit targetUnit = LevelGrid.Instance.GetUnitAtGridPosition(testGridPosition);
                if (targetUnit.IsEnemy() == unit.IsEnemy()) continue;

                Vector3 unitWorldPosition = LevelGrid.Instance.GetWorldPosition(unitGridPosition);
                Vector3 shootDirection = (targetUnit.GetWorldPosition() - unitWorldPosition ).normalized;
                float unitShoulderHeight = 1.7f;
                bool isBlocked = Physics.Raycast(
                    unitWorldPosition  + Vector3.up * unitShoulderHeight, 
                    shootDirection,
                    Vector3.Distance(unitWorldPosition , targetUnit.GetWorldPosition()),
                    obstacleLayerMask
                );
                if (isBlocked) continue;
                validGridPositionList.Add(testGridPosition);
            }
        }
        return validGridPositionList;
    }

    public override void TakeAction(GridPosition gridPosition, Action onActionComplete)
    {
        targetUnit = LevelGrid.Instance.GetUnitAtGridPosition(gridPosition);
        state = State.PreShooting;
        stateTimer = preShootingStateTime;
        canShoot = true;
        ActionStart(onActionComplete);
    }

    public Unit GetTargetUnit()
    {
        return targetUnit;
    }

    public int GetMaxShootDistance()
    {
        return maxShootDistance;
    }

    public int GetTargetCountAsPosition(GridPosition gridPosition)
    {
        return GetValidActionGridPositionList(gridPosition).Count;
    }

    public override EnemyAIAction GetEnemyAIAction(GridPosition gridPosition)
    {
        Unit targetUnit = LevelGrid.Instance.GetUnitAtGridPosition(gridPosition);
        return new EnemyAIAction 
        {
            gridPosition = gridPosition,
            actionValue = 100 + (int)(100f * Mathf.RoundToInt(1 - targetUnit.GetHealthNormalized()))
        };
    }

}
