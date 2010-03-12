﻿using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Spells
{
    /// <summary>
    /// Gives a temporary boost to armour
    /// </summary>
    public class MageArmour : Spell
    {
        public override bool DoSpell(Point target)
        {
            Player player = Game.Dungeon.Player;
            Dungeon dungeon = Game.Dungeon;

            //Do we already have the effect?
            if (player.IsEffectActive(new PlayerEffects.DamageUp(player, 0, 0)))
            {
                Game.MessageQueue.AddMessage("Spell already in effect.");
                LogFile.Log.LogEntryDebug("Magic armour already in effect", LogDebugLevel.Medium);
                return false;
            }

            //Add an armour up effect

            //Apply the armour effect to the player
            //Duration note 100 is normally 1 turn for a non-sped up player
            int duration = 4000 + Game.Random.Next(3000);
            int toArmour = (int)Math.Floor(player.MagicStat / 20.0) + 1;

            //Add a message
            Game.MessageQueue.AddMessage("You cast Mage Armour.");
            LogFile.Log.LogEntryDebug("Mage Armour Cast. Duration: " + duration + " Armour: +" + toArmour, LogDebugLevel.Medium);

            player.AddEffect(new PlayerEffects.ArmourUp(player, duration, toArmour));

            return true;
        }

        public override int MPCost()
        {
            return 5;
        }

        public override bool NeedsTarget()
        {
            return false;
        }

        public override string SpellName()
        {
            return "Mage Armour";
        }

        public override string Abbreviation()
        {
            return "MA";
        }

        internal override int GetRequiredMagic()
        {
            return 45;
        }
    }
}