#region File Description
//-----------------------------------------------------------------------------
// ParticleSystem.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace TurboPort.ParticleSystems
{
    /// <summary>
    /// The main component in charge of displaying particles.
    /// </summary>
    public abstract class ParticleSystem : DrawableGameComponent
    {
        #region Fields


        // Settings class controls the appearance and animation of this particle system.
        ParticleSettings settings = new ParticleSettings();
        private BasicEffect basicEffect;
        private VertexPositionColorTexture[] basicVertexes;

        // For loading the effect and particle texture.
        IContentLoader content;


        // Custom effect for drawing particles. This computes the particle
        // animation entirely in the vertex shader: no per-particle CPU work required!
        Effect particleEffect;


        // Shortcuts for accessing frequently changed effect parameters.
        EffectParameter effectViewParameter;
        EffectParameter effectProjectionParameter;
        EffectParameter effectViewportScaleParameter;
        EffectParameter effectTimeParameter;


        // An array of particles, treated as a circular queue.
        ParticleVertex[] particles;


        // A vertex buffer holding our particles. This contains the same data as
        // the particles array, but copied across to where the GPU can access it.
        DynamicVertexBuffer vertexBuffer;


        // Index buffer turns sets of four vertices into particle quads (pairs of triangles).
        IndexBuffer indexBuffer;


        // The particles array and vertex buffer are treated as a circular queue.
        // Initially, the entire contents of the array are free, because no particles
        // are in use. When a new particle is created, this is allocated from the
        // beginning of the array. If more than one particle is created, these will
        // always be stored in a consecutive block of array elements. Because all
        // particles last for the same amount of time, old particles will always be
        // removed in order from the start of this active particle region, so the
        // active and free regions will never be intermingled. Because the queue is
        // circular, there can be times when the active particle region wraps from the
        // end of the array back to the start. The queue uses modulo arithmetic to
        // handle these cases. For instance with a four entry queue we could have:
        //
        //      0
        //      1 - first active particle
        //      2 
        //      3 - first free particle
        //
        // In this case, particles 1 and 2 are active, while 3 and 4 are free.
        // Using modulo arithmetic we could also have:
        //
        //      0
        //      1 - first free particle
        //      2 
        //      3 - first active particle
        //
        // Here, 3 and 0 are active, while 1 and 2 are free.
        //
        // But wait! The full story is even more complex.
        //
        // When we create a new particle, we add them to our managed particles array.
        // We also need to copy this new data into the GPU vertex buffer, but we don't
        // want to do that straight away, because setting new data into a vertex buffer
        // can be an expensive operation. If we are going to be adding several particles
        // in a single frame, it is faster to initially just store them in our managed
        // array, and then later upload them all to the GPU in one single call. So our
        // queue also needs a region for storing new particles that have been added to
        // the managed array but not yet uploaded to the vertex buffer.
        //
        // Another issue occurs when old particles are retired. The CPU and GPU run
        // asynchronously, so the GPU will often still be busy drawing the previous
        // frame while the CPU is working on the next frame. This can cause a
        // synchronization problem if an old particle is retired, and then immediately
        // overwritten by a new one, because the CPU might try to change the contents
        // of the vertex buffer while the GPU is still busy drawing the old data from
        // it. Normally the graphics driver will take care of this by waiting until
        // the GPU has finished drawing inside the VertexBuffer.SetData call, but we
        // don't want to waste time waiting around every time we try to add a new
        // particle! To avoid this delay, we can specify the SetDataOptions.NoOverwrite
        // flag when we write to the vertex buffer. This basically means "I promise I
        // will never try to overwrite any data that the GPU might still be using, so
        // you can just go ahead and update the buffer straight away". To keep this
        // promise, we must avoid reusing vertices immediately after they are drawn.
        //
        // So in total, our queue contains four different regions:
        //
        // Vertices between firstActiveParticle and firstNewParticle are actively
        // being drawn, and exist in both the managed particles array and the GPU
        // vertex buffer.
        //
        // Vertices between firstNewParticle and firstFreeParticle are newly created,
        // and exist only in the managed particles array. These need to be uploaded
        // to the GPU at the start of the next draw call.
        //
        // Vertices between firstFreeParticle and firstRetiredParticle are free and
        // waiting to be allocated.
        //
        // Vertices between firstRetiredParticle and firstActiveParticle are no longer
        // being drawn, but were drawn recently enough that the GPU could still be
        // using them. These need to be kept around for a few more frames before they
        // can be reallocated.

        int firstActiveParticle;
        int firstNewParticle;
        int firstFreeParticle;
        int firstRetiredParticle;


        // Store the current time, in seconds.
        float currentTime;


        // Count how many times Draw has been called. This is used to know
        // when it is safe to retire old particles back into the free list.
        int drawCounter;


        // Shared random number generator.
        static Random random = new Random();
        private short[] indices;

        #endregion

        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        protected ParticleSystem(Game game, IContentLoader content)
            : base(game)
        {
            this.content = content;
        }


        /// <summary>
        /// Initializes the component.
        /// </summary>
        public override void Initialize()
        {
            InitializeSettings(settings);

            // Allocate the particle array, and fill in the corner fields (which never change).
            particles = new ParticleVertex[settings.MaxParticles * 4];
            basicVertexes = new VertexPositionColorTexture[settings.MaxParticles * 4];

            for (int i = 0; i < settings.MaxParticles; i++)
            {
                particles[i * 4 + 0].Corner = new Vector2(-1, -1);
                particles[i * 4 + 1].Corner = new Vector2(1, -1);
                particles[i * 4 + 2].Corner = new Vector2(1, 1);
                particles[i * 4 + 3].Corner = new Vector2(-1, 1);
            }
            for (int i = 0; i < settings.MaxParticles; i++)
            {
                basicVertexes[i * 4 + 0].TextureCoordinate = new Vector2(0, 0);
                basicVertexes[i * 4 + 1].TextureCoordinate = new Vector2(1, 0);
                basicVertexes[i * 4 + 2].TextureCoordinate = new Vector2(1, 1);
                basicVertexes[i * 4 + 3].TextureCoordinate = new Vector2(0, 1);
                basicVertexes[i * 4 + 0].Color = Color.DarkRed;
                basicVertexes[i * 4 + 1].Color = Color.DarkRed;
                basicVertexes[i * 4 + 2].Color = Color.DarkRed;
            }

            basicEffect = new BasicEffect(GraphicsDevice);

            base.Initialize();
        }


        /// <summary>
        /// Derived particle system classes should override this method
        /// and use it to initalize their tweakable settings.
        /// </summary>
        protected abstract void InitializeSettings(ParticleSettings settings);


        /// <summary>
        /// Loads graphics for the particle system.
        /// </summary>
        protected override void LoadContent()
        {
            try
            {
                LoadParticleEffect();
            }
            catch (ContentLoadException)
            {
                LoadBackupBasicEffect();
            }

            // Create a dynamic vertex buffer.
            vertexBuffer = new DynamicVertexBuffer(GraphicsDevice, ParticleVertex.VertexDeclaration,
                                                   settings.MaxParticles * 4, BufferUsage.WriteOnly);

            // Create and populate the index buffer.
            indices = new short[settings.MaxParticles * 6];

            for (int i = 0; i < settings.MaxParticles; i++)
            {
                indices[i * 6 + 0] = (short)(i * 4 + 0);
                indices[i * 6 + 1] = (short)(i * 4 + 1);
                indices[i * 6 + 2] = (short)(i * 4 + 2);

                indices[i * 6 + 3] = (short)(i * 4 + 0);
                indices[i * 6 + 4] = (short)(i * 4 + 2);
                indices[i * 6 + 5] = (short)(i * 4 + 3);
            }

            indexBuffer = new IndexBuffer(GraphicsDevice, typeof(ushort), indices.Length, BufferUsage.WriteOnly);

            indexBuffer.SetData(indices);
        }


        /// <summary>
        /// Helper for loading and initializing the particle effect.
        /// </summary>
        void LoadParticleEffect()
        {
            var effect = content.Load<Effect>("ParticleEffect");

            // If we have several particle systems, the content manager will return
            // a single shared effect instance to them all. But we want to preconfigure
            // the effect with parameters that are specific to this particular
            // particle system. By cloning the effect, we prevent one particle system
            // from stomping over the parameter settings of another.
            
            particleEffect = effect.Clone();

            EffectParameterCollection parameters = particleEffect.Parameters;

            // Look up shortcuts for parameters that change every frame.
            effectViewParameter = parameters["View"];
            effectProjectionParameter = parameters["Projection"];
            effectViewportScaleParameter = parameters["ViewportScale"];
            effectTimeParameter = parameters["CurrentTime"];

            // Set the values of parameters that do not change.
            parameters["Duration"].SetValue((float)settings.Duration.TotalSeconds);
            parameters["DurationRandomness"].SetValue(settings.DurationRandomness);
            parameters["Gravity"].SetValue(settings.Gravity);
            parameters["EndVelocity"].SetValue(settings.EndVelocity);
            parameters["MinColor"].SetValue(settings.MinColor.ToVector4());
            parameters["MaxColor"].SetValue(settings.MaxColor.ToVector4());

            parameters["RotateSpeed"].SetValue(
                new Vector2(settings.MinRotateSpeed, settings.MaxRotateSpeed));
            
            parameters["StartSize"].SetValue(
                new Vector2(settings.MinStartSize, settings.MaxStartSize));
            
            parameters["EndSize"].SetValue(
                new Vector2(settings.MinEndSize, settings.MaxEndSize));

            // Load the particle texture, and set it onto the effect.
            Texture2D texture = content.Load<Texture2D>(settings.TextureName);

            parameters["Texture"].SetValue(texture);
        }

        void LoadBackupBasicEffect()
        {
            // Load the particle texture, and set it onto the effect.
            Texture2D texture = content.Load<Texture2D>(settings.TextureName);

            basicEffect.Texture = texture;
            basicEffect.TextureEnabled = true;
            basicEffect.World = Matrix.Identity;
            basicEffect.LightingEnabled = false;
            basicEffect.AmbientLightColor = new Vector3(1f, 1f, 1f);

            basicEffect.DirectionalLight0.Enabled = true;
            basicEffect.DirectionalLight0.SpecularColor = new Vector3(0f, 0f, 0f);
            basicEffect.LightingEnabled = false;
            basicEffect.VertexColorEnabled = true;
        }

        #endregion

        #region Update and Draw


        /// <summary>
        /// Updates the particle system.
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            if (gameTime == null)
                throw new ArgumentNullException("gameTime");

            currentTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

            RetireActiveParticles();
            FreeRetiredParticles();

            // If we let our timer go on increasing for ever, it would eventually
            // run out of floating point precision, at which point the particles
            // would render incorrectly. An easy way to prevent this is to notice
            // that the time value doesn't matter when no particles are being drawn,
            // so we can reset it back to zero any time the active queue is empty.

            if (firstActiveParticle == firstFreeParticle)
                currentTime = 0;

            if (firstRetiredParticle == firstActiveParticle)
                drawCounter = 0;
        }


        /// <summary>
        /// Helper for checking when active particles have reached the end of
        /// their life. It moves old particles from the active area of the queue
        /// to the retired section.
        /// </summary>
        void RetireActiveParticles()
        {
            float particleDuration = (float)settings.Duration.TotalSeconds;

            while (firstActiveParticle != firstNewParticle)
            {
                // Is this particle old enough to retire?
                // We multiply the active particle index by four, because each
                // particle consists of a quad that is made up of four vertices.
                float particleAge = currentTime - particles[firstActiveParticle * 4].Time;

                if (particleAge < particleDuration)
                    break;

                // Remember the time at which we retired this particle.
                particles[firstActiveParticle * 4].Time = drawCounter;

                // Move the particle from the active to the retired queue.
                firstActiveParticle++;

                if (firstActiveParticle >= settings.MaxParticles)
                    firstActiveParticle = 0;
            }
        }


        /// <summary>
        /// Helper for checking when retired particles have been kept around long
        /// enough that we can be sure the GPU is no longer using them. It moves
        /// old particles from the retired area of the queue to the free section.
        /// </summary>
        void FreeRetiredParticles()
        {
            while (firstRetiredParticle != firstActiveParticle)
            {
                // Has this particle been unused long enough that
                // the GPU is sure to be finished with it?
                // We multiply the retired particle index by four, because each
                // particle consists of a quad that is made up of four vertices.
                int age = drawCounter - (int)particles[firstRetiredParticle * 4].Time;

                // The GPU is never supposed to get more than 2 frames behind the CPU.
                // We add 1 to that, just to be safe in case of buggy drivers that
                // might bend the rules and let the GPU get further behind.
                if (age < 3)
                    break;

                // Move the particle from the retired to the free queue.
                firstRetiredParticle++;

                if (firstRetiredParticle >= settings.MaxParticles)
                    firstRetiredParticle = 0;
            }
        }

        private bool flap;
        
        /// <summary>
        /// Draws the particle system.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice device = GraphicsDevice;

            // Restore the vertex buffer contents if the graphics device was lost.
            if (vertexBuffer.IsContentLost)
            {
                vertexBuffer.SetData(particles);
            }

            // If there are any particles waiting in the newly added queue,
            // we'd better upload them to the GPU ready for drawing.
            if (firstNewParticle != firstFreeParticle)
            {
                AddNewParticlesToVertexBuffer();
            }

            // If there are any active particles, draw them now!
            if (firstActiveParticle != firstFreeParticle)
            {
                device.BlendState = settings.BlendState;
                device.DepthStencilState = DepthStencilState.DepthRead;

                if (particleEffect != null)
                {
                    DrawUsingParticleEffect(device);
                }
                else
                {
                    DrawUsingBasicEffect(device);
                }
                // Reset some of the renderstates that we changed,
                // so as not to mess up any other subsequent drawing.
                device.DepthStencilState = DepthStencilState.Default;
            }

            drawCounter++;
        }

        private void DrawUsingParticleEffect(GraphicsDevice device)
        {
            // Set an effect parameter describing the viewport size. This is
            // needed to convert particle sizes into screen space point sizes.
            effectViewportScaleParameter.SetValue(new Vector2(0.5f/device.Viewport.AspectRatio, -0.5f));

            // Set an effect parameter describing the current time. All the vertex
            // shader particle animation is keyed off this value.
            effectTimeParameter.SetValue(currentTime);

            device.SetVertexBuffer(vertexBuffer);
            device.Indices = indexBuffer;

            // Set the particle vertex and index buffer.
            device.SetVertexBuffer(vertexBuffer);
            device.Indices = indexBuffer;

            // Activate the particle effect.
            foreach (EffectPass pass in particleEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                if (firstActiveParticle < firstFreeParticle)
                {
                    // If the active particles are all in one consecutive range,
                    // we can draw them all in a single call.
                    device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0,
                        firstActiveParticle*4, (firstFreeParticle - firstActiveParticle)*4,
                        firstActiveParticle*6, (firstFreeParticle - firstActiveParticle)*2);
                }
                else
                {
                    // If the active particle range wraps past the end of the queue
                    // back to the start, we must split them over two draw calls.
                    device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0,
                        firstActiveParticle*4, (settings.MaxParticles - firstActiveParticle)*4,
                        firstActiveParticle*6, (settings.MaxParticles - firstActiveParticle)*2);

                    if (firstFreeParticle > 0)
                    {
                        device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0,
                            0, firstFreeParticle*4,
                            0, firstFreeParticle*2);
                    }
                }
            }
        }

        private void DrawUsingBasicEffect(GraphicsDevice device)
        {
            RasterizerState state = new RasterizerState();
            //state = GraphicsDevice.RasterizerState;
            state.CullMode = CullMode.None;
            GraphicsDevice.RasterizerState = state;

            DoManualCalculation();
            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                if (firstActiveParticle < firstFreeParticle)
                {
                    //// If the active particles are all in one consecutive range,
                    //// we can draw them all in a single call.
                    device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList,
                        basicVertexes, (firstActiveParticle/2)*4, (firstFreeParticle - firstActiveParticle)*4,
                        indices, (firstActiveParticle/2)*6, (firstFreeParticle - firstActiveParticle)*2);
                }
                else
                {
                    //// If the active particle range wraps past the end of the queue
                    //// back to the start, we must split them over two draw calls.
                    device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList,
                        basicVertexes, (firstActiveParticle/2)*4, (settings.MaxParticles - firstActiveParticle)*4,
                        indices, (firstActiveParticle/2)*6, (settings.MaxParticles - firstActiveParticle)*2);

                    if (firstFreeParticle > 0)
                    {
                        device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList,
                            basicVertexes, 0, firstFreeParticle*4,
                            indices, 0, firstFreeParticle*2);
                    }
                }
            }
        }

        private void DoManualCalculation()
        {
            if (firstActiveParticle < firstFreeParticle)
            {
                for (int i = firstActiveParticle; i < firstFreeParticle; i++)
                {
                    DoManualCalculation(i);
                }

            }
            else
            {
                for (int i = firstActiveParticle; i < settings.MaxParticles; i++)
                {
                    DoManualCalculation(i);
                }
                if (firstFreeParticle > 0)
                {
                    for (int i = 0; i < firstFreeParticle; i++)
                    {
                        DoManualCalculation(i);
                    }
                }
            }




        }

        private void DoManualCalculation(int index)
        {
            Vector3 pos = particles[index * 4].Position;
            float age = currentTime - particles[index*4].Time;
            float normalizedAge = Math.Min(age/(float) settings.Duration.TotalSeconds, 1);

            var random = particles[index * 4].Random.ToVector4();
            float particleSize = ComputeParticleSize(random.Z, normalizedAge);

            pos = ComputeParticlePosition(pos, particles[index*4].Velocity, age, normalizedAge);
            Color color = new Color(ComputeParticleColor(random.Z, normalizedAge));

            for (int i = 0; i < 4; i++)
            {
                Vector3 vector3Delta;
                switch (i)
                {
                    case 0: vector3Delta = new Vector3(-particleSize, -particleSize, 0); break;
                    case 1: vector3Delta = new Vector3(particleSize, -particleSize, 0); break;
                    case 2: vector3Delta = new Vector3(particleSize, particleSize, 0); break;
                    case 3: vector3Delta = new Vector3(-particleSize, particleSize, 0); break;
                    default:
                        return;
                }
                basicVertexes[index * 4 + i].Position = pos + vector3Delta;
                basicVertexes[index*4 + i].Color = color;
            }
        }
        // Vertex shader helper for computing the position of a particle.
        Vector3 ComputeParticlePosition(Vector3 position, Vector3 velocity,
            float age, float normalizedAge)
        {
            float startVelocity = velocity.Length();

            // Work out how fast the particle should be moving at the end of its life,
            // by applying a constant scaling factor to its starting velocity.
            float endVelocity = startVelocity * settings.EndVelocity;

            // Our particles have constant acceleration, so given a starting velocity
            // S and ending velocity E, at time T their velocity should be S + (E-S)*T.
            // The particle position is the sum of this velocity over the range 0 to T.
            // To compute the position directly, we must integrate the velocity
            // equation. Integrating S + (E-S)*T for T produces S*T + (E-S)*T*T/2.

            float velocityIntegral = startVelocity * normalizedAge +
                (endVelocity - startVelocity) * normalizedAge *
                normalizedAge / 2;

            position += Vector3.Normalize(velocity) * velocityIntegral * (float)settings.Duration.TotalSeconds;

            // Apply the gravitational force.
            position += settings.Gravity * age * normalizedAge;

            // Apply the camera view and projection transforms.
            return position;
        }

        // Vertex shader helper for computing the size of a particle.
        private float ComputeParticleSize(float randomValue, float normalizedAge)
        {
            // Apply a random factor to make each particle a slightly different size.
            float startSize = settings.MinStartSize + ((settings.MaxStartSize - settings.MinStartSize)*randomValue);
            float endSize = settings.MinStartSize + ((settings.MaxEndSize - settings.MinEndSize) * randomValue);

            // Compute the actual size based on the age of the particle.
            float size = startSize + ((endSize - startSize) * normalizedAge);

            // Project the size into screen coordinates.
            return size;
        }

        /// <summary>
        /// Helper for uploading new particles from our managed
        /// array to the GPU vertex buffer.
        /// </summary>
        void AddNewParticlesToVertexBuffer()
        {
            int stride = ParticleVertex.SizeInBytes;

            if (firstNewParticle < firstFreeParticle)
            {
                // If the new particles are all in one consecutive range,
                // we can upload them all in a single call.
                vertexBuffer.SetData(firstNewParticle * stride * 4, particles,
                                     firstNewParticle * 4,
                                     (firstFreeParticle - firstNewParticle) * 4,
                                     stride, SetDataOptions.NoOverwrite);
            }
            else
            {
                // If the new particle range wraps past the end of the queue
                // back to the start, we must split them over two upload calls.
                vertexBuffer.SetData(firstNewParticle * stride * 4, particles,
                                     firstNewParticle * 4,
                                     (settings.MaxParticles - firstNewParticle) * 4,
                                     stride, SetDataOptions.NoOverwrite);

                if (firstFreeParticle > 0)
                {
                    vertexBuffer.SetData(0, particles,
                                         0, firstFreeParticle * 4,
                                         stride, SetDataOptions.NoOverwrite);
                }
            }

            // Move the particles we just uploaded from the new to the active queue.
            firstNewParticle = firstFreeParticle;
        }

        Vector4 ComputeParticleColor(float randomValue, float normalizedAge)
        {
            // Apply a random factor to make each particle a slightly different color.
            Vector4 color = Vector4.Lerp(settings.MinColor.ToVector4(), settings.MaxColor.ToVector4(), randomValue);
            
            // Fade the alpha based on the age of the particle. This curve is hard coded
            // to make the particle fade in fairly quickly, then fade out more slowly:
            // plot x*(1-x)*(1-x) for x=0:1 in a graphing program if you want to see what
            // this looks like. The 6.7 scaling factor normalizes the curve so the alpha
            // will reach all the way up to fully solid.

            color.W *= normalizedAge * (1 - normalizedAge) * (1 - normalizedAge) * 6.7f;

            return color;
        }
        #endregion

        #region Public Methods


        /// <summary>
        /// Sets the camera view and projection matrices
        /// that will be used to draw this particle system.
        /// </summary>
        public void SetCamera(Matrix view, Matrix projection)
        {
            if (particleEffect != null)
            {
                effectViewParameter.SetValue(view);
                effectProjectionParameter.SetValue(projection);
            }
            else
            {
                basicEffect.View = view;
                basicEffect.Projection = projection;
            }
        }


        /// <summary>
        /// Adds a new particle to the system.
        /// </summary>
        public void AddParticle(Vector3 position, Vector3 velocity)
        {
            // Figure out where in the circular queue to allocate the new particle.
            int nextFreeParticle = firstFreeParticle + 1;

            if (nextFreeParticle >= settings.MaxParticles)
                nextFreeParticle = 0;

            // If there are no free particles, we just have to give up.
            if (nextFreeParticle == firstRetiredParticle)
                return;

            // Adjust the input velocity based on how much
            // this particle system wants to be affected by it.
            velocity *= settings.EmitterVelocitySensitivity;

            // Add in some random amount of horizontal velocity.
            float horizontalVelocity = MathHelper.Lerp(settings.MinHorizontalVelocity,
                                                       settings.MaxHorizontalVelocity,
                                                       (float)random.NextDouble());

            double horizontalAngle = random.NextDouble() * MathHelper.TwoPi;

            velocity.X += horizontalVelocity * (float)Math.Cos(horizontalAngle);
            velocity.Z += horizontalVelocity * (float)Math.Sin(horizontalAngle);

            // Add in some random amount of vertical velocity.
            velocity.Y += MathHelper.Lerp(settings.MinVerticalVelocity,
                                          settings.MaxVerticalVelocity,
                                          (float)random.NextDouble());

            // Choose four random control values. These will be used by the vertex
            // shader to give each particle a different size, rotation, and color.
            Color randomValues = new Color((byte)random.Next(255),
                                           (byte)random.Next(255),
                                           (byte)random.Next(255),
                                           (byte)random.Next(255));

            // Fill in the particle vertex structure.
            for (int i = 0; i < 4; i++)
            {
                particles[firstFreeParticle * 4 + i].Position = position;
                particles[firstFreeParticle * 4 + i].Velocity = velocity;
                particles[firstFreeParticle * 4 + i].Random = randomValues;
                particles[firstFreeParticle * 4 + i].Time = currentTime;
            }

            firstFreeParticle = nextFreeParticle;
        }


        #endregion
    }
}
