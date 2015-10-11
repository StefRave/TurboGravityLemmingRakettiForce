#region File Description
//-----------------------------------------------------------------------------
// ProjectileTrailParticleSystem.cs
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
    /// Custom particle system for leaving smoke trails behind the rocket projectiles.
    /// </summary>
    class ProjectileTrailParticleSystem : ParticleSystem
    {
        public ProjectileTrailParticleSystem(Game game, IContentLoader content)
            : base(game, content)
        { }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            float relativeSize = 0.5f;

            settings.TextureName = "smokewhite";

            settings.MaxParticles = 1000;

            settings.Duration = TimeSpan.FromSeconds(3);

            settings.DurationRandomness = 1.5f;

            settings.EmitterVelocitySensitivity = 0.1f;

            settings.MinHorizontalVelocity = 0 * relativeSize;
            settings.MaxHorizontalVelocity = 1 * relativeSize;

            settings.MinVerticalVelocity = -1 * relativeSize;
            settings.MaxVerticalVelocity = 1 * relativeSize;

            settings.MinColor = new Color(64, 96, 128, 255);
            settings.MaxColor = new Color(255, 255, 255, 128);

            settings.MinRotateSpeed = -4;
            settings.MaxRotateSpeed = 4;

            settings.MinStartSize = 1 * relativeSize;
            settings.MaxStartSize = 3 * relativeSize;

            settings.MinEndSize = 4 * relativeSize;
            settings.MaxEndSize = 11 * relativeSize;
        }
    }
}
