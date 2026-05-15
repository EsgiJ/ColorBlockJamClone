using ColorBlockJamClone.Data;
using ColorBlockJamClone.Gameplay.Grid;
using UnityEngine;

namespace ColorBlockJamClone.Gameplay.Wall
{
    public class Wall : MonoBehaviour
    {
        [SerializeField] private Transform _wallMesh;
        
        public GridSide Side { get; private set; }
        public int PositionAlongSide { get; private set; }
        public int Width { get; private set; }

        public void Initialize(GridSide side, int positionAlongSide, int width, GridSystem grid)
        {
            Side = side;
            PositionAlongSide = positionAlongSide;
            Width = Mathf.Max(1, width);

            name = $"Wall_{side}_{positionAlongSide}";

            PlaceOnEdge(grid);
        }

        private void PlaceOnEdge(GridSystem grid)
        {
            float cellSize = grid.CellSize;
            Vector3 pos;
            float yRot = 0f;

            switch (Side)
            {
                case GridSide.Bottom:
                    pos = grid.GridToWorldCentered(new Vector2Int(PositionAlongSide, 0))
                          + new Vector3((Width - 1) * cellSize * 0.5f, 0f, -cellSize * 0.75f);
                    yRot = 0f;
                    break;
                case GridSide.Top:
                    pos = grid.GridToWorldCentered(new Vector2Int(PositionAlongSide, grid.Height - 1))
                          + new Vector3((Width - 1) * cellSize * 0.5f, 0f, cellSize * 0.75f);
                    yRot = 180f;
                    break;
                case GridSide.Left:
                    pos = grid.GridToWorldCentered(new Vector2Int(0, PositionAlongSide))
                          + new Vector3(-cellSize * 0.75f, 0f, (Width - 1) * cellSize * 0.5f);
                    yRot = 90f;
                    break;
                case GridSide.Right:
                    pos = grid.GridToWorldCentered(new Vector2Int(grid.Width - 1, PositionAlongSide))
                          + new Vector3(cellSize * 0.75f, 0f, (Width - 1) * cellSize * 0.5f);
                    yRot = -90f;
                    break;
                default:
                    pos = Vector3.zero;
                    break;
            }

            pos.y = transform.position.y; 
            transform.localPosition = pos;
            transform.rotation = Quaternion.Euler(0f, yRot, 0f);
            _wallMesh.localScale = new Vector3(Width * cellSize, cellSize * 1f, cellSize * 0.5f);
        }
    }
}