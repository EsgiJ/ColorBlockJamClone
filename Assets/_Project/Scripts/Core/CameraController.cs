using UnityEngine;

namespace ColorBlockJamClone.Core
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private float _tiltAngle = 80f;
        [SerializeField] private float _widthPadding = 4f;   

        private void Reset()
        {
            _camera = GetComponent<Camera>();
        }

        public void FrameGrid(int gridWidth, int gridHeight, float cellSize)
        {
            if (_camera == null) 
                _camera = Camera.main;

            float gridWorldWidth = gridWidth * cellSize;
            float gridWorldHeight = gridHeight * cellSize;

            float fovYRad = _camera.fieldOfView * Mathf.Deg2Rad;
            float aspect = _camera.aspect;
            float fovXRad = 2f * Mathf.Atan(Mathf.Tan(fovYRad * 0.5f) * aspect);

            float distForWidth = (gridWorldWidth * 0.5f + _widthPadding) / Mathf.Tan(fovXRad * 0.5f);

            float distForHeight = (gridWorldHeight * 0.5f + _widthPadding) / Mathf.Tan(fovYRad * 0.5f);

            float distance = Mathf.Max(distForWidth, distForHeight);

            float tiltRad = _tiltAngle * Mathf.Deg2Rad;
            Vector3 camPos = new Vector3(
                0f,
                distance * Mathf.Sin(tiltRad),
                -distance * Mathf.Cos(tiltRad)
            );

            _camera.transform.position = camPos;
            _camera.transform.rotation = Quaternion.Euler(_tiltAngle, 0f, 0f);
        }
    }
}