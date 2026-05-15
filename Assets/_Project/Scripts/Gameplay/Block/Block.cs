using System;
using ColorBlockJamClone.Data;
using ColorBlockJamClone.Gameplay.Grid;
using DG.Tweening;
using UnityEngine;

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
        public FeedbackConfigSO Feedback { get; private set; }

        private GameObject _visualInstance;
        private Vector3 _visualBaseLocalPos;
        private Tween _floatTween;

        public void Initialize(
            BlockShapeSO shape, BlockColor color, Vector2Int gridPosition,
            int rotationSteps, ColorPaletteSO palette, FeedbackConfigSO feedback,
            float cellSize)
        {
            Shape = shape;
            Color = color;
            GridPosition = gridPosition;
            RotationSteps = rotationSteps;
            CellSize = cellSize;
            ColorPalette = palette;
            Feedback = feedback;

            name = $"Block_{color}_{shape.ShapeName}";
            BuildVisual();
        }

        private void BuildVisual()
        {
            if (_visualInstance != null) 
                Destroy(_visualInstance);

            var material = ColorPalette.GetMaterial(Color);

            if (Shape.VisualPrefab != null)
            {
                _visualInstance = Instantiate(Shape.VisualPrefab, transform);

                var offsets = Shape.GetRotatedOffsets(RotationSteps);
                Vector2 centroid = Vector2.zero;
                foreach (var o in offsets) 
                {
                    centroid += new Vector2(o.x, o.y);
                }
                centroid /= offsets.Length;

                _visualInstance.transform.localPosition += new Vector3(
                    centroid.x * CellSize, 0f, centroid.y * CellSize
                );
                _visualInstance.transform.localRotation =
                    Quaternion.Euler(0f, -RotationSteps * 90f, 0f) * _visualInstance.transform.localRotation;

                _visualBaseLocalPos = _visualInstance.transform.localPosition;

                if (material != null)
                    foreach (var r in _visualInstance.GetComponentsInChildren<Renderer>())
                        r.sharedMaterial = material;
            }
        }

        public Vector2Int[] GetOccupiedCells() => GetCellsAt(GridPosition);

        public Vector2Int[] GetCellsAt(Vector2Int origin)
        {
            var offsets = Shape.GetRotatedOffsets(RotationSteps);
            var result = new Vector2Int[offsets.Length];
            
            for (int i = 0; i < offsets.Length; i++)
            {
                result[i] = origin + offsets[i];                
            }

            return result;
        }

        public void SetGridPosition(Vector2Int newPos, GridSystem grid)
        {
            GridPosition = newPos;
            var worldPos = grid.GridToWorldCentered(newPos);
            worldPos.y = transform.position.y;
            transform.position = worldPos;
        }

        public void PlayPickup()
        {
            if (Feedback == null || _visualInstance == null) return;

            transform.DOKill();
            transform.DOScale(Vector3.one * Feedback.blockPickupScale, Feedback.blockPickupDuration)
                .SetEase(Feedback.blockPickupEase);

            _visualInstance.transform.DOKill();
            _floatTween?.Kill();

            var liftPos = _visualBaseLocalPos + Vector3.up * Feedback.blockLiftHeight;

            _visualInstance.transform.DOLocalMove(liftPos, Feedback.blockPickupDuration)
                .SetEase(Ease.OutCubic)
                .OnComplete(StartFloat);
        }

        private void StartFloat()
        {
            if (_visualInstance == null || Feedback == null) return;
            float currentY = _visualInstance.transform.localPosition.y;
            _floatTween = _visualInstance.transform
                .DOLocalMoveY(currentY + Feedback.blockFloatAmplitude, Feedback.blockFloatPeriod * 0.5f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }

        public void PlaySnap()
        {
            if (Feedback == null) 
                return;

            transform.DOKill();
            transform.DOScale(Vector3.one, Feedback.blockSnapDuration).SetEase(Feedback.blockSnapEase);

            _floatTween?.Kill();
            if (_visualInstance != null)
            {
                _visualInstance.transform.DOKill();
                _visualInstance.transform.DOLocalMove(_visualBaseLocalPos, Feedback.blockSnapDuration)
                    .SetEase(Ease.OutCubic);
            }
        }

        public void AnimateExit(Vector3 outwardDir, Action onComplete)
        {
            transform.DOKill();
            _floatTween?.Kill();
            if (_visualInstance != null) 
                _visualInstance.transform.DOKill();

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
            _floatTween?.Kill();
            transform.localScale = Vector3.one;
            if (_visualInstance != null)
            {
                _visualInstance.transform.DOKill();
                _visualInstance.transform.localPosition = _visualBaseLocalPos;
            }
        }
    }
}