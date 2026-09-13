using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace StoragebornXenotype
{
    public class StoragebornGene : Gene
    {
        public override void PostAdd()
        {
            base.PostAdd();
            StoragebornController.ApplyTo(pawn);
            StoragebornController.RandomizeBodyGene(pawn);
        }
    }

    public class StoragebornSizeGene : Gene
    {
        public override bool Active
        {
            get
            {
                return base.Active && StoragebornController.HasStoragebornGene(pawn);
            }
        }
    }

}
