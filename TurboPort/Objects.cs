using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

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

    public class ObjectShip : IHandleController
    {
        private readonly IMissleProjectileFactory missleProjectileFactory;
        public Vector3  centerOffset;
        public float    scale;

        public Vector3  Rotation;
        public Vector3  Position;
        public Vector3  Speed;

        public Vector3 GunPosition;
        public Vector3 ShootingVelocity;
        public Vector3  OldPosition;

		public BoundingBox    boundingBox;
		public BoundingSphere boundingSphere;
        public Vector3[] colisionPoints; 
        public Matrix   renderMatrix;
        //public Vector3[] translatedPoints; 
        public bool hackColliding = false;
        private Model model;
        public Vector3 bodyColor;

		// NOTE (san): temp solution until we have a proper renderer again
		public Texture2D texture;

        public Model Model
        {
            get { return model; }
        }

        //public Vector3  Position { get { return position; } set { position = value; } }
        //public Vector3  Speed    { get { return speed; }    set { speed    = value; } }

        static  bool colorSwitchHack;

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

        public static Vector3[] FindCollisionPoints(ModelMeshCollection mesh, BoundingBox boundingBox)
        {
			Vector3 inset = (boundingBox.Max - boundingBox.Min) * 0.01f;
            boundingBox.Min += inset;
            boundingBox.Max -= inset;

            Vector3[] collisionPoints = new Vector3[6];
            int[] numberAdded = new int[6];

            foreach(ModelMesh modelMesh in mesh)
            {
                foreach(ModelMeshPart meshPart in modelMesh.MeshParts)
                {
                    int vertexStride = meshPart.VertexBuffer.VertexDeclaration.VertexStride;

                    Vector3[] vertices = new Vector3[meshPart.NumVertices];
                    meshPart.VertexBuffer.GetData(0, vertices, 0, meshPart.NumVertices, vertexStride);

                    foreach(Vector3 vertex in vertices)
                    {
                        if(vertex.X < boundingBox.Min.X)
                        {
                            collisionPoints[0] += vertex;
                            numberAdded[0]++;
                        }
                        else if(vertex.X > boundingBox.Max.X)
                        {
                            collisionPoints[1] += vertex;
                            numberAdded[1]++;
                        }
                        if(vertex.Y < boundingBox.Min.Y)
                        {
                            collisionPoints[2] += vertex;
                            numberAdded[2]++;
                        }
                        else if(vertex.Y > boundingBox.Max.Y)
                        {
                            collisionPoints[3] += vertex;
                            numberAdded[3]++;
                        }
                        if(vertex.Z < boundingBox.Min.Z)
                        {
                            collisionPoints[4] += vertex;
                            numberAdded[4]++;
                        }
                        else if(vertex.Z > boundingBox.Max.Z)
                        {
                            collisionPoints[5] += vertex;
                            numberAdded[5]++;
                        }
                    }
                }
            }
            for(int i = 0; i < collisionPoints.Length; i++)
            {
                collisionPoints[i] *= 1f / numberAdded[i];
            }
			collisionPoints[0].X = boundingBox.Min.X;
			collisionPoints[1].X = boundingBox.Max.X;
			collisionPoints[2].Y = boundingBox.Min.Y;
			collisionPoints[3].Y = boundingBox.Max.Y;
			collisionPoints[4].Z = boundingBox.Min.Z;
			collisionPoints[5].Z = boundingBox.Max.Z;
            
            return collisionPoints;
        }

        public static ObjectShip CreateShip(GraphicsDevice device, ContentManager content, IMissleProjectileFactory missleProjectileFactory)
        {
            ObjectShip result = new ObjectShip(missleProjectileFactory);
            result.model = content.Load<Model>(@"objects/pop_simple_lemming3");
            CorrectModel(result.model);
            // NOTE (san): temp solution until we have a proper renderer again
            result.texture = content.Load<Texture2D>(@"gfx/particle");

            Vector3 originalColor = ((BasicEffect)result.model.Meshes[0].Effects[0]).DiffuseColor;
            result.bodyColor = colorSwitchHack ? new Vector3(originalColor.X, originalColor.Z, originalColor.Y) : originalColor;
            colorSwitchHack = !colorSwitchHack;

            result.centerOffset = new Vector3(0f, 0f, 0f);
            result.scale    = 5.0f;
            result.Position = new Vector3(160f, 160f, 0f);
            result.Speed    = new Vector3(0f, 0f, 0f);
            result.Rotation = Vector3.Zero;

            GetBoundingFromMeshes(result.model.Meshes, out result.boundingBox, out result.boundingSphere);
            result.colisionPoints = FindCollisionPoints(result.model.Meshes, result.boundingBox);

            return result;
        }

        private static void CorrectModel(Model model)
        {
            foreach (var mesh in model.Meshes)
                foreach (BasicEffect leffect in mesh.Effects)
                    leffect.Alpha = 1;
        }

        private static void GetBoundingFromMeshes(IList<ModelMesh> meshes, out BoundingBox boundingBox, out BoundingSphere boundingSphere)
        {
            boundingBox = CreateBoundingBox(meshes);

            boundingSphere = meshes[0].BoundingSphere;
            for (int i = 1; i < meshes.Count; i++)
                boundingSphere = BoundingSphere.CreateMerged(boundingSphere, meshes[i].BoundingSphere);
        }

        public static BoundingBox CreateBoundingBox(IList<ModelMesh> meshes)
        {
            // Initialize minimum and maximum corners of the bounding box to max and min values
            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            // For each mesh of the model
            foreach (ModelMesh mesh in meshes)
            {
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    int vertexStride = meshPart.VertexBuffer.VertexDeclaration.VertexStride;

                    Vector3[] vertexData = new Vector3[meshPart.NumVertices];
                    meshPart.VertexBuffer.GetData(0, vertexData, 0, meshPart.NumVertices, vertexStride);

                    for (int i = 0; i < meshPart.NumVertices; i++)
                    {
                        Vector3 transformedPosition = vertexData[i];

                        min = Vector3.Min(min, transformedPosition);
                        max = Vector3.Max(max, transformedPosition);
                    }
                }
            }
            return new BoundingBox(min, max);
        }


        public void Render(GraphicsDevice device, BasicEffect be, int p1, int p2)
        {
            renderMatrix =
                Matrix.CreateTranslation(new Vector3(centerOffset.X, centerOffset.Y, centerOffset.Z)) *
                Matrix.CreateRotationY(Rotation.Y) *
                Matrix.CreateRotationZ(Rotation.Z) *
                Matrix.CreateScale(10) *
                Matrix.CreateTranslation(Position);

            ((BasicEffect)model.Meshes[0].Effects[0]).DiffuseColor = bodyColor;

            foreach(ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.LightingEnabled = be.LightingEnabled;
                    effect.AmbientLightColor = be.AmbientLightColor;
                    effect.DirectionalLight0.DiffuseColor = be.DirectionalLight0.DiffuseColor;
                    effect.DirectionalLight0.Direction = be.DirectionalLight0.Direction;
                    effect.DirectionalLight0.Enabled = be.DirectionalLight0.Enabled;
                    effect.DirectionalLight0.SpecularColor = be.DirectionalLight0.SpecularColor;
                    effect.Projection = be.Projection;
                    effect.View = be.View;
                    effect.World = renderMatrix;
                }
                mesh.Draw();
            }
        }

        bool prevFire = false;

        private ObjectShip(IMissleProjectileFactory missleProjectileFactory)
        {
            this.missleProjectileFactory = missleProjectileFactory;
        }

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

            Matrix matrix =
                Matrix.CreateTranslation(new Vector3(centerOffset.X, centerOffset.Y, centerOffset.Z))*
                Matrix.CreateRotationY(Rotation.Y)*
                Matrix.CreateRotationZ(Rotation.Z)*
                Matrix.CreateScale(10);
            var gunLocation = Vector3.Transform(new Vector3(0, boundingSphere.Radius, 0), matrix);
            ShootingVelocity = Vector3.Normalize(gunLocation) + (Speed * 0.02f);
            GunPosition = Position + gunLocation;
        }
    }
}
