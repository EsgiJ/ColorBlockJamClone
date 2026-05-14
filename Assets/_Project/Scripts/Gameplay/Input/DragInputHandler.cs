using System.Collections.Generic;
using ColorBlockJamClone.Core;
using ColorBlockJamClone.Gameplay.Grid;
using UnityEngine;

namespace ColorBlockJamClone.Gameplay.Input
{
    public class DragInputHandler : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private int _maxLookahead = 12;

        private GridSystem _grid;
        private BlockMover _mover;
        private IReadOnlyList<Gate.Gate> _gates;
        private System.Action<Block.Block> _onBlockExited;

        private Block.Block _dragging;
        private Vector3 _dragStartGround;
        private Vector3 _dragStartBlockPos;
        private Vector2Int _dragStartGrid;

        private bool _gameStarted;
        private Vector2Int _currentSnap;

        public void Initialize(GridSystem grid, BlockMover mover, IReadOnlyList<Gate.Gate> gates, System.Action<Block.Block> onBlockExited)
        {
            _grid = grid;
            _mover = mover;
            _gates = gates;
            _onBlockExited = onBlockExited;
            if (_camera == null) _camera = Camera.main;
        }

        private void Update()
        {
            if (_grid == null) return;

            if (UnityEngine.Input.GetMouseButtonDown(0)) 
                StartDrag();
            else if (UnityEngine.Input.GetMouseButton(0) && _dragging != null) 
                UpdateDrag();
            else if (UnityEngine.Input.GetMouseButtonUp(0) && _dragging != null) 
                EndDrag();
        }

        private void StartDrag()
        {
            var screenPos = UnityEngine.Input.mousePosition;
            var ray = _camera.ScreenPointToRay(screenPos);
            if (!Physics.Raycast(ray, out var hit, 100f)) return;

            var block = hit.collider.GetComponentInParent<Block.Block>();
            if (block == null) return;

            _dragging = block;
            _dragStartGround = WorldOnGroundPlane(screenPos);
            _dragStartBlockPos = block.transform.position;
            _dragStartGrid = block.GridPosition;
            _currentSnap = Vector2Int.zero;

            _grid.Release(block);

            if (!_gameStarted)
            {
                _gameStarted = true;
                GameEvents.RaiseLevelStarted();
            }
        }

        private void UpdateDrag()
        {
            var currentGround = WorldOnGroundPlane(UnityEngine.Input.mousePosition);
            var delta = currentGround - _dragStartGround;

            float desiredCellsX = delta.x / _grid.CellSize;
            float desiredCellsZ = delta.z / _grid.CellSize;

            Vector2Int desiredSnap = new Vector2Int(
                Mathf.RoundToInt(desiredCellsX),
                Mathf.RoundToInt(desiredCellsZ)
            );

            int safety = 100;
            while (_currentSnap != desiredSnap && safety-- > 0)
            {
                Vector2Int remaining = desiredSnap - _currentSnap;
                Vector2Int primary, secondary;

                if (Mathf.Abs(remaining.x) >= Mathf.Abs(remaining.y))
                {
                    primary   = new Vector2Int(System.Math.Sign(remaining.x), 0);
                    secondary = new Vector2Int(0, System.Math.Sign(remaining.y));
                }
                else
                {
                    primary   = new Vector2Int(0, System.Math.Sign(remaining.y));
                    secondary = new Vector2Int(System.Math.Sign(remaining.x), 0);
                }

                if (primary != Vector2Int.zero &&
                    IsValidAt(_dragging, _dragStartGrid + _currentSnap + primary))
                {
                    _currentSnap += primary;
                    continue;
                }
                if (secondary != Vector2Int.zero &&
                    IsValidAt(_dragging, _dragStartGrid + _currentSnap + secondary))
                {
                    _currentSnap += secondary;
                    continue;
                }
                break;
            }

            float allowedX = Mathf.Clamp(desiredCellsX, _currentSnap.x - 0.5f, _currentSnap.x + 0.5f);
            float allowedZ = Mathf.Clamp(desiredCellsZ, _currentSnap.y - 0.5f, _currentSnap.y + 0.5f);

            Vector3 worldOffset = new Vector3(allowedX * _grid.CellSize, 0f, allowedZ * _grid.CellSize);
            _dragging.transform.position = _dragStartBlockPos + worldOffset;
        }

        private void EndDrag()
        {
            Vector2Int newPos = _dragStartGrid + _currentSnap;

            // Safety (drag boyunca valid kalmış olmalı ama yine de kontrol et)
            if (!_grid.CanOccupy(_dragging, _dragging.GetCellsAt(newPos)))
                newPos = _dragStartGrid;

            _dragging.SetGridPosition(newPos, _grid);
            _grid.Occupy(_dragging);

            var exitGate = FindExitGate(_dragging);
            if (exitGate != null)
            {
                _grid.Release(_dragging);
                var exitedBlock = _dragging;
                _dragging.AnimateExit(
                    exitGate.OutwardWorldDirection,
                    () => _onBlockExited?.Invoke(exitedBlock)
                );
            }

            _dragging = null;
            _currentSnap = Vector2Int.zero;
        }

        private bool IsValidAt(Block.Block block, Vector2Int gridPos)
        {
            return _grid.CanOccupy(block, block.GetCellsAt(gridPos));
        }

        private Gate.Gate FindExitGate(Block.Block block)
        {
            foreach (var cell in block.GetOccupiedCells())
            {
                foreach (var gate in _gates)
                {
                    if (gate.Color != block.Color) 
                        continue;

                    if (gate.CoversCell(cell, _grid.Width, _grid.Height)) 
                        return gate;
                }
            }
            return null;
        }

        private Vector3 WorldOnGroundPlane(Vector3 screenPos)
        {
            var ray = _camera.ScreenPointToRay(screenPos);
            var plane = new Plane(Vector3.up, _grid.Origin);
            return plane.Raycast(ray, out float d) ? ray.GetPoint(d) : Vector3.zero;
        }
    }
}