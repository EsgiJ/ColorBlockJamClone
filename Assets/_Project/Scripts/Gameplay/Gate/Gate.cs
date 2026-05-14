using ColorBlockJamClone.Data;
using ColorBlockJamClone.Gameplay.Grid;
using UnityEngine;

namespace ColorBlockJamClone.Gameplay.Gate
{
    public class Gate : MonoBehaviour
    {
        [SerializeField] private Renderer _renderer;

        public BlockColor Color { get; private set; }
        public GridSide Side { get; private set; }
        public int PositionAlongSide { get; private set; }
        public int Width { get; private set; }

        public void Initialize(BlockColor color, GridSide side, int positionAlongSide, int width, ColorPaletteSO palette, GridSystem grid)
        {
            Color = color;
            Side = side;
            PositionAlongSide = positionAlongSide;
            Width = Mathf.Max(1, width);

            name = $"Gate_{color}_{side}_{positionAlongSide}";

            ApplyColor(palette);
            PlaceOnEdge(grid);
        }

        private void ApplyColor(ColorPaletteSO palette)
        {
            if (_renderer != null)
                _renderer.sharedMaterial = palette.GetMaterial(Color);
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
                          + new Vector3((Width - 1) * cellSize * 0.5f, 0f, -cellSize);
                    yRot = 0f;
                    break;
                case GridSide.Top:
                    pos = grid.GridToWorldCentered(new Vector2Int(PositionAlongSide, grid.Height - 1))
                          + new Vector3((Width - 1) * cellSize * 0.5f, 0f, cellSize);
                    yRot = 180f;
                    break;
                case GridSide.Left:
                    pos = grid.GridToWorldCentered(new Vector2Int(0, PositionAlongSide))
                          + new Vector3(-cellSize, 0f, (Width - 1) * cellSize * 0.5f);
                    yRot = 90f;
                    break;
                case GridSide.Right:
                    pos = grid.GridToWorldCentered(new Vector2Int(grid.Width - 1, PositionAlongSide))
                          + new Vector3(cellSize, 0f, (Width - 1) * cellSize * 0.5f);
                    yRot = -90f;
                    break;
                default:
                    pos = Vector3.zero;
                    break;
            }

            pos.y = transform.position.y; 
            transform.position = pos;
            transform.rotation = Quaternion.Euler(0f, yRot, 0f);
            transform.localScale = new Vector3(Width * cellSize, cellSize * 0.4f, cellSize * 0.5f);
        }
    }
}