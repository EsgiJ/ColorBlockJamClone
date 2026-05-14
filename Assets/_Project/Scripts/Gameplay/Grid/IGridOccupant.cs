using UnityEngine;

namespace ColorBlockJamClone.Gameplay.Grid
{
    /// <summary>
    /// Anything that can occupy grid cells. Implemented by Block.
    /// </summary>
    public interface IGridOccupant
    {
        Vector2Int[] GetOccupiedCells();
    }
}