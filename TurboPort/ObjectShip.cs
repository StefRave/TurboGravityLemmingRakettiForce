using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TurboPort
{
    public class ObjectShip : IHandleController, I3DCollistionObject
    {
        private readonly IMissleProjectileFactory missleProjectileFactory;
        private readonly BulletBuffer bulletBuffer;
        private readonly ILevelBackground levelBackground;
        private Vector3  centerOffset;
        private float    scale;
        private static readonly VelocityPosistionCalculator velocityPosistionCalculator = new VelocityPosistionCalculator { Mass = 35 };

        public Vector3  Rotation;
        public Vector3  Position;
        public Vector3  Velocity;

        public Vector3  OldPosition;

        private BoundingBox    boundingBox;
        private BoundingSphere boundingSphere;
        public Vector3[] colisionPoints;
        bool prevFire;
        private Matrix renderMatrix;
        private bool shipColliding;
        private Vector3 bodyColor;
        private Vector3 screenColor;
        private BasicEffect bodyEffect;
        private BasicEffect screenEffect;

        public Model Model { get; private set; }


        static  bool colorSwitchHack;

        public ObjectShip(IMissleProjectileFactory missleProjectileFactory, BulletBuffer bulletBuffer, ILevelBackground levelBackground)
        {
            this.missleProjectileFactory = missleProjectileFactory;
            this.bulletBuffer = bulletBuffer;
            this.levelBackground = levelBackground;
        }

        public void Initialize(ContentManager content)
        {
            Model = content.Load<Model>(@"objects/pop_simple_lemming3");
            CorrectModel(Model);

            bodyEffect = (BasicEffect)Model.Meshes["Body"].Effects[0];
            screenEffect = (BasicEffect)Model.Meshes["Screen"].Effects[0];
            bodyColor = colorSwitchHack ? screenEffect.DiffuseColor : bodyEffect.DiffuseColor;
            screenColor = colorSwitchHack ? bodyEffect.DiffuseColor : screenEffect.DiffuseColor;
            colorSwitchHack = !colorSwitchHack;

            centerOffset = new Vector3(0f, 0f, 0f);
            scale = 1.0f;
            Position = new Vector3(160f, 160f, 0f);
            Velocity = new Vector3(0f, 0f, 0f);
            Rotation = Vector3.Zero;

            MeshUtil.GetBoundingFromMeshes(Model.Meshes, scale, out boundingBox, out boundingSphere);

            var originalBox = boundingBox;
            originalBox.Min /= scale;
            originalBox.Max /= scale;
            colisionPoints = MeshUtil.FindCollisionPoints(Model.Meshes, originalBox);
        }

        public void Bots(ObjectShip other)
        {
            Vector3 distance = other.Position - Position;
            if(distance.Length() < (boundingSphere.Radius * scale))
            {
                Position = OldPosition;
                other.Position = other.OldPosition;

                Vector3 tmp = other.Velocity;
                other.Velocity = Velocity;
                Velocity = tmp;
                SoundHandler.Shipcollide();
            }
        }

        private static void CorrectModel(Model model)
        {
            foreach (var mesh in model.Meshes)
                foreach (BasicEffect leffect in mesh.Effects)
                    leffect.Alpha = 1;
        }

        public void Update(GameTime gameTime)
        {
            renderMatrix =
                Matrix.CreateTranslation(new Vector3(centerOffset.X, centerOffset.Y, centerOffset.Z))*
                Matrix.CreateRotationY(Rotation.Y)*
                Matrix.CreateRotationZ(Rotation.Z);

            var pointLocationMatrix = renderMatrix*
                                      Matrix.CreateTranslation(Position);

            bool isColliding = false;
            for (int i = 0; i < 6; i++)
            {
                Vector3 v = colisionPoints[i];
                v = Vector3.Transform(v, pointLocationMatrix);

                bool pointColliding = levelBackground.CheckCollision(v);

                VertexPositionColor pv;
                pv.Position = v;
                pv.Color = pointColliding ? Color.Yellow : Color.White;

                if (pointColliding)
                {
                    bulletBuffer.AddBulletToRender(pv);
                }

                isColliding |= pointColliding;
            }
            if (!shipColliding && isColliding)
            {
                Position = OldPosition;
                Velocity = -Velocity * 0.3f;
                SoundHandler.Checkpoint();
            }
            shipColliding = isColliding;
        }

        public void Render(GraphicsDevice graphicsDevice, BasicEffect be)
        {
            var world = renderMatrix *
                Matrix.CreateTranslation(Position);

            graphicsDevice.DepthStencilState = DepthStencilState.Default;
            bodyEffect.DiffuseColor = bodyColor;
            screenEffect.DiffuseColor = screenColor;

            foreach (ModelMesh mesh in Model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.LightingEnabled = be.LightingEnabled;
                    effect.DirectionalLight0.DiffuseColor = be.DirectionalLight0.DiffuseColor;
                    effect.DirectionalLight0.Direction = be.DirectionalLight0.Direction;
                    effect.DirectionalLight0.Enabled = be.DirectionalLight0.Enabled;
                    effect.Projection = be.Projection;
                    effect.View = be.View;
                    effect.World = world;
                }
                mesh.Draw();
            }
        }

        public float CollisionRadius()
        {
            return boundingSphere.Radius;
        }

        public void DrawCollistion(Matrix view, Matrix projection, Vector3 position)
        {
            Matrix world =
                renderMatrix *
                Matrix.CreateTranslation(position);

            foreach (ModelMesh mesh in Model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.LightingEnabled = false;
                    effect.Projection = projection;
                    effect.View = view;
                    effect.World = world;
                }
                mesh.Draw();
            }
        }

        public void HandleController(PlayerControl control, double elapsedTime)
        {
            OldPosition = Position;

            float rotation = control.Rotation;
            float thrust = control.Thrust;

            Rotation.Z += (float)(-rotation * elapsedTime * 7);
            Rotation.Y = rotation * 3.14f / 2f;
            velocityPosistionCalculator.CalcVelocityAndPosition(ref Position, ref Velocity, elapsedTime, Rotation.Z, thrust * 100);

            if (!prevFire && control.Fire)
            {
                var gunLocation = Vector3.Transform(new Vector3(0, boundingSphere.Radius, 0), renderMatrix);
                missleProjectileFactory.Fire(Position + gunLocation, Rotation.Z, Velocity);
            }
            prevFire = control.Fire;
        }
    }
}
