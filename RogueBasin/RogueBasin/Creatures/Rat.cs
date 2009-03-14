﻿using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.Creatures
{
    public class Rat : MonsterSimpleAI
    {
        const int classMaxHitpoints = 10;

        public Rat()
        {
            //Add a default right hand slot
            EquipmentSlots.Add(new EquipmentSlotInfo(EquipmentSlot.RightHand));
        }

        public override void InventoryDrop()
        {
            //Nothing to drop

            //Hmm, could use this corpses
        }

        protected override int ClassMaxHitpoints()
        {
            return classMaxHitpoints;
        }

        /// <summary>
        /// Creature AC. Set by type of creature.
        /// </summary>
        public override int ArmourClass()
        {
            return 10;
        }

        /// <summary>
        /// Creature 1dn damage.  Set by type of creature.
        /// </summary>
        public override int DamageBase()
        {
            return 2;
        }

        /// <summary>
        /// Creature damage modifier.  Set by type of creature.
        /// </summary>
        public override int DamageModifier()
        {
            return 0;
        }

        public override int HitModifier()
        {
            return 0;
        }

        /// <summary>
        /// Rat
        /// </summary>
        /// <returns></returns>
        public override string SingleDescription { get { return "rat"; } }

        /// <summary>
        /// Rats
        /// </summary>
        public override string GroupDescription  { get { return "rats"; } }

        protected override char GetRepresentation()
        {
            return 'r';
        }
    }
}
