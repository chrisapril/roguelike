﻿using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Triggers
{
    /// <summary>
    /// Magic library
    /// </summary>
    public class BackToSchool : DungeonSquareTrigger
    {
        /// <summary>
        /// Not that Triggered is static so triggering one type of event triggers them all. This allows the same event to be put in multiple places and only triggered once
        /// </summary>
        public static bool Triggered { get; set; }

        public BackToSchool()
        {
            Triggered = false;
        }

        public override bool CheckTrigger(int level, Point mapLocation)
        {
            //Check we are in the right place - should be in the base I think
            if (CheckLocation(level, mapLocation) == false)
            {
                return false;
            }

            //The player shouldn't want to go back, but let them if they must

            //Check they really do want to leave
            bool decision = Screen.Instance.YesNoQuestion("Really go back to school?");

            if (decision == false)
            {
                LogFile.Log.LogEntryDebug("Player chose not to leave wilderness", LogDebugLevel.Low);
                return false;
            }

            //Go back to town
            LogFile.Log.LogEntry("Player leaving wilderness");
            LogFile.Log.LogEntryDebug("Player leaving wilderness", LogDebugLevel.Medium);

            Game.MessageQueue.AddMessage("You make it safely back to school without being discovered!");

            Game.Dungeon.MoveToNextDate();
            Game.Dungeon.PlayerBackToTown();

            return true;
        }
    }
}
