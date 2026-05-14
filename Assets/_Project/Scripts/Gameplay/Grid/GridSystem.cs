using UnityEngine;

namespace ColorBlockJamClone.Gameplay.Grid
{
    public class GridSystem
    {
        public int Width { get; }
        public int Height { get; }
        public float CellSize { get; }
        public Vector3 Origin { get; }

        private readonly Cell[,] _cells;

            public GridSystem(int width, int height, float cellSize, Vector3 origin, Vector2Int[] blockedCells = null)
            {
                Width = width;
                Height = height;
                CellSize = cellSize;
                Origin = origin;

                _cells = new Cell[width, height];
                for (int x = 0; x < width; x++)
                    for (int y = 0; y < height; y++)
                        _cells[x, y] = new Cell(new Vector2Int(x, y));

                if (blockedCells != null)
                {
                    foreach (var c in blockedCells)
                        if (IsInBounds(c)) 
                            _cells[c.x, c.y].IsBlocked = true;
                }
            }

            public bool IsInBounds(Vector2Int pos) =>
                pos.x >= 0 && pos.x < Width && pos.y >= 0 && pos.y < Height;

            public Cell GetCell(Vector2Int pos) =>
                IsInBounds(pos) ? _cells[pos.x, pos.y] : null;

            public Vector3 GridToWorld(Vector2Int gridPos) =>
                Origin + new Vector3(gridPos.x * CellSize, 0f, gridPos.y * CellSize);

            public Vector3 GridToWorldCentered(Vector2Int gridPos) =>
                GridToWorld(gridPos) + new Vector3(CellSize * 0.5f, 1.5f, CellSize * 0.5f);

            public Vector2Int WorldToGrid(Vector3 worldPos)
            {
                var local = worldPos - Origin;
                return new Vector2Int(
                    Mathf.FloorToInt(local.x / CellSize),
                    Mathf.FloorToInt(local.z / CellSize)
                );
            }

            public bool CanOccupy(IGridOccupant occupant, Vector2Int[] proposedCells)
            {
                foreach (var pos in proposedCells)
                {
                    if (!IsInBounds(pos)) 
                        return false;

                    var cell = _cells[pos.x, pos.y];

                    if (cell.IsBlocked) 
                        return false;
                    
                    if (cell.OccupiedBy != null && cell.OccupiedBy != occupant)     
                        return false;
                }
                return true;
            }

            public void Occupy(IGridOccupant occupant)
            {
                foreach (var pos in occupant.GetOccupiedCells())
                {
                    var cell = GetCell(pos);
                    if (cell != null) 
                        cell.OccupiedBy = occupant;
                }
            }

            public void Release(IGridOccupant occupant)
            {
                foreach (var pos in occupant.GetOccupiedCells())
                {
                    var cell = GetCell(pos);
                    if (cell != null && cell.OccupiedBy == occupant) 
                        cell.OccupiedBy = null;
                }
            }
    }
}