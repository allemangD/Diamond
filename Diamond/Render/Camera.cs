using OpenTK;

namespace Diamond.Render
{
    /// <summary>
    /// Manages a projection and view matrix
    /// </summary>
    public class Camera
    {
        private Vector3 _position = Vector3.Zero;
        private Vector3 _target = -Vector3.One;
        private Vector3 _up = Vector3.UnitZ;

        /// <summary>
        /// The view matrix
        /// </summary>
        public Matrix4 View;

        /// <summary>
        /// The projection matrix
        /// </summary>
        public Matrix4 Projection;

        /// <summary>
        /// Sets and updates the position of the view matrix
        /// </summary>
        public Vector3 Position
        {
            set
            {
                _position = value;
                UpdateView();
            }
            get => _position;
        }

        /// <summary>
        /// Sets and updates the target of the view matrix
        /// </summary>
        public Vector3 Target
        {
            set
            {
                _target = value;
                UpdateView();
            }
            get => _target;
        }

        /// <summary>
        /// Sets and updates the up vector of the view matrix
        /// </summary>
        public Vector3 Up
        {
            set
            {
                _up = value;
                UpdateView();
            }
            get => _up;
        }

        /// <summary>
        /// Recalculate the view matrix
        /// </summary>
        private void UpdateView()
        {
            View = Matrix4.LookAt(_position, _target, _up);
        }
    }
}