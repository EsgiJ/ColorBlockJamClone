using ColorBlockJamClone.Data;
using ColorBlockJamClone.Gameplay.Grid;
using DG.Tweening;
using UnityEngine;

namespace ColorBlockJamClone.Gameplay.Gate
{
    public class Gate : MonoBehaviour
    {
        [SerializeField] private Renderer _renderer;
        [SerializeField] private Transform _gateMesh;
        
        private float _originalY;

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
            _gateMesh.localScale = new Vector3(Width * cellSize, cellSize * 1f, cellSize * 0.5f);

            _originalY = transform.position.y;
        }

        public bool CoversCell(Vector2Int cell, int gridWidth, int gridHeight)
        {
            switch (Side)
            {
                case GridSide.Bottom:
                    return cell.y == 0 && cell.x >= PositionAlongSide && cell.x < PositionAlongSide + Width;
                case GridSide.Top:
                    return cell.y == gridHeight - 1 && cell.x >= PositionAlongSide && cell.x < PositionAlongSide + Width;
                case GridSide.Left:
                    return cell.x == 0 && cell.y >= PositionAlongSide && cell.y < PositionAlongSide + Width;
                case GridSide.Right:
                    return cell.x == gridWidth - 1 && cell.y >= PositionAlongSide && cell.y < PositionAlongSide + Width;
            }
            return false;
        }

        public void PlayOpen(float blockExitDuration)
        {
            transform.DOKill();

            Sequence seq = DOTween.Sequence();

            seq.Append(transform.DOMoveY(_originalY - 1.5f, 0.2f).SetEase(Ease.OutQuad));

            seq.AppendInterval(blockExitDuration);

            seq.Append(transform.DOMoveY(_originalY, 0.3f).SetEase(Ease.OutBack));
        }

        public Vector3 OutwardWorldDirection => Side switch
        {
            GridSide.Bottom => Vector3.back,
            GridSide.Top    => Vector3.forward,
            GridSide.Left   => Vector3.left,
            GridSide.Right  => Vector3.right,
            _ => Vector3.zero
        };
    }
}