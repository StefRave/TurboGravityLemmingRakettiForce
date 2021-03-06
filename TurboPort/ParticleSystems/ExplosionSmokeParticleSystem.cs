#region File Description
//-----------------------------------------------------------------------------
// ExplosionSmokeParticleSystem.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements

using System;
using Microsoft.Xna.Framework;

#endregion

namespace TurboPort.ParticleSystems
{
    /// <summary>
    /// Custom particle system for creating the smokey part of the explosions.
    /// </summary>
    class ExplosionSmokeParticleSystem : ParticleSystem
    {
        public ExplosionSmokeParticleSystem(Game game, IContentLoader content)
            : base(game, content)
        { }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            const float relativeSize = 0.1f;

            settings.TextureName = "smoke";

            settings.MaxParticles = 100;

            settings.Duration = TimeSpan.FromSeconds(4);

            settings.MinHorizontalVelocity = 0 * relativeSize;
            settings.MaxHorizontalVelocity = 50 * relativeSize;

            settings.MinVerticalVelocity = -10 * relativeSize;
            settings.MaxVerticalVelocity = 50 * relativeSize;

            settings.EndVelocity = 0;

            settings.MinColor = Color.LightGray;
            settings.MaxColor = Color.White;

            settings.MinRotateSpeed = -2;
            settings.MaxRotateSpeed = 2;

            settings.MinStartSize = 7;
            settings.MaxStartSize = 7;

            settings.MinEndSize = 70 * relativeSize;
            settings.MaxEndSize = 140 * relativeSize;
        }
    }
}
