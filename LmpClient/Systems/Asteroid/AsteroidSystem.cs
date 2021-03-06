﻿using LmpClient.Base;
using LmpClient.Events;
using LmpClient.Systems.Lock;
using System.Collections.Generic;
using System.Linq;

namespace LmpClient.Systems.Asteroid
{
    public class AsteroidSystem : System<AsteroidSystem>
    {
        #region Fields

        public AsteroidEvents AsteroidEvents { get; } = new AsteroidEvents();
        public bool AsteroidSystemReady => Enabled && HighLogic.LoadedScene >= GameScenes.FLIGHT;

        #endregion

        #region Base overrides

        public override string SystemName { get; } = nameof(AsteroidSystem);

        protected override void OnEnabled()
        {
            base.OnEnabled();

            LockEvent.onLockReleaseUnityThread.Add(AsteroidEvents.LockReleased);
            GameEvents.onLevelWasLoadedGUIReady.Add(AsteroidEvents.LevelLoaded);
            TrackingEvent.onStartTrackingAsteroid.Add(AsteroidEvents.StartTrackingAsteroid);
            TrackingEvent.onStopTrackingAsteroid.Add(AsteroidEvents.StopTrackingAsteroid);
            GameEvents.onNewVesselCreated.Add(AsteroidEvents.NewVesselCreated);
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();

            LockEvent.onLockReleaseUnityThread.Remove(AsteroidEvents.LockReleased);
            GameEvents.onLevelWasLoadedGUIReady.Remove(AsteroidEvents.LevelLoaded);
            TrackingEvent.onStartTrackingAsteroid.Remove(AsteroidEvents.StartTrackingAsteroid);
            TrackingEvent.onStopTrackingAsteroid.Remove(AsteroidEvents.StopTrackingAsteroid);
            GameEvents.onNewVesselCreated.Remove(AsteroidEvents.NewVesselCreated);
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Try to acquire the asteroid-spawning lock if nobody else has it.
        /// </summary>
        public void TryGetAsteroidLock()
        {
            if (!LockSystem.LockQuery.AsteroidLockExists())
                LockSystem.Singleton.AcquireAsteroidLock();
        }

        public bool VesselIsAsteroid(Vessel vessel)
        {
            if (vessel != null && !vessel.loaded)
                return ProtoVesselIsAsteroid(vessel.protoVessel);

            //Check the vessel has exactly one part.
            return vessel && vessel.parts != null && vessel.parts.Count == 1 && vessel.parts[0].partName == "PotatoRoid";
        }

        private static bool ProtoVesselIsAsteroid(ProtoVessel protoVessel)
        {
            if (protoVessel == null) return false;

            if ((protoVessel.protoPartSnapshots == null || protoVessel.protoPartSnapshots.Count == 0) && protoVessel.vesselName.StartsWith("Ast."))
                return true;

            return protoVessel.protoPartSnapshots?.FirstOrDefault()?.partName == "PotatoRoid";
        }

        public int GetAsteroidCount()
        {
            var seenAsteroids = GetCurrentAsteroids().Count();
            return seenAsteroids;
        }

        public IEnumerable<Vessel> GetCurrentAsteroids()
        {
            return FlightGlobals.Vessels.Where(VesselIsAsteroid);
        }

        #endregion
    }
}
