﻿using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace PriorityClean
{
    public static class PriorityCleanDefOf
    {
        public static TerrainDef SterileTile = TerrainDef.Named("SterileTile");
    }

    public class WorkGiver_PriorityClean : WorkGiver_Scanner
    {
        public override PathEndMode PathEndMode
        {
            get => PathEndMode.Touch;
        }

        public override ThingRequest PotentialWorkThingRequest
        {
            get => ThingRequest.ForGroup(ThingRequestGroup.Filth);
        }

        public override int LocalRegionsToScanFirst
        {
            get => 4;
        }

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            List<Thing> list = pawn.Map.listerFilthInHomeArea.FilthInHomeArea;
            list.RemoveAll(thing => !thing.Map.terrainGrid.TerrainAt(thing.InteractionCell).defName.Equals("SterileTile"));
            return list;
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return pawn.Faction == Faction.OfPlayer &&
                t != null &&
                t is Filth && 
                t.Map.terrainGrid.TerrainAt(t.InteractionCell).defName.Equals("SterileTile") && 
                t.Map.areaManager.Home[t.Position] && 
                pawn.CanReserve(t, 1, -1, null, forced);
         }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Job job = new Job(JobDefOf.Clean);
            job.AddQueuedTarget(TargetIndex.A, t);
            int num = 25;
            Map map = t.Map;
            Room room = t.GetRoom(RegionType.Set_Passable);
            for (int i = 0; i < 100; i++)
            {
                IntVec3 intVec = t.Position + GenRadial.RadialPattern[i];
                if (intVec.InBounds(map) && intVec.GetRoom(map, RegionType.Set_Passable) == room)
                {
                    List<Thing> thingList = intVec.GetThingList(map);
                    for (int j = 0; j < thingList.Count; j++)
                    {
                        Thing thing = thingList[j];
                        if (HasJobOnThing(pawn, thing, forced) && thing != t)
                        {
                            job.AddQueuedTarget(TargetIndex.A, thing);
                        }
                    }
                    if (job.GetTargetQueue(TargetIndex.A).Count >= num)
                        break;
                } else {
                    break;
                }
            }
            if (job.targetQueueA != null && job.targetQueueA.Count >= 5)
            {
                job.targetQueueA.SortBy((LocalTargetInfo targ) => targ.Cell.DistanceToSquared(pawn.Position));
            }
            return job;
        }
    }
}
