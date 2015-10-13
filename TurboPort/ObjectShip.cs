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

        internal Vector3  oldPosition;

        private BoundingBox    boundingBox;
        private BoundingSphere boundingSphere;
        private Vector3[] colisionPoints;
        private bool prevFire;
        private Matrix renderMatrix;
        private bool shipColliding;
        private Vector3 bodyColor;
        private Vector3 screenColor;
        private BasicEffect bodyEffect;
        private BasicEffect screenEffect;

        private Model model;
        public bool HasLanded { get; set; }


        static  bool colorSwitchHack;
        public bool hit;

        public ObjectShip(IMissleProjectileFactory missleProjectileFactory, BulletBuffer bulletBuffer, ILevelBackground levelBackground)
        {
            this.missleProjectileFactory = missleProjectileFactory;
            this.bulletBuffer = bulletBuffer;
            this.levelBackground = levelBackground;
            HasLanded = true;
        }

        public void Initialize(ContentManager content)
        {
            model = content.Load<Model>(@"objects/pop_simple_lemming3");
            CorrectModel(model);

            bodyEffect = (BasicEffect)model.Meshes["Body"].Effects[0];
            screenEffect = (BasicEffect)model.Meshes["Screen"].Effects[0];
            bodyColor = colorSwitchHack ? screenEffect.DiffuseColor : bodyEffect.DiffuseColor;
            screenColor = colorSwitchHack ? bodyEffect.DiffuseColor : screenEffect.DiffuseColor;
            colorSwitchHack = !colorSwitchHack;

            centerOffset = new Vector3(0f, 0f, 0f);
            scale = 1.0f;
            Position = new Vector3(160f, 160f, 0f);
            Velocity = new Vector3(0f, 0f, 0f);
            Rotation = Vector3.Zero;

            MeshUtil.GetBoundingFromMeshes(model.Meshes, scale, out boundingBox, out boundingSphere);

            var originalBox = boundingBox;
            originalBox.Min /= scale;
            originalBox.Max /= scale;
            colisionPoints = MeshUtil.FindCollisionPoints(model.Meshes, originalBox);
        }

        public void Bots(ObjectShip other)
        {
            Vector3 distance = other.Position - Position;
            if(distance.Length() < (boundingSphere.Radius * scale))
            {
                Position = oldPosition;
                other.Position = other.oldPosition;

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

            if (HasLanded && Velocity.Y == 0)
                return;

            //DoOldCollsionWithBackground();
        }

        private void DoOldCollsionWithBackground()
        {
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
                Position = oldPosition;
                Velocity = -Velocity*0.3f;
                SoundHandler.Checkpoint();
            }
            shipColliding = isColliding;
        }

        public bool CheckCollision(Vector3 projectilePosition, CollisionPositionInTexture collisionPositionInTexture)
        {
            var projectileRelativeToCenter = Position - projectilePosition;
            var containmentType = boundingSphere.Contains(projectileRelativeToCenter);
            if (containmentType != ContainmentType.Disjoint)
            {
                int offsetX = collisionPositionInTexture.Rect.X + (collisionPositionInTexture.Rect.Width / 2) +
                              (int) projectileRelativeToCenter.X;
                int offsetY = collisionPositionInTexture.Rect.Y + (collisionPositionInTexture.Rect.Height / 2) +
                              (int) projectileRelativeToCenter.Y;
                byte textureDataAtLocation = collisionPositionInTexture.ByteData[offsetX + offsetY * collisionPositionInTexture.Size.X];
                if (textureDataAtLocation == 0)
                    return false;

                SoundHandler.Bigexp();
                return true;
            }

            return false;
        }

        public void Render(GraphicsDevice graphicsDevice, BasicEffect be)
        {
            var world = renderMatrix *
                Matrix.CreateTranslation(Position);

            graphicsDevice.DepthStencilState = DepthStencilState.Default;
            bodyEffect.DiffuseColor = hit ? new Vector3(1, 0,0) : bodyColor;
            screenEffect.DiffuseColor = screenColor;

            foreach (ModelMesh mesh in model.Meshes)
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

        public void DrawToCollistionTexture(Matrix view, Matrix projection, Vector3 position)
        {
            Matrix world =
                renderMatrix *
                Matrix.CreateTranslation(position);

            foreach (ModelMesh mesh in model.Meshes)
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
            oldPosition = Position;

            float rotation = control.Rotation;
            float thrust = control.Thrust;
            if (thrust != 0)
                HasLanded = false;

            Rotation.Y = rotation * 3.14f / 2f;
            if (!HasLanded)
            {
                Rotation.Z += (float)(-rotation * elapsedTime * 7);
                velocityPosistionCalculator.CalcVelocityAndPosition(ref Position, ref Velocity,
                    elapsedTime, Rotation.Z, thrust*100);}

            if (!prevFire && control.Fire)
            {
                var gunLocation = Vector3.Transform(new Vector3(0, boundingSphere.Radius, 0), renderMatrix);
                missleProjectileFactory.Fire(Position + gunLocation, Rotation.Z, Velocity);
            }
            prevFire = control.Fire;
        }
    }
}
