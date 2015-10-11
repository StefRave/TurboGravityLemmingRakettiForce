#region File Description
//-----------------------------------------------------------------------------
// ExplosionParticleSystem.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace TurboPort.ParticleSystems
{
    /// <summary>
    /// Custom particle system for creating the fiery part of the explosions.
    /// </summary>
    class ExplosionParticleSystem : ParticleSystem
    {
        public ExplosionParticleSystem(Game game, IContentLoader content)
            : base(game, content)
        { }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            const float relativeSize = 0.1f;

            settings.TextureName = "explosion";

            settings.MaxParticles = 100;

            settings.Duration = TimeSpan.FromSeconds(2);
            settings.DurationRandomness = 1;

            settings.MinHorizontalVelocity = 20 * relativeSize;
            settings.MaxHorizontalVelocity = 30 * relativeSize;

            settings.MinVerticalVelocity = -20 * relativeSize;
            settings.MaxVerticalVelocity = 20 * relativeSize;

            settings.EndVelocity = 0;

            settings.MinColor = Color.DarkGray;
            settings.MaxColor = Color.Gray;

            settings.MinRotateSpeed = -1;
            settings.MaxRotateSpeed = 1;

            settings.MinStartSize = 7;
            settings.MaxStartSize = 7;

            settings.MinEndSize = 70 * relativeSize;
            settings.MaxEndSize = 140 * relativeSize;

            // Use additive blending.
            settings.BlendState = BlendState.Additive;
        }
    }
}
