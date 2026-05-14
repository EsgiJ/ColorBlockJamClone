using UnityEngine;

namespace ColorBlockJam.Gameplay.Grid
{
    /// <summary>
    /// Single cell on the grid. Pure data
    /// </summary>
    public class Cell
    {
        public Vector2Int Position { get; }
        public bool IsBlocked { get; set; }
        public object OccupiedBy { get; set; } // Occupied will be implemented later

        public bool IsFree => !IsBlocked && OccupiedBy == null;

        public Cell(Vector2Int position)
        {
            Position = position;
        }
    }
}