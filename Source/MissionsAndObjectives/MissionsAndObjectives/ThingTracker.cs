﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace MissionsAndObjectives
{
    public class ThingTracker : IExposable
    {
        private bool any = false;

        private ObjectiveType type = ObjectiveType.None;

        private List<ThingValue> targetsToCheck = new List<ThingValue>();

        public Dictionary<ThingDef, int> discoveredThings = new Dictionary<ThingDef, int>();

        public Dictionary<ThingDef, int> madeThings = new Dictionary<ThingDef, int>();

        public Dictionary<ThingDef, int> destroyedThings = new Dictionary<ThingDef, int>();

        public Dictionary<PawnKindDef, int> killedThings = new Dictionary<PawnKindDef, int>();

        public TargetInfo target;

        public ThingTracker()
        {
        }

        public ThingTracker(List<ThingValue> defs, ObjectiveType type, bool flag)
        {
            targetsToCheck = defs;
            this.type = type;
            any = flag;
            foreach (ThingValue tv in targetsToCheck)
            {
                ThingDef def = tv.ThingDef;
                if (def != null)
                {
                    if (type == ObjectiveType.Construct || type == ObjectiveType.Craft)
                    {
                        if (!madeThings.ContainsKey(def))
                        {
                            this.madeThings.Add(def, 0);
                        }
                    }
                    if (type == ObjectiveType.Discover)
                    {
                        if (!discoveredThings.ContainsKey(def))
                        {
                            this.discoveredThings.Add(def, 0);
                        }
                    }
                    if (type == ObjectiveType.Destroy)
                    {
                        if (!destroyedThings.ContainsKey(def))
                        {
                            this.destroyedThings.Add(def, 0);
                        }
                    }
                    if (type == ObjectiveType.Hunt)
                    {
                        if (!killedThings.ContainsKey(tv.PawnKindDef))
                        {
                            this.killedThings.Add(tv.PawnKindDef, 0);
                        }                     
                    }
                }
            }
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref any, "any");
            Scribe_Collections.Look(ref this.madeThings, "madeThings");
            Scribe_Collections.Look(ref this.discoveredThings, "discoveredThings");
            Scribe_Collections.Look(ref this.destroyedThings, "destroyedThings");
            Scribe_Collections.Look(ref this.killedThings, "killedThings");
        }

        public void Reset()
        {
            foreach (ThingValue tv in targetsToCheck)
            {
                ThingDef def = tv.ThingDef;
                if (type == ObjectiveType.Construct || type == ObjectiveType.Craft)
                {
                    madeThings[def] = 0;
                }
                if (type == ObjectiveType.Discover)
                {
                    discoveredThings[def] = 0;
                }
                if (type == ObjectiveType.Destroy)
                {
                    destroyedThings[def] = 0; 
                }
                if (type == ObjectiveType.Hunt)
                {
                    killedThings[tv.PawnKindDef] = 0;
                }
            }
        }

        //General

        public int GetTargetCount
        {
            get
            {
                return targetsToCheck.Sum(tv => tv.value);
            }
        }

        //Discovered

        public int GetCountDiscovered
        {
            get
            {
                return discoveredThings.Values.Sum();
            }
        }

        public bool AllDiscovered
        {
            get
            {
                if (any)
                {
                    return targetsToCheck.Any(tv => discoveredThings[tv.ThingDef] >= tv.value);
                }
                return GetCountDiscovered >= targetsToCheck.Sum(tv => tv.value);
            }
        }

        // Killed-Desroyed Sum

        public bool AllDestroyedKilled
        {
            get
            {
                return AllPawnsKilled && AllThingsDestroyed;
            }
        }

        public int GetSumKilledDestroyed
        {
            get
            {
                return GetCountKilledPawns + GetCountDestroyedThings;
            }
        }

        // Killed

        public bool AllPawnsKilled
        {
            get
            {
                if (killedThings.Count > 0)
                {
                    if (any)
                    {
                        return targetsToCheck.Any(tv => killedThings.ContainsKey(tv.PawnKindDef) && killedThings[tv.PawnKindDef] >= tv.value);
                    }
                    return GetCountKilledPawns >= targetsToCheck.Sum(tv => tv.value);
                }
                return true;
            }
        }

        public int GetCountKilledPawns
        {
            get
            {
                if (!killedThings.ToList().NullOrEmpty())
                {
                    return killedThings.Values.Sum();
                }
                return 0;
            }
        }

        // Destroyed

        public bool AllThingsDestroyed
        {
            get
            {
                if (destroyedThings.Count > 0)
                {
                    if (any)
                    {
                        return targetsToCheck.Any(tv => destroyedThings.ContainsKey(tv.ThingDef) && destroyedThings[tv.ThingDef] >= tv.value);
                    }
                    return GetCountDestroyedThings >= targetsToCheck.Sum(tv => tv.value);
                }
                return true;
            }
        }

        public int GetCountDestroyedThings
        {
            get
            {
                if (!destroyedThings.ToList().NullOrEmpty())
                {
                    return destroyedThings.Values.Sum();
                }
                return 0;
            }
        }

        // Made

        public bool AllMade
        {
            get
            {
                if (any)
                {
                    return targetsToCheck.Any(tv => madeThings[tv.ThingDef] >= tv.value);
                }
                return GetCountMade >= targetsToCheck.Sum(tv => tv.value);
            }
        }

        public int GetCountMade
        {
            get
            {
                return madeThings.Keys.Sum(k => madeThings[k]);
            }
        }

        public int GetCountMadeFor(ThingDef def)
        {
            return madeThings[def];
        }

        // Voids

        public void Kill(PawnKindDef def, IntVec3 cell, Map map)
        {
            if (killedThings.ContainsKey(def))
            {
                killedThings[def] += 1;
                target = new TargetInfo(cell, map, true);
            }
        }

        public void Destroy(ThingDef def, IntVec3 cell, Map map)
        {
            if (destroyedThings.ContainsKey(def))
            {
                destroyedThings[def] += 1;
                target = new TargetInfo(cell, map, true);
            }
        }

        public void Discover(Thing thing)
        {
            if (discoveredThings.ContainsKey(thing.def))
            {
                discoveredThings[thing.def] += 1;
                target = thing;
            }
        }

        public void Make(ThingDef def, IntVec3 cell, Map map)
        {
            if (madeThings.ContainsKey(def))
            {
                madeThings[def] += 1;
                target = new TargetInfo(cell, map, true);
            }
        }
    }
}
