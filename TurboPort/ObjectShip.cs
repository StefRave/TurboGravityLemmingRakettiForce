using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TurboPort.Input;

namespace TurboPort
{
    public class ObjectShip : IControllerInputProcessor, I3DCollistionObject
    {
        private readonly IMissileProjectileFactory missileProjectileFactory;
        private Vector3  centerOffset;
        private float    scale;
        private readonly VelocityPositionCalculator velocityPositionCalculator = new VelocityPositionCalculator { Mass = 35 };

        public Vector3  Rotation;
        public Vector3  Position;
        public Vector3  Velocity;

        private BoundingSphere boundingSphere;
        private bool prevFire;
        private Matrix renderMatrix;
        private Vector3 bodyColor;
        private Vector3 screenColor;
        private BasicEffect bodyEffect;
        private BasicEffect screenEffect;

        private Model model;
        public bool HasLanded { get; set; }


        static  bool colorSwitchHack;
        public bool hit;

        public ObjectShip(IMissileProjectileFactory missileProjectileFactory)
        {
            this.missileProjectileFactory = missileProjectileFactory;
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

            MeshUtil.GetBoundingFromMeshes(model.Meshes, scale, out boundingSphere);
        }

        public void Bots(ObjectShip other)
        {
            Vector3 distance = other.Position - Position;
            if(distance.Length() < (boundingSphere.Radius * scale))
            {
                Collide(this, other);

                SoundHandler.Shipcollide();
            }
        }

        // Collision from http://www.gamasutra.com/view/feature/3015/pool_hall_lessons_fast_accurate_.php?page=3
        private static void Collide(ObjectShip circle1, ObjectShip circle2)
        {
            // First, find the normalized vector n from the center of 
            // circle1 to the center of circle2
            Vector3 n = circle1.Position - circle2.Position;
            n.Normalize();
            // Find the length of the component of each of the movement
            // vectors along n. 
            // a1 = v1 . n
            // a2 = v2 . n
            float a1 = Vector3.Dot(circle1.Velocity, n);
            float a2 = Vector3.Dot(circle2.Velocity, n);

            float mass1 = circle1.velocityPositionCalculator.Mass;
            if (circle1.HasLanded)
                mass1 *= 10;
            float mass2 = circle2.velocityPositionCalculator.Mass;
            if (circle2.HasLanded)
                mass2 *= 10;
            // Using the optimized version, 
            // optimizedP =  2(a1 - a2)
            //              -----------
            //                m1 + m2
            float optimizedP = (2.0f * (a1 - a2)) / (mass1  + mass2);

            // Math.min added to prevent collisions between balls moving away
            // from one another, thus preventing "entanglement".
            // http://gamedev.stackexchange.com/a/15936
            //optimizedP = MathHelper.Max(optimizedP, 0);

            // Calculate v1', the new movement vector of circle1
            // v1' = v1 - optimizedP * m2 * n
            Vector3 v1n = circle1.Velocity - optimizedP * mass2 * n;

            // Calculate v1', the new movement vector of circle1
            // v2' = v2 + optimizedP * m1 * n
            Vector3 v2n = circle2.Velocity + optimizedP * mass1 * n;

            circle1.Velocity = v1n;
            circle2.Velocity = v2n;
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
                int textureDataAtLocation = collisionPositionInTexture.ByteData[offsetX + offsetY * collisionPositionInTexture.Size.X];
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

        public void ProcessControllerInput(PlayerControl control, double elapsedTime)
        {
            float rotation = control.Rotation;
            float thrust = control.Thrust;
            if (thrust != 0)
                HasLanded = false;

            Rotation.Y = rotation * 3.14f / 2f;
            if (HasLanded)
            {
                Rotation = Vector3.Zero;
                Velocity = Vector3.Zero;
            }
            else
            {
                Rotation.Z += (float)(-rotation * elapsedTime * 7);
                velocityPositionCalculator.CalcVelocityAndPosition(ref Position, ref Velocity,
                    elapsedTime, Rotation.Z, thrust*100);}

            if (!prevFire && control.Fire)
            {
                var gunLocation = Vector3.Transform(new Vector3(0, boundingSphere.Radius, 0), renderMatrix);
                missileProjectileFactory.Fire(Position + gunLocation, Rotation.Z, Velocity);
            }
            prevFire = control.Fire;
        }
    }
}
