﻿using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Items
{
    public class PotionMPRestore : Item, IUseableItem
    {
        bool usedUp;

        public PotionMPRestore()
        {
            usedUp = false;
        }

        public bool Use(Creature user)
        {
            //Currently healing is implemented as a player effect so we need to check the user is a player
            Player player = user as Player;

            //Not a player
            if (player == null)
            {
                return false;
            }
            
            Game.MessageQueue.AddMessage("You eat the berry.");

            //Apply the healing effect to the player
            int healing = 10 + Game.Random.Next(10);
            player.AddEffect(new PlayerEffects.MPRestore(player, healing));

            //Add a message
            

            //This uses up the potion
            usedUp = true;

            return true;
        }

        public override int GetWeight()
        {
            return 10;
        }

        public override string SingleItemDescription
        {
            get { return "gold berry"; }
        }

        public override string GroupItemDescription
        {
            get { return "gold berries"; }
        }

        public override libtcodWrapper.Color GetColour()
        {
            return ColorPresets.Gold;
        }

        public bool UsedUp
        {
            set { usedUp = value; }
            get { return usedUp; }
        }

        protected override char GetRepresentation()
        {
            return (char)236;
        }

        public override bool UseHiddenName { get { return false; } }

        public override string HiddenSuffix
        {
            get
            {
                return "vial";
            }
        }
    }
}
