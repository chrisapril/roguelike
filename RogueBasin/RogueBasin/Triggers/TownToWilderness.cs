﻿using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin.Triggers
{
    /// <summary>
    /// When you enter the entrance square
    /// </summary>
    public class TownToWilderness : DungeonSquareTrigger
    {

        /// <summary>
        /// Not that Triggered is static so triggering one type of event triggers them all. This allows the same event to be put in multiple places and only triggered once
        /// </summary>
        public static bool Triggered { get; set; }

        public TownToWilderness()
        {
            Triggered = false;
        }

        public override bool CheckTrigger(int level, Point mapLocation)
        {
            //Check we are in the right place
            if (CheckLocation(level, mapLocation) == false)
            {
                return false;
            }
            //Otherwise in the right place

            //Play intro movie first time
            if (!Triggered)
            {
                //                Screen.Instance.PlayMovie("treasureRoom", true);
                Triggered = true;
            }

            //Update the player's stats for the adventure
            Game.Dungeon.SyncStatsWithTraining();

            //Teleport the player to the start location in the wilderness

            //Increment player level
            Player player = Game.Dungeon.Player;

            player.LocationLevel = 1;
            player.LocationMap = Game.Dungeon.Levels[player.LocationLevel].PCStartLocation;

            //Set vision
            player.SightRadius = 10;

            //This runs any triggers
            Game.Dungeon.MovePCAbsolute(player.LocationLevel, player.LocationMap.x, player.LocationMap.y);


            return true;
        }
    }
}