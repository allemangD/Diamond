using OpenTK;

namespace Diamond.Render
{
    public class Camera
    {
        private Vector3 _position = Vector3.Zero;
        private Vector3 _target = -Vector3.One;
        private Vector3 _up = Vector3.UnitZ;

        public Matrix4 View;
        public Matrix4 Projection;

        public Vector3 Position
        {
            set
            {
                _position = value;
                UpdateView();
            }
            get => _position;
        }

        public Vector3 Target
        {
            set
            {
                _target = value;
                UpdateView();
            }
            get => _target;
        }

        public Vector3 Up
        {
            set
            {
                _up = value;
                UpdateView();
            }
            get => _up;
        }

        private void UpdateView()
        {
            View = Matrix4.LookAt(_position, _target, _up);
        }
    }
}