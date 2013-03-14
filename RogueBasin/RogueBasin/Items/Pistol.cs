﻿using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;

namespace RogueBasin.Items
{
    public class Pistol : Item, IEquippableItem
    {

        /// <summary>
        /// Public for serialization
        /// </summary>
        public int Ammo { get; set; }

        public Pistol()
        {
            Ammo = MaxAmmo();
        }

        /// <summary>
        /// Equipment slots where we can be equipped
        /// </summary>
        public List<EquipmentSlot> EquipmentSlots
        {
            get
            {
                List<EquipmentSlot> retList = new List<EquipmentSlot>();
                retList.Add(EquipmentSlot.Weapon);

                return retList;
            }
        }

        public bool Equip(Creature user)
        {
            LogFile.Log.LogEntryDebug("Pistol equipped", LogDebugLevel.Medium);

            //Give player story. Mention level up if one will occur.

            if (Game.Dungeon.Player.PlayItemMovies)
            {
                //Screen.Instance.PlayMovie("plotbadge", true);
                //Screen.Instance.PlayMovie("multiattack", false);
            }

            //Messages
            //Game.MessageQueue.AddMessage("A fine short sword - good for slicing and dicing.");

            //Screen.Instance.PlayMovie("plotbadge", true);

            //Level up?
            //Game.Dungeon.Player.LevelUp();

            //Add move?
            //Game.Dungeon.LearnMove(new SpecialMoves.MultiAttack());
            //Screen.Instance.PlayMovie("multiattack", false);

            //Add any equipped (actually permanent) effects
            //Game.Dungeon.Player.Speed += 10;

            return true;
        }

        /// <summary>
        /// not used in this game
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public bool UnEquip(Creature user)
        {
            LogFile.Log.LogEntryDebug("Pistol unequipped", LogDebugLevel.Low);
            return true;
        }
        /// <summary>
        /// not used in this game
        /// </summary>
        public override int GetWeight()
        {
            return 50;
        }

        public override string SingleItemDescription
        {
            get { return "pistol"; }
        }

        /// <summary>
        /// not used in this game
        /// </summary>
        public override string GroupItemDescription
        {
            get { return "pistols"; }
        }

        protected override char GetRepresentation()
        {
            return '{';
        }

        public override libtcodWrapper.Color GetColour()
        {
            return ColorPresets.Gainsboro;
        }

        public int ArmourClassModifier()
        {
            return 0;
        }

        public int DamageBase()
        {
            //1d6
            return 0;
        }

        public int DamageModifier()
        {
            return 0;
        }

        public int HitModifier()
        {
            return 0;
        }

        public bool HasMeleeAction()
        {
            return false;
        }


        public bool HasFireAction()
        {
            return true;
        }

        /// <summary>
        /// Can be thrown
        /// </summary>
        /// <returns></returns>
        public bool HasThrowAction()
        {

            return true;
        }

        /// <summary>
        /// Can be operated
        /// </summary>
        /// <returns></returns>
        public bool HasOperateAction()
        {
            return false;
        }

        public int MaxAmmo()
        {
            return 3;
        }

        public int RemainingAmmo()
        {
            return Ammo;
        }

        /// <summary>
        /// Fires the item - probably should be a method
        /// </summary>
        /// <param name="target"></param>
        /// <param name="enemyTarget"></param>
        /// <returns></returns>
        public bool FireItem(Point target)
        {
            //Should be guaranteed in range by caller

            Player player = Game.Dungeon.Player;
            Dungeon dungeon = Game.Dungeon;

            LogFile.Log.LogEntryDebug("Firing pistol", LogDebugLevel.Medium);

            //Get the points along the line of where we are firing
            CreatureFOV currentFOV = Game.Dungeon.CalculateCreatureFOV(player);
            List<Point> targetSquares = currentFOV.GetPathLinePointsInFOV(player.LocationMap, target);

            //Remove 1 ammo
            Ammo--;

            //Hit the first monster only
            Monster monster = null;
            foreach (Point p in targetSquares)
            {
                //Check there is a monster at target
                SquareContents squareContents = dungeon.MapSquareContents(player.LocationLevel, target);

                //Hit the monster if it's there
                if (squareContents.monster != null)
                {
                    monster = squareContents.monster;
                    break;
                }
            }

            //Make firing sound
            Game.Dungeon.AddSoundEffect(FireSoundMagnitude(), player.LocationLevel, player.LocationMap);

            if(monster == null) {
                LogFile.Log.LogEntryDebug("No monster in target for Pistol.Ammo used anyway.", LogDebugLevel.Medium);
                return true;
            }

            //Draw attack

            Screen.Instance.DrawShotgunMissileAttack(targetSquares);

            //Damage monster
            
            int damageBase = 2;

            string combatResultsMsg = "PvM (" + monster.Representation + ")Pistol: Dam: 2";
            LogFile.Log.LogEntryDebug(combatResultsMsg, LogDebugLevel.Medium);

            //Apply damage
            player.ApplyDamageToMonster(monster, damageBase, false, false);

            return true;
          
        }


        /// <summary>
        /// Throws the item - check if we can't pull this out
        /// </summary>
        /// <param name="target"></param>
        /// <param name="enemyTarget"></param>
        /// <returns></returns>
        public bool ThrowItem(Point target)
        {
            LogFile.Log.LogEntryDebug("Throwing pistol", LogDebugLevel.Medium);

            Player player = Game.Dungeon.Player;

            //Make throwing sound
            Game.Dungeon.AddSoundEffect(ThrowSoundMagnitude(), player.LocationLevel, player.LocationMap);

            //Stun enemy for 1 round (to do)

            return true;
        }

        /// <summary>
        /// Operates the item - definitely a method
        /// </summary>
        /// <param name="target"></param>
        /// <param name="enemyTarget"></param>
        /// <returns></returns>
        public bool OperateItem()
        {
            return false;
        }

        /// <summary>
        /// What type of targetting reticle is needed? [for throwing]
        /// </summary>
        /// <returns></returns>
        public virtual TargettingType TargetTypeThrow()
        {
            return TargettingType.Line;
        }

        /// <summary>
        /// What type of targetting reticle is needed? [for firing]
        /// </summary>
        /// <returns></returns>
        public virtual TargettingType TargetTypeFire()
        {
            return TargettingType.Line;
        }

        /// <summary>
        /// Throwing range
        /// </summary>
        /// <returns></returns>
        public int RangeThrow()
        {
            return 8;
        }

        /// <summary>
        /// Firing range
        /// </summary>
        /// <returns></returns>
        public int RangeFire()
        {
            return 8;
        }

        public double FireSoundMagnitude()
        {
            return 0.66;
        }

        /// <summary>
        /// Noise mag of this weapon on throwing
        /// </summary>
        /// <returns></returns>
        public double ThrowSoundMagnitude() {
            return 0.2;   
        }

    }
}