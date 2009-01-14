﻿using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;
using Console = System.Console;

namespace RogueBasin {

    //Represents our screen
    public class Screen
    {
        static Screen instance = null;

        //Console/screen size
        int width;
        int height;

        //Top left coord to start drawing the map at
        Point mapTopLeft;

        /// <summary>
        /// Dimensions of message display area
        /// </summary>
        Point msgDisplayTopLeft;
        int msgDisplayNumLines;

        Dictionary<MapTerrain, char> terrainChars;
        char PCChar;

        //Keep enough state so that we can draw each screen
        string lastMessage = "";

        public static Screen Instance
        {
            get
            {
                if (instance == null)
                    instance = new Screen();
                return instance;
            }
        }


        Screen()
        {
            width = 200;
            height = 80;

            mapTopLeft = new Point(5, 5);

            msgDisplayTopLeft = new Point(0, 1);
            msgDisplayNumLines = 1;

            terrainChars = new Dictionary<MapTerrain, char>();
            terrainChars.Add(MapTerrain.Empty, '.');
            terrainChars.Add(MapTerrain.Wall, '#');
            terrainChars.Add(MapTerrain.Corridor, '|');

            PCChar = '@';
        }

        //Setup the screen
        public void InitialSetup()
        {
            //Note that 

            //CustomFontRequest fontReq = new CustomFontRequest("terminal.png", 8, 8, CustomFontRequestFontTypes.Grayscale);
            RootConsole.Width = width;
            RootConsole.Height = height;
            RootConsole.WindowTitle = "RogueBase";
            RootConsole.Fullscreen = false;
            //RootConsole.Font = fontReq;
            /*
            RootConsole rootConsole = RootConsole.GetInstance();

            rootConsole.PrintLine("Hello world!", 30, 30, LineAlignment.Left);
            rootConsole.Flush();
            */
            Console.WriteLine("debug test message.");

        }

        //Draw the current dungeon map and objects
        public void Draw()
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            //Clear screen
            rootConsole.Clear();

            DrawMap(Game.Dungeon.PCMap);

            //Draw creatures

            DrawCreatures(Game.Dungeon.Monsters);

            //Draw PC

            Point PClocation = Game.Dungeon.Player.LocationMap;

            rootConsole.PutChar(mapTopLeft.x + PClocation.x, mapTopLeft.y + PClocation.y, PCChar);

            //Flush the console
            rootConsole.Flush();
            
        }

        private void DrawCreatures(List<Monster> creatureList)
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            foreach (Creature creature in creatureList)
            {
                //Not on this level
                if (creature.LocationLevel != Game.Dungeon.Player.LocationLevel)
                    continue;

                rootConsole.PutChar(mapTopLeft.x + creature.LocationMap.x, mapTopLeft.y + creature.LocationMap.y, creature.Representation);
            }
        }

        //Draw a map only (useful for debugging)
        public void DrawMapDebug(Map map)
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            //Clear screen
            rootConsole.Clear();

            DrawMap(map);
            
            //Flush the console
            rootConsole.Flush();
        }

        private void DrawMap(Map map)
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            for (int i = 0; i < map.width; i++)
            {
                for (int j = 0; j < map.height; j++)
                {
                    int screenX = mapTopLeft.x + i;
                    int screenY = mapTopLeft.y + j;

                    rootConsole.PutChar(screenX, screenY, terrainChars[map.mapSquares[i, j].Terrain]);

                }
            }
        }
        internal void ConsoleLine(string datedEntry)
        {
            Console.WriteLine(datedEntry);
        }

        internal void ClearMessageLine()
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            lastMessage = null;

            ClearMessageBar();

            rootConsole.Flush();
        }

        internal void PrintMessage(string message)
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            //Update state
            lastMessage = message;

            //Clear message bar
            ClearMessageBar();

            //Display new message
            rootConsole.PrintLineRect(message, msgDisplayTopLeft.x, msgDisplayTopLeft.y, width - msgDisplayTopLeft.x, msgDisplayNumLines, LineAlignment.Left);

            rootConsole.Flush();
        }

        void ClearMessageBar()
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            rootConsole.DrawRect(msgDisplayTopLeft.x, msgDisplayTopLeft.y, width - msgDisplayTopLeft.x, msgDisplayNumLines, true);
        }

    }
}