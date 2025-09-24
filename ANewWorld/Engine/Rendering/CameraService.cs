using Microsoft.Xna.Framework;

namespace ANewWorld.Engine.Rendering
{
    public sealed class CameraService
    {
        public int VirtualWidth { get; }
        public int VirtualHeight { get; }
        public int WorldWidth { get; }
        public int WorldHeight { get; }

        public Vector2 Position { get; private set; }
        public float Zoom { get; set; } = 1f;

        public CameraService(int virtualWidth, int virtualHeight, int worldWidth, int worldHeight, float zoom)
        {
            VirtualWidth = virtualWidth;
            VirtualHeight = virtualHeight;
            WorldWidth = worldWidth;
            WorldHeight = worldHeight;
            Zoom = zoom;
            Position = new Vector2(virtualWidth / 2f, virtualHeight / 2f);
        }

        public void Update(in Vector2 target)
        {
            // No clamping for now
            Vector2 clamped = target;
            //clamped.X = (float)System.Math.Round(clamped.X);
            //clamped.Y = (float)System.Math.Round(clamped.Y);
            Position = clamped;
        }

        public Matrix GetViewMatrix()
        {
            var translateToOrigin = Matrix.CreateTranslation(new Vector3(-Position.X, -Position.Y, 0f));
            var scale = Matrix.CreateScale(Zoom, Zoom, 1f);
            var translateToCenter = Matrix.CreateTranslation(new Vector3(VirtualWidth / 2f, VirtualHeight / 2f, 0f));
            return translateToOrigin * scale * translateToCenter;
        }
    }
}
