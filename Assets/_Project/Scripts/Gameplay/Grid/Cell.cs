using UnityEngine;

namespace ColorBlockJamClone.Gameplay.Grid
{
    public class Cell
    {
        public Vector2Int Position { get; }
        public bool IsBlocked { get; set; }
        public IGridOccupant OccupiedBy { get; set; }

        public bool IsFree => !IsBlocked && OccupiedBy == null;

        public Cell(Vector2Int position)
        {
            Position = position;
        }
    }
}