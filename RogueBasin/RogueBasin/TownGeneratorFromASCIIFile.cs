﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace RogueBasin
{
    /// <summary>
    /// Loads a txt file made in ascii paint or similar
    /// </summary>
    class TownGeneratorFromASCIIFile
    {
        bool fileLoaded = false;

        string mapFilename;

        Map baseMap;

        /// <summary>
        /// How we map terrain in the file to the game
        /// </summary>
        Dictionary<char, MapTerrain> terrainMapping;

        /// <summary>
        /// How we map chars in the file to features.
        /// Just a list and a switch case for now. Might be nicer as an enum
        /// </summary>
        List<char> featureChars;

        /// <summary>
        /// Special characters in the file
        /// </summary>
        List<char> specialChars;

        List<string> storedMapRows;

        int width;
        int height;

        public TownGeneratorFromASCIIFile()
        {
            terrainMapping = new Dictionary<char, MapTerrain>();
            featureChars = new List<char>();
            specialChars = new List<char>();

            storedMapRows = new List<string>();

            SetupTerrainMapping();
            SetupFeatureMapping();
            SetupSpecialChars();
        }

        private void SetupSpecialChars()
        {
            specialChars.Add('x'); //PC start location
            specialChars.Add('1'); //battle trigger
            specialChars.Add('2'); //spot girl trigger
            specialChars.Add('3'); //help girl trigger
            specialChars.Add('4'); //treasure room trigger
            specialChars.Add('5'); 
            specialChars.Add('6'); 
            specialChars.Add('7'); 
            specialChars.Add('8');
            specialChars.Add('9');
            specialChars.Add('0');

           
        }

        private void SetupFeatureMapping()
        {
            featureChars.Add('>');
            featureChars.Add('<');
        }

        private void SetupTerrainMapping()
        {
            terrainMapping.Add(' ', MapTerrain.Void);
            terrainMapping.Add('.', MapTerrain.Empty);
            terrainMapping.Add(',', MapTerrain.Grass);
            terrainMapping.Add('=', MapTerrain.River);
            terrainMapping.Add('^', MapTerrain.Mountains);
            terrainMapping.Add('*', MapTerrain.Trees);
            terrainMapping.Add('-', MapTerrain.Road);
            terrainMapping.Add('#', MapTerrain.Wall);
            terrainMapping.Add('+', MapTerrain.ClosedDoor);
        }

        /// <summary>
        /// Loads ASCII file. Will throw exception if it can't find it
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public void LoadASCIIFile(string filenameRoot) {

            try
            {
                Assembly _assembly = Assembly.GetExecutingAssembly();

                //MessageBox.Show("Showing all embedded resource names");

                //string[] names = _assembly.GetManifestResourceNames();
                //foreach (string name in names)
                //    MessageBox.Show(name);


                string filename = "RogueBasin.bin.Debug." + filenameRoot;
                Stream _fileStream = _assembly.GetManifestResourceStream(filename);


                mapFilename = filename;
                LogFile.Log.LogEntry("Loading dungeon level from file: " + filename);

                StreamReader reader = new StreamReader(_fileStream);
                string thisLine;

                List<string> mapRows = new List<string>();
                
                while ((thisLine = reader.ReadLine()) != null)
                {
                    mapRows.Add(thisLine);
                }

                //Calculate dimensions
                width = 0;

                foreach (string mapRow in mapRows)
                {
                    if (mapRow.Length > width)
                        width = mapRow.Length;
                }

                if(width == 0) {
                    LogFile.Log.LogEntry("No data in map file: " + filename);
                    throw new ApplicationException("No data in file - width is 0:");
                }

                //Check all rows are this long, if not add padding
                storedMapRows.Clear();

                foreach (string mapRow in mapRows)
                {
                    int strLength = mapRow.Length;
                    //If the string requires padding
                    if (strLength < width)
                    {
                        StringBuilder appendingString = new StringBuilder(mapRow);
                        for (int i = 0; i < width - strLength; i++)
                        {
                            appendingString.Append(" ");
                        }
                        storedMapRows.Add(appendingString.ToString());
                    }
                    else
                    {
                        storedMapRows.Add(mapRow);
                    }
                } 
              
                //Scan the loaded rows to check that they don't contain any unknown terrain
               
                foreach (string mapRow in storedMapRows)
                {
                    foreach (char c in mapRow)
                    {
                        if (terrainMapping.ContainsKey(c))
                        {
                            continue;
                        }

                        if (featureChars.Contains(c))
                        {
                            continue;
                        }

                        if (specialChars.Contains(c))
                            continue;

                        //Otherwise we don't know what it is
                        //Now we have literals so we don't do this
                        //LogFile.Log.LogEntry("Unknown char" + c.ToString() + " in file: " + filename.ToString() );
                        //throw new ApplicationException("Unknown char " + c.ToString() + " in file: " + filename.ToString() );
                    }
                }                
          
                //Set height
                height = storedMapRows.Count;

                //Confirm load success
                fileLoaded = true;
            }
            catch (Exception e)
            {
                LogFile.Log.LogEntry("Can't load file: " + filenameRoot + " because: " + e.Message);
                throw new ApplicationException("Can't load file: " + filenameRoot + " because: " + e.Message);
            }
        }


        /// <summary>
        /// Adds the complete map to the dungeon. Throw exception on failure, but shouldn't fail
        /// </summary>
        /// <returns></returns>
        public void AddMapToDungeon()
        {
            if (!fileLoaded)
            {
                LogFile.Log.LogEntry("MapGeneratorFromASCIIFile::AddMapToDungeon: No map loaded");
                throw new ApplicationException("No map loaded");
            }

            baseMap = new Map(width, height);

            baseMap.LightLevel = 0;

            int row = 0;
            
            //Sort out the terrain first
            //Features and special areas are empty

            foreach (string mapRow in storedMapRows)
            {
                for (int i = 0; i < width; i++)
                {
                    char mapChar = mapRow[i];

                    MapTerrain thisTerrain;
                    MapSquare thisSquare = baseMap.mapSquares[i, row];

                    //if this square is terrain
                    if (terrainMapping.ContainsKey(mapChar))
                    {
                        thisTerrain = terrainMapping[mapChar];
                        
                    }
                    else {

                        //Features and special chars are empty
                        if (featureChars.Contains(mapChar))
                            thisTerrain = MapTerrain.Empty;
                        else if (specialChars.Contains(mapChar))
                            thisTerrain = MapTerrain.Empty;
                        //OK it's not a special, it's a literal
                        else
                        {
                            thisTerrain = MapTerrain.Literal;
                        }
                    }
                    
                    //Set terrain and features
                    thisSquare.Terrain = thisTerrain;

                    if (thisTerrain == MapTerrain.Literal)
                        thisSquare.terrainLiteral = mapChar;

                     //This should be done in the map gen functions - right now dungeon does a bit of it too
                    switch (thisTerrain)
                    {
                        case MapTerrain.Wall:
                        case MapTerrain.Void:
                        case MapTerrain.Literal:
                            thisSquare.Walkable = false;
                            thisSquare.BlocksLight = true;
                            break;
                        case MapTerrain.Empty:
                            thisSquare.Walkable = true;
                            thisSquare.BlocksLight = false;
                            break;
                        case MapTerrain.Mountains:
                            thisSquare.Walkable = false;
                            thisSquare.BlocksLight = true;
                            break;
                        case MapTerrain.Trees:
                            thisSquare.Walkable = false;
                            thisSquare.BlocksLight = false;
                            break;
                        case MapTerrain.River:
                            thisSquare.Walkable = false;
                            thisSquare.BlocksLight = false;
                            break;
                        case MapTerrain.Road:
                            thisSquare.Walkable = true;
                            thisSquare.BlocksLight = false;
                            break;
                        case MapTerrain.Grass:
                            thisSquare.Walkable = true;
                            thisSquare.BlocksLight = false;
                            break;
                    }
                }
                
                row++;
            }

            //Add the (terrain complete) map to the dungeon before adding features and specials
            int levelNo = Game.Dungeon.AddMap(baseMap);

            //Sort out features

            row = 0;

            foreach (string mapRow in storedMapRows)
            {
                for (int i = 0; i < width; i++)
                {
                    char mapChar = mapRow[i];

                   if (featureChars.Contains(mapChar))
                    {
                        bool featureAddSuccess = false;
                       switch (mapChar)
                        {
                            case '>':
                                featureAddSuccess = Game.Dungeon.AddFeature(new Features.StaircaseDown(), levelNo, new Point(i, row));
                                break;
                            case '<':
                                featureAddSuccess = Game.Dungeon.AddFeature(new Features.StaircaseUp(), levelNo, new Point(i, row));
                                break;
                        }

                       if (!featureAddSuccess)
                       {
                           LogFile.Log.LogEntry("MapGeneratorFromASCIIFile::AddMapToDungeon: Failed to add terrain feature");
                           throw new ApplicationException("Failed to add feature");
                       }
                    }
                }

                row++;
            }

            //Sort out special characters

            row = 0;

            foreach (string mapRow in storedMapRows)
            {
                for (int i = 0; i < width; i++)
                {
                    char mapChar = mapRow[i];

                    if (specialChars.Contains(mapChar))
                    {
                        switch (mapChar)
                        {
                            //PC start location is meaningless for everything except the first level
                            case 'x':
                                baseMap.PCStartLocation = new Point(i, row);
                                break;
                            case '1':
                                Game.Dungeon.AddTrigger(levelNo, new Point(i, row), new Triggers.TrainAthleticsTrigger());
                                break;
                            case '2':
                                Game.Dungeon.AddTrigger(levelNo, new Point(i, row), new Triggers.TrainCombatTrigger());
                                break;
                            case '3':
                                Game.Dungeon.AddTrigger(levelNo, new Point(i, row), new Triggers.TrainRestTrigger());
                                break;
                            case '4':
                                Game.Dungeon.AddTrigger(levelNo, new Point(i, row), new Triggers.TrainCharmTrigger());
                                break;
                            case '5':
                                Game.Dungeon.AddTrigger(levelNo, new Point(i, row), new Triggers.TrainMagicTrigger());
                                break;
                            case '6':
                                Game.Dungeon.AddTrigger(levelNo, new Point(i, row), new Triggers.TrainMagicLibraryTrigger());
                                break;
                            case '7':
                                Game.Dungeon.AddTrigger(levelNo, new Point(i, row), new Triggers.TrainGeographyLibraryTrigger());
                                break;
                            case '8':
                                Game.Dungeon.AddTrigger(levelNo, new Point(i, row), new Triggers.TrainMasterTrigger());
                                break;
                            case '9':
                                Game.Dungeon.AddTrigger(levelNo, new Point(i, row), new Triggers.TrainLeaveSchoolTrigger());
                                break;
                            case '0':
                                Game.Dungeon.AddTrigger(levelNo, new Point(i, row), new Triggers.TownToWilderness());
                                break;
                            case '%':
                                Game.Dungeon.AddDecorationFeature(new Features.Corpse(), levelNo, new Point(i, row));
                                break;
                            case 'Y':
                                Game.Dungeon.AddMonster(new Creatures.Lich(), levelNo, new Point(i, row));
                                break;
                            case 'G':
                                Game.Dungeon.AddMonster(new Creatures.Friend(), levelNo, new Point(i, row));
                                break;
                        }
                    }
                }

                row++;
            }

        }

        public Point RandomPointInRoom()
        {
            do
            {
                int x = Game.Random.Next(width);
                int y = Game.Random.Next(height);

                if (baseMap.mapSquares[x, y].Terrain == MapTerrain.Empty)
                {
                    return new Point(x, y);
                }
            }
            while (true);
        }

        internal Point GetPCStartLocation()
        {
            return baseMap.PCStartLocation;
        }
    }
}
