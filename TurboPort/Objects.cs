using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using OpenTK.Graphics.OpenGL;

namespace TurboPort
{
    public interface IHandleController
    {
        void HandleController(PlayerControl control, double timeElapsed);
    }

    public class ShipBase
    {
        public Vector3 Position;
        
        public void Interact(ObjectShip other)
        {
            if(other.Speed.Y <= 0)
            {
                if(Math.Abs(other.Position.X - Position.X) < 16)
                {
                    if(other.Position.Y <= Position.Y)
                    {
                        if(other.OldPosition.Y >= Position.Y)
                        {
                            double pop = Math.Cos(other.Rotation.Z);
                            if(pop >= 0.93)
                            {
                                if(other.OldPosition.Y != Position.Y)
                                {
                                    SoundHandler.TochDown();
                                }
                                other.Speed = Vector3.Zero;
                                other.Position.Y = Position.Y;
                                other.Rotation.Z = 0;
                            }
                            else
                            {
                                SoundHandler.Bigexp();
                            }
                        }
                    }
                }
            }
        }

        public ShipBase(Vector3 position)
        {
            this.Position = position;
        }
    }

    public class ObjectShip : IHandleController, I3DCollistionObject
    {
        private readonly IMissleProjectileFactory missleProjectileFactory;
        private readonly BulletBuffer bulletBuffer;
        private readonly ILevelBackground levelBackground;
        private Vector3  centerOffset;
        private float    scale;

        public Vector3  Rotation;
        public Vector3  Position;
        public Vector3  Speed;

        public Vector3 GunPosition;
        public Vector3 ShootingVelocity;
        public Vector3  OldPosition;

        private BoundingBox    boundingBox;
        private BoundingSphere boundingSphere;
        public Vector3[] colisionPoints; 
        //public Vector3[] translatedPoints; 
        public Vector3 bodyColor;

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

            Vector3 originalColor = ((BasicEffect)Model.Meshes[0].Effects[0]).DiffuseColor;
            bodyColor = colorSwitchHack ? new Vector3(originalColor.X, originalColor.Z, originalColor.Y) : originalColor;
            colorSwitchHack = !colorSwitchHack;

            centerOffset = new Vector3(0f, 0f, 0f);
            scale = 10.0f;
            Position = new Vector3(160f, 160f, 0f);
            Speed = new Vector3(0f, 0f, 0f);
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
                //Speed = -Speed;
                Position = OldPosition;
                other.Position = other.OldPosition;

                Vector3 tmp = other.Speed;
                other.Speed = Speed; // + Vector3.Scale(Speed, (float)0.5);
                Speed = tmp; // + Vector3.Scale(tmp, (float)0.5);
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
                                      Matrix.CreateScale(scale) *
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
                Speed = -Speed * 0.3f;
                SoundHandler.Checkpoint();
            }
            shipColliding = isColliding;
        }

        public void Render(BasicEffect be)
        {
            var world = renderMatrix *
                Matrix.CreateScale(scale) *
                Matrix.CreateTranslation(Position);

            ((BasicEffect)Model.Meshes[0].Effects[0]).DiffuseColor = bodyColor;

            foreach(ModelMesh mesh in Model.Meshes)
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
                Matrix.CreateScale(10)*
                Matrix.CreateTranslation(position);

            Model.Draw(world, view, projection);
        }

        bool prevFire = false;
        private Matrix renderMatrix;
        private bool shipColliding;

        public void HandleController(PlayerControl control, double dt)
        {
            OldPosition = Position;

            if(!prevFire && control.Fire)
            {
                missleProjectileFactory.Fire(GunPosition, ShootingVelocity);
            }
            prevFire = control.Fire;

            //double g = TestSettings.Value1; // 50
            //double z = TestSettings.Value2; // 0.7
            //double t = TestSettings.Value3; // 200
            double g = 80;
            double z = 0.7;
            double t = 250;

            Rotation.Z += (float)(-control.Rotation * dt * 7);
            Rotation.Y = control.Rotation * 3.14f /2f;
            var gc = t * Math.Sin(-Rotation.Z) * control.Thrust;
            var vzg = Speed.X*z-gc;
            Position.X += (float)((gc*dt+(vzg-vzg*Math.Exp(-z*dt))/z)/z);
            Speed.X = (float)(gc/z+Math.Exp(-z*dt)*vzg/z);
            
            gc = -g + t * Math.Cos( Rotation.Z) * control.Thrust;
            vzg = Speed.Y*z-gc;
            Position.Y += (float)((gc*dt+(vzg-vzg*Math.Exp(-z*dt))/z)/z);
            Speed.Y = (float)(gc/z+Math.Exp(-z*dt)*vzg/z);


            var gunLocation = Vector3.Transform(new Vector3(0, boundingSphere.Radius, 0), renderMatrix);
            ShootingVelocity = Vector3.Normalize(gunLocation) + (Speed * 0.02f);
            GunPosition = Position + gunLocation;
        }
    }
}
