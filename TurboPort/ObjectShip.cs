using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using ProtoBuf;
using TurboPort.Event;
using TurboPort.Input;

namespace TurboPort
{
    [ProtoContract()]
    [GameEvent("Ship")]
    public class ObjectShip : GameObject, IControllerInputProcessor, I3DCollistionObject
    {
        private readonly IMissileProjectileFactory missileProjectileFactory;
        private static Vector3  centerOffset;
        private static float    scale;
        private static BoundingSphere boundingSphere;

        private readonly VelocityPositionCalculator velocityPositionCalculator = new VelocityPositionCalculator { Mass = 35 };
        private Vector3  rotation;
        [ProtoMember(1)] private Vector3 position;
        [ProtoMember(2)] private Vector3  velocity;
        [ProtoMember(3)] private float controlRotation;
        [ProtoMember(4)] private float controlThrust;

        [ProtoMember(5)]
        private int EventsWire { get { return (int) events; } set { events = (Event) value; } }

        private Event events;

        private bool prevFire;
        private Matrix renderMatrix;
        private static Vector3 bodyColor;
        private static Vector3 screenColor;
        private static BasicEffect bodyEffect;
        private static BasicEffect screenEffect;

        private static Model model;
        public bool HasLanded { get; private set; }
        public bool Hit { get; set; }
        public Vector3 Rotation => rotation;
        public Vector3 Position => position;
        public Vector3 Velocity => velocity;


        static bool colorSwitchHack;


        public ObjectShip(IMissileProjectileFactory missileProjectileFactory)
        {
            this.missileProjectileFactory = missileProjectileFactory;
            events = Event.Create;
        }

        public static void Initialize(ContentManager content)
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
            boundingSphere = MeshUtil.GetBoundingFromMeshes(model.Meshes, scale);
        }

        public void Bots(ObjectShip other)
        {
            Vector3 distance = other.position - position;
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
            Vector3 n = circle1.position - circle2.position;
            n.Normalize();
            // Find the length of the component of each of the movement
            // vectors along n. 
            // a1 = v1 . n
            // a2 = v2 . n
            float a1 = Vector3.Dot(circle1.velocity, n);
            float a2 = Vector3.Dot(circle2.velocity, n);

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
            Vector3 v1n = circle1.velocity - optimizedP * mass2 * n;

            // Calculate v1', the new movement vector of circle1
            // v2' = v2 + optimizedP * m1 * n
            Vector3 v2n = circle2.velocity + optimizedP * mass1 * n;

            circle1.velocity = v1n;
            circle2.velocity = v2n;
        }

        private static void CorrectModel(Model model)
        {
            foreach (var mesh in model.Meshes)
                foreach (BasicEffect leffect in mesh.Effects)
                    leffect.Alpha = 1;
        }

        public void Update(GameTime gameTime)
        {
            double elapsedTime = gameTime.ElapsedGameTime.TotalSeconds;
            rotation.Y = controlRotation * 3.14f / 2f;
            if (controlThrust > 0)
                HasLanded = false;
            if (!HasLanded)
            {
                rotation.Z += (float)(-controlRotation * elapsedTime * 7);
                velocityPositionCalculator.CalcVelocityAndPosition(ref position, ref velocity,
                    elapsedTime, rotation.Z, controlThrust * 100);
            }

            renderMatrix =
                Matrix.CreateTranslation(new Vector3(centerOffset.X, centerOffset.Y, centerOffset.Z))*
                Matrix.CreateRotationY(rotation.Y)*
                Matrix.CreateRotationZ(rotation.Z);
        }

        public bool CheckCollision(Vector3 projectilePosition, CollisionPositionInTexture collisionPositionInTexture)
        {
            var projectileRelativeToCenter = position - projectilePosition;
            var containmentType = boundingSphere.Contains(projectileRelativeToCenter);
            if (containmentType != ContainmentType.Disjoint)
            {
                int offsetX = collisionPositionInTexture.Rect.X + (collisionPositionInTexture.Rect.Width / 2) +
                              (int) projectileRelativeToCenter.X;
                int offsetY = collisionPositionInTexture.Rect.Y + (collisionPositionInTexture.Rect.Height / 2) +
                              (int) projectileRelativeToCenter.Y;
                int textureDataAtLocation = 0;//!!!!!!!!collisionPositionInTexture.CollisionData[offsetX + offsetY * collisionPositionInTexture.Size.X];
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
                Matrix.CreateTranslation(position);

            graphicsDevice.DepthStencilState = DepthStencilState.Default;
            bodyEffect.DiffuseColor = Hit ? new Vector3(1, 0,0) : bodyColor;
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

        public void ProcessControllerInput(PlayerControl control)
        {
            if ((controlRotation != control.Rotation) ||
                (controlThrust != control.Thrust))
            {
                controlRotation = control.Rotation;
                controlThrust = control.Thrust;
                PublishEvent();
            }

            if (!prevFire && control.Fire)
            {
                var gunLocation = Vector3.Transform(new Vector3(0, boundingSphere.Radius, 0), renderMatrix);
                Vector3 vector3 = velocity;
                Vector3 myRotation = gunLocation;
                myRotation.Normalize();

                missileProjectileFactory.Fire(position + gunLocation, rotation.Z, vector3 + (myRotation * 140));
            }
            prevFire = control.Fire;
        }

        protected internal override void ProcessGameEvents()
        {
            if (events.HasFlag(Event.Create))
                ApplyCreate();
            if (events.HasFlag(Event.Landed))
                ApplyHasLanded();
            events = 0;
        }

        protected internal override void ObjectStored()
        {
            events = 0;
        }

        public void LandShip(float positionY)
        {
            position.Y = positionY;
            rotation.Z = 0;

            events |= Event.Landed;
            PublishEvent();
            ApplyHasLanded();
        }

        public void ApplyHasLanded()
        {
            HasLanded = true;
            rotation.Z = 0;
            velocity = Vector3.Zero;
            controlRotation = 0;

            SoundHandler.TochDown();
        }

        public void HitWithBackground()
        {
            velocity = velocity * 0.3f;
            events |= Event.HitBackground;
            PublishEvent();
            ApplyHitWithBackground();
        }

        public void ApplyHitWithBackground()
        {
        }

        public void CreateInitialize(Vector3 startPosition)
        {
            position = startPosition;
            ApplyCreate();
        }

        private void ApplyCreate()
        {
            HasLanded = true;
        }

        [Flags]
        public enum Event
        {
            Create = 1 << 0,
            Landed = 1 << 1,
            HitBackground = 1 << 2,
        }
    }
}
