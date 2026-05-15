using UnityEngine;
using ColorBlockJamClone.Data;
using ColorBlockJamClone.Gameplay.Grid;
using System;
using DG.Tweening;

namespace ColorBlockJamClone.Gameplay.Block
{   
    public class Block : MonoBehaviour, IGridOccupant
    {
        public BlockShapeSO Shape { get; private set; }
        public BlockColor Color { get; private set; }
        public Vector2Int GridPosition { get; private set; }
        public int RotationSteps { get; private set; }
        public float CellSize { get; private set; }
        public ColorPaletteSO ColorPalette { get; private set; }

        public void Initialize(BlockShapeSO shape, BlockColor color, Vector2Int gridPosition, int rotationSteps, ColorPaletteSO palette, float cellSize)
        {
            Shape = shape;
            Color = color;
            GridPosition = gridPosition;
            RotationSteps = rotationSteps;
            CellSize = cellSize;
            ColorPalette = palette;

            name = $"Block_{color}_{shape.ShapeName}";
            BuildVisual();
        }

        private void BuildVisual()
        {
            var material = ColorPalette.GetMaterial(Color);

            if (Shape.VisualPrefab != null)
            {
                var visual = Instantiate(Shape.VisualPrefab, transform);

                var offsets = Shape.GetRotatedOffsets(RotationSteps);
                Vector2 centroid = Vector2.zero;

                foreach (var o in offsets) 
                    centroid += new Vector2(o.x, o.y);

                centroid /= offsets.Length;

                visual.transform.localPosition = new Vector3(
                    centroid.x * CellSize,
                    0f,
                    centroid.y * CellSize
                );

                visual.transform.localRotation = Quaternion.Euler(0f, -RotationSteps * 90f, 0f) * visual.transform.localRotation;

                if (material != null)
                {
                    foreach (var renderer in visual.GetComponentsInChildren<Renderer>())
                    {
                        renderer.sharedMaterial = material;
                    }
                }
            }
            else
            {
                Debug.LogWarning($"[Block] Shape '{Shape.ShapeName}' has no VisualPrefab. " + "Falling back to per-cell visuals.");
            }
        }

        public Vector2Int[] GetOccupiedCells() => GetCellsAt(GridPosition);

        public Vector2Int[] GetCellsAt(Vector2Int origin)
        {
            var offsets = Shape.GetRotatedOffsets(RotationSteps);
            var result = new Vector2Int[offsets.Length];
            for (int i = 0; i < offsets.Length; i++)
                result[i] = origin + offsets[i];
            return result;
        }

        public void SetGridPosition(Vector2Int newPos, GridSystem grid)
        {
            GridPosition = newPos;
            transform.position = grid.GridToWorldCentered(newPos);
        }

        public void PlayPickup()
        {
            transform.DOKill();
            transform.DOScale(Vector3.one * 1.08f, 0.12f).SetEase(Ease.OutBack);
        }

        public void PlaySnap()
        {
            transform.DOKill();
            transform.DOScale(Vector3.one, 0.18f).SetEase(Ease.OutBounce);
        }

        public void AnimateExit(Vector3 outwardDir, Action onComplete)
        {
            transform.DOKill();
            const float duration = 0.55f;

            Sequence seq = DOTween.Sequence();
            seq.Append(transform.DOMove(transform.position + outwardDir * (CellSize * 1.2f), duration)
                .SetEase(Ease.InCubic));
            seq.Join(transform.DOScale(Vector3.zero, duration).SetEase(Ease.InCubic));
            seq.OnComplete(() => onComplete?.Invoke());
        }

        public void ResetForReuse()
        {
            transform.DOKill();
            transform.localScale = Vector3.one;
        }        
    }
}