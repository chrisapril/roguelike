﻿using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.Items
{
    public class PotionMajSpeedUp : Item, IUseableItem
    {
        bool usedUp;

        public PotionMajSpeedUp()
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

            //Add a message
            Game.MessageQueue.AddMessage("You drink the potion.");

            //Apply the speed up effect to the player
            //Duration note 100 is normally 1 turn for a non-sped up player

            int duration = 30 * Creature.turnTicks + Game.Random.Next(50 * Creature.turnTicks);
            int speedUpAmount = 75 + Game.Random.Next(50);

            player.AddEffect(new PlayerEffects.SpeedUp(duration, speedUpAmount));



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
            get { return "p5"; }
        }

        public override string GroupItemDescription
        {
            get { return "potions"; }
        }

        public bool UsedUp
        {
            set { usedUp = value; }
            get { return usedUp; }
        }

        protected override char GetRepresentation()
        {
            return '!';
        }

        public override bool UseHiddenName { get { return true; } }

        public override string HiddenSuffix
        {
            get
            {
                return "vial";
            }
        }
    }
}
