using ColorBlockJamClone.Gameplay.Block;
using ColorBlockJamClone.Gameplay.Grid;
using UnityEngine;

public class BlockMover
{
    private readonly GridSystem _gridSystem;

    public BlockMover(GridSystem gridSystem)
    {
        _gridSystem = gridSystem;
    }

    public int GetMaxFreeSteps(Block block, Vector2Int axisStep, int maxLookahead)
    {
        int free = 0;
        for (int step = 1; step <= maxLookahead; step++)
        {
            var proposedCells = block.GetCellsAt(block.GridPosition + axisStep * step);
            if (!_gridSystem.CanOccupy(block, proposedCells)) 
                break;
            free = step;
        }
        return free;
    }
}
