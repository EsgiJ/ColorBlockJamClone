using System.Collections.Generic;
using ColorBlockJamClone.Core;
using ColorBlockJamClone.Data;
using ColorBlockJamClone.Gameplay.Grid;
using UnityEngine;

namespace ColorBlockJamClone.Gameplay.Input
{
    public class DragInputHandler : MonoBehaviour
    {
        [SerializeField] private Camera _camera;

        private GridSystem _grid;
        private BlockMover _mover;
        private IReadOnlyList<Gate.Gate> _gates;
        private System.Action<Block.Block> _onBlockExited;

        private Block.Block _dragging;
        private Vector3 _dragStartGround;
        private Vector3 _dragStartBlockPos;
        private Vector2Int _dragStartGrid;

        private Vector3 _currentBlockOffset;
        private bool _gameStarted;
        private System.Action _onFirstInput;

        public void Initialize(
            GridSystem grid, 
            BlockMover mover, 
            IReadOnlyList<Gate.Gate> gates, 
            System.Action<Block.Block> onBlockExited, 
            System.Action onFirstInput)
        {
            _grid = grid;
            _mover = mover;
            _gates = gates;
            _onBlockExited = onBlockExited;
            _onFirstInput = onFirstInput;
            if (_camera == null) _camera = Camera.main;
            _gameStarted = false;
        }

        private void Update()
        {
            if (_grid == null) 
                return;

            var state = GameManager.Instance?.State;
            if (state != GameState.Start && state != GameState.Gameplay)
                return;

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
                _currentBlockOffset = Vector3.zero;     

                _grid.Release(block);

                if (!_gameStarted)
                {
                    _gameStarted = true;
                    _onFirstInput?.Invoke();   
                }

                _dragging.PlayPickup();
                AudioManager.Instance?.PlayBlockGrab();
        }

        private void UpdateDrag()
        {
            var currentGround = WorldOnGroundPlane(UnityEngine.Input.mousePosition);
            Vector3 targetOffset = currentGround - _dragStartGround;
            targetOffset.y = 0f;

            Vector3 diff = targetOffset - _currentBlockOffset;
            float dist = diff.magnitude;

            if (dist > 0.001f)
            {
                int subSteps = Mathf.Max(1, Mathf.CeilToInt(dist / (_grid.CellSize * 0.25f)));
                Vector3 step = diff / subSteps;

                for (int i = 0; i < subSteps; i++)
                {
                    Vector3 trial = _currentBlockOffset + step;
                    if (IsValidOffset(trial))
                    {
                        _currentBlockOffset = trial;
                        continue;
                    }

                    Vector3 trialX = _currentBlockOffset + new Vector3(step.x, 0f, 0f);
                    if (Mathf.Abs(step.x) > 0.0001f && IsValidOffset(trialX))
                    {
                        _currentBlockOffset = trialX;
                        continue;
                    }

                    Vector3 trialZ = _currentBlockOffset + new Vector3(0f, 0f, step.z);
                    if (Mathf.Abs(step.z) > 0.0001f && IsValidOffset(trialZ))
                    {
                        _currentBlockOffset = trialZ;
                        continue;
                    }

                    break;
                }
            }

            _dragging.transform.position = _dragStartBlockPos + _currentBlockOffset;
        }

        private bool IsValidOffset(Vector3 worldOffset)
        {
            float cellX = worldOffset.x / _grid.CellSize;
            float cellZ = worldOffset.z / _grid.CellSize;

            var offsets = _dragging.Shape.GetRotatedOffsets(_dragging.RotationSteps);

            foreach (var off in offsets)
            {
                float cx = _dragStartGrid.x + off.x + 0.5f + cellX;
                float cz = _dragStartGrid.y + off.y + 0.5f + cellZ;

                int gxMin = Mathf.FloorToInt(cx - 0.5f + 0.001f);
                int gxMax = Mathf.FloorToInt(cx + 0.5f - 0.001f);
                int gzMin = Mathf.FloorToInt(cz - 0.5f + 0.001f);
                int gzMax = Mathf.FloorToInt(cz + 0.5f - 0.001f);

                for (int gx = gxMin; gx <= gxMax; gx++)
                {
                    for (int gz = gzMin; gz <= gzMax; gz++)
                    {
                        var pos = new Vector2Int(gx, gz);

                        if (!_grid.IsInBounds(pos))
                            return false;

                        var cell = _grid.GetCell(pos);
                        if (cell.IsBlocked)
                            return false;
                        if (cell.OccupiedBy != null && cell.OccupiedBy != _dragging as IGridOccupant)
                            return false;
                    }
                }
            }

            return true;
        }

        private void EndDrag()
        {
            Vector2Int finalSnap = new Vector2Int(
                Mathf.RoundToInt(_currentBlockOffset.x / _grid.CellSize),
                Mathf.RoundToInt(_currentBlockOffset.z / _grid.CellSize)
            );
            Vector2Int newPos = _dragStartGrid + finalSnap;

            if (!_grid.CanOccupy(_dragging, _dragging.GetCellsAt(newPos)))
                newPos = _dragStartGrid;

            _dragging.SetGridPosition(newPos, _grid);
            _grid.Occupy(_dragging);

            var exitGate = FindExitGate(_dragging);
            if (exitGate != null)
            {
                _grid.Release(_dragging);
                var exitedBlock = _dragging;

                exitGate.PlayOpen(0.55f);
                
                AudioManager.Instance?.PlayBlockExit();
                
                _dragging.AnimateExit(
                    exitGate.OutwardWorldDirection,
                    () => _onBlockExited?.Invoke(exitedBlock)
                );
            }
            else
            {
                _dragging.PlaySnap();
                AudioManager.Instance?.PlayBlockSnap();
            }

            _dragging = null;
            _currentBlockOffset = Vector3.zero;
        }

        private Gate.Gate FindExitGate(Block.Block block)
        {
            foreach (var gate in _gates)
            {
                if (gate.Color != block.Color) 
                    continue;

                var blockCells = block.GetOccupiedCells();
                var edgeCells = new List<Vector2Int>();
                foreach (var c in blockCells)
                {
                    if (IsCellOnGateEdge(c, gate.Side)) 
                        edgeCells.Add(c);
                }

                if (edgeCells.Count == 0) 
                    continue;

                bool allCovered = true;
                foreach (var c in edgeCells)
                {
                    if (!gate.CoversCell(c, _grid.Width, _grid.Height))
                    {
                        allCovered = false;
                        break;
                    }
                }

                if (allCovered) 
                    return gate;
            }
            return null;
        }

        private bool IsCellOnGateEdge(Vector2Int cell, GridSide side) => side switch
        {
            GridSide.Bottom => cell.y == 0,
            GridSide.Top    => cell.y == _grid.Height - 1,
            GridSide.Left   => cell.x == 0,
            GridSide.Right  => cell.x == _grid.Width - 1,
            _ => false
        };

        private Vector3 WorldOnGroundPlane(Vector3 screenPos)
        {
            var ray = _camera.ScreenPointToRay(screenPos);
            var plane = new Plane(Vector3.up, _grid.Origin);
            return plane.Raycast(ray, out float d) ? ray.GetPoint(d) : Vector3.zero;
        }

        public void ResetForNewLevel()
        {
            _gameStarted = false;
            _dragging = null;
        }
    }
}