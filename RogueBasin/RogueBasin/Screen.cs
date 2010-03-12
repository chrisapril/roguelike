﻿using System;
using System.Collections.Generic;
using System.Text;
using libtcodWrapper;
using Console = System.Console;
using System.IO;
using System.Windows.Forms;
using System.Reflection;

namespace RogueBasin {

    //Represents our screen
    public class Screen
    {
        static Screen instance = null;

        //Console/screen size
        public int Width { get; set; }
        public int Height { get; set; }

        bool debugMode = true;

        //Top left coord to start drawing the map at
        Point mapTopLeft;

        /// <summary>
        /// Dimensions of message display area
        /// </summary>
        Point msgDisplayTopLeft;
        public int msgDisplayNumLines;

        Point statsDisplayTopLeft;
        Point princessStatsTopLeft;

        Point hitpointsOffset;
        Point maxHitpointsOffset;
        Point overdriveHitpointsOffset;
        Point speedOffset;
        Point worldTickOffset;
        Point levelOffset;

        Point armourOffset;
        Point damageOffset;
        Point playerLevelOffset;

        Point charmOffset;

        Point specialMoveStatusLine;
        Point spellStatusLine;
        Point trainStatsLine;

        Point calendarOffset;

        Color inFOVTerrainColor = ColorPresets.White;
        Color seenNotInFOVTerrainColor = ColorPresets.Gray;
        Color neverSeenFOVTerrainColor;
        Color inMonsterFOVTerrainColor = ColorPresets.Blue;

        
        Color creatureColor = ColorPresets.White;
        Color itemColor = ColorPresets.Red ;
        Color featureColor = ColorPresets.White;

        Color hiddenColor = ColorPresets.Black;

        Color charmBackground = ColorPresets.DarkKhaki;
        Color passiveBackground = ColorPresets.DarkMagenta;
        Color normalBackground = ColorPresets.Black;
        Color normalForeground = ColorPresets.White;

        Color targetBackground = ColorPresets.White;
        Color targetForeground = ColorPresets.Black;

        Color literalColor = ColorPresets.White;

        //Keep enough state so that we can draw each screen
        string lastMessage = "";

        //Inventory
        Point inventoryTL;
        Point inventoryTR;
        Point inventoryBL;

        //Training
        Point trainingTL;
        Point trainingTR;
        Point trainingBL;

        bool displayInventory;
        
        /// <summary>
        /// Equipment screen is displayed
        /// </summary>
        bool displayEquipment;

        /// <summary>
        /// Select new equipment screen is displayed
        /// </summary>
        bool displayEquipmentSelect;

        bool displaySpecialMoveMovies;

        bool displaySpells;

        bool displayTrainingUI;

        //Death members
        public List<string> TotalKills { get; set; }
        public List<string> DeathPreamble { get; set; }

        Point DeathTL { get; set; }
        int DeathWidth { get; set; }
        int DeathHeight { get; set; }

        int selectedInventoryIndex;
        int topInventoryIndex;

        Inventory currentInventory;
        List<EquipmentSlotInfo> currentEquipment;
        string inventoryTitle;
        string inventoryInstructions;

        Point movieTL = new Point(5, 5);
        int movieWidth = 80;
        int movieHeight = 25;
        uint movieMSBetweenFrames = 500;

        /// <summary>
        /// Targetting mode
        /// </summary>
        bool targettingMode = false;

        /// <summary>
        /// Targetting cursor
        /// </summary>
        public Point Target { get; set; }

        //Current movie
        List <MovieFrame> movieFrames;

        public Color PCColor { get; set;}

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
            Width = 90;
            Height = 35;

            mapTopLeft = new Point(5, 5);

            msgDisplayTopLeft = new Point(0, 1);
            msgDisplayNumLines = 3;

            inventoryTL = new Point(5, 5);
            inventoryTR = new Point(75, 5);
            inventoryBL = new Point(5, 30);

            trainingTL = new Point(10, 10);
            trainingTR = new Point(70, 10);
            trainingBL = new Point(25, 25);

            princessStatsTopLeft = new Point(7, 32);
            charmOffset = new Point(6, 0);

            calendarOffset = new Point(20, 0);

            specialMoveStatusLine = new Point(7, 33);
            spellStatusLine = new Point(7, 34);
            trainStatsLine = new Point(7, 30);
           
            //Colors
            neverSeenFOVTerrainColor = Color.FromRGB(90, 90, 90);

            TotalKills = null;

            DeathTL = new Point(1, 1);
            DeathWidth = 89;
            DeathHeight = 34;

            PCColor = ColorPresets.White;

            trainingStatsRecord = new List<TrainStats>();
        }

        //Setup the screen
        public void InitialSetup()
        {

            CustomFontRequest fontReq = new CustomFontRequest("tallfont.png", 8, 16, CustomFontRequestFontTypes.LayoutAsciiInColumn);
            RootConsole.Width = Width;
            RootConsole.Height = Height;
            RootConsole.WindowTitle = "DDRogue";
            RootConsole.Fullscreen = false;
            RootConsole.Font = fontReq;
            /*
            RootConsole rootConsole = RootConsole.GetInstance();

            rootConsole.PrintLine("Hello world!", 30, 30, LineAlignment.Left);
            rootConsole.Flush();
            */
            Console.WriteLine("debug test message.");

        }

        /// <summary>
        /// Draw a fireball effect
        /// </summary>
        /// <param name="sqs"></param>
        /// <param name="color"></param>
        public void DrawFlashSquares(List<Point> sqs, Color color)
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            //Draw the screen as normal
            Draw();

            //Draw the flash overlay

            foreach (Point sq in sqs)
            {
                rootConsole.ForegroundColor = color;
                rootConsole.PutChar(mapTopLeft.x + sq.x, mapTopLeft.y + sq.y, '*');
            }

            rootConsole.ForegroundColor = normalForeground;

            FlushConsole();

            //Wait
            TCODSystem.Sleep(200);

            //Redraw
            Draw();
            FlushConsole();
        }

        /// <summary>
        /// Draw a flash effect of a line
        /// Start is the origin (line is not drawn here, but origin used to calculate shape of line)
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="color"></param>
        public void DrawFlashLine(Point start, Point end, Color color)
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            //Draw the screen as normal
            Draw();

            //Draw the line overlay

            //Cast a line between the start and end
            TCODLineDrawing.InitLine(start.x, start.y, end.x, end.y);

            int currentX = start.x;
            int currentY = start.y;

            bool finishedLine = TCODLineDrawing.StepLine(ref currentX, ref currentY);
            
            int deltaX = end.x - start.x;
            int deltaY = end.y - start.y;

            char drawChar = '-';

            if(deltaX < 0 && deltaY < 0)
                drawChar = '\\';
            else if(deltaX < 0 && deltaY > 0)
                drawChar = '/';
            else if(deltaX > 0 && deltaY < 0)
                drawChar = '/';
            else if(deltaX > 0 && deltaY > 0)
                drawChar = '\\';
            else if(deltaY == 0)
                drawChar = '-';
            else if(deltaX == 0)
                drawChar = '|';

            rootConsole.ForegroundColor = color;

            while(!finishedLine) {

                rootConsole.PutChar(mapTopLeft.x + currentX, mapTopLeft.y + currentY, drawChar);
                finishedLine = TCODLineDrawing.StepLine(ref currentX, ref currentY);

            }

            rootConsole.ForegroundColor = normalForeground;

            FlushConsole();

            //Wait
            TCODSystem.Sleep(200);

            //For screenshots only
            //KeyPress userKey = Keyboard.WaitForKeyPress(true);

            //Redraw
            Draw();
            FlushConsole();
        }

        /// <summary>
        /// Call after all drawing is complete to output onto screen
        /// </summary>
        public void FlushConsole()
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            rootConsole.Flush();
        }

        

        public void TargettingModeOn() {
            targettingMode = true;
        }

        public void TargettingModeOff()
        {
            targettingMode = false;
        }

        /// <summary>
        /// Play the movie indicated by the filename root.
        /// </summary>
        /// <param name="root"></param>
        /// 
        Color normalMovieColor = ColorPresets.White;
        Color flashMovieColor = ColorPresets.Red;

        public void PlayMovie(string filenameRoot, bool keypressBetweenFrames)
        {
            if (filenameRoot == "" || filenameRoot.Length == 0)
            {
                LogFile.Log.LogEntryDebug("Not playing movie with no name", LogDebugLevel.Medium);
                return;
            }

            try
            {

                //Draw the basis of the screen
                Draw();

                //Get screen handle
                RootConsole rootConsole = RootConsole.GetInstance();

                //Load whole movie
                LoadMovie(filenameRoot);

                //Use the width and height of the first frame to centre the movie
                //Unlikely to be any control codes on the first line
                int width = movieFrames[0].width;
                int height = movieFrames[0].height;

                int xOffset = (movieWidth - width) / 2;
                int yOffset = (movieHeight - height) / 2;

                Point frameTL = new Point(movieTL.x + xOffset, movieTL.y + yOffset);
                
                int frameNo = 0;

                //Draw each frame of the movie
                foreach (MovieFrame frame in movieFrames)
                {

                    //Draw frame
                    rootConsole.DrawFrame(movieTL.x, movieTL.y, movieWidth, movieHeight, true);

                    //Draw content
                    List<string> scanLines = frame.scanLines;

                    bool hasFlashingChars = DrawMovieFrame(frame.scanLines, frameTL, width, true);

                    if (hasFlashingChars)
                    {
                        //Wait and then redraw without the highlight to make a flash effect
                        Screen.Instance.FlushConsole();
                        TCODSystem.Sleep(movieMSBetweenFrames);
                        DrawMovieFrame(frame.scanLines, frameTL, width, false);
                    }

                    
                    if (keypressBetweenFrames == true)
                    {
                        //Don't ask for a key press if it's the last frame, one will happen below automatically
                        if (frameNo != movieFrames.Count - 1)
                        {
                            rootConsole.PrintLineRect("Press any key to continue", movieTL.x + movieWidth / 2, movieTL.y + movieHeight - 2, movieWidth, 1, LineAlignment.Center);
                            Screen.Instance.FlushConsole();
                            KeyPress userKey = Keyboard.WaitForKeyPress(true);
                        }
                    }
                    else
                    {
                        //Wait for the specified time

                        Screen.Instance.FlushConsole();
                        TCODSystem.Sleep(movieMSBetweenFrames);
                    }

                    frameNo++;
                }

                //Print press any key
                rootConsole.PrintLineRect("Press ENTER to continue", movieTL.x + movieWidth / 2, movieTL.y + movieHeight - 2, movieWidth, 1, LineAlignment.Center);

                Screen.Instance.FlushConsole();

                //Await keypress then redraw normal screen
                WaitForEnterKey();

                DrawAndFlush();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to play movie: " + filenameRoot + " : " + ex.Message);
            }
        }

        /// <summary>
        /// Wait for ENTER
        /// </summary>
        private void WaitForEnterKey()
        {
            while (true)
            {
                KeyPress userKey = Keyboard.WaitForKeyPress(true);

                if (userKey.KeyCode == KeyCode.TCODK_ENTER)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Draw a frame. If flashOn then highlight flashing squares in red
        /// </summary>
        /// <param name="scanLines"></param>
        /// <param name="frameTL"></param>
        /// <param name="width"></param>
        /// <param name="flashOn"></param>
        private bool DrawMovieFrame(List<string> scanLines, Point frameTL, int width, bool flashOn)
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            int offset = 0;

            bool flashingChars = false;
            char flashChar = '£';

            foreach (string line in scanLines)
            {
                //Check for special characters
                if (line.Contains(flashChar.ToString()))
                {
                    //We will return this, so that the caller knows to call us again with flashOn = false
                    flashingChars = true;

                    //Print char by char
                    int coffset = 0;
                    bool nextCharFlash = false;
                    foreach (char c in line)
                    {
                        if (c == flashChar)
                        {
                            if (flashOn)
                            {
                                nextCharFlash = true;
                            }
                            //Skip this char
                            continue;
                        }

                        if (nextCharFlash)
                        {
                            rootConsole.ForegroundColor = flashMovieColor;
                            nextCharFlash = false;
                        }
                        else
                        {
                            rootConsole.ForegroundColor = normalMovieColor;
                        }

                        rootConsole.PutChar(frameTL.x + coffset, frameTL.y + offset, c);
                        coffset++;
                    }
                }
                else
                {
                    //Print whole line
                    rootConsole.PrintLineRect(line, frameTL.x, frameTL.y + offset, width, 1, LineAlignment.Left);
                }
                offset++;
            }

            return flashingChars;
        }

        private void LoadMovie(string filenameRoot)
        {
            try
            {
                LogFile.Log.LogEntry("Loading movie: " + filenameRoot);

                int frameNo = 0;

                movieFrames = new List<MovieFrame>();

                Assembly _assembly = Assembly.GetExecutingAssembly();

                //MessageBox.Show("Showing all embedded resource names");

                //string[] names = _assembly.GetManifestResourceNames();
                //foreach (string name in names)
                //    MessageBox.Show(name);

                do
                {
                    string filename = "RogueBasin.bin.Debug.movies." + filenameRoot + frameNo.ToString() + ".amf";
                    Stream _fileStream = _assembly.GetManifestResourceStream(filename);

                    //If this is the first frame check if there is at least one frame
                    if (frameNo == 0)
                    {
                        if (_fileStream == null)
                        {
                            throw new ApplicationException("Can't find file: " + filename);
                        }
                    }
                    //Otherwise, not finding a file just means the end of a movie

                    if (_fileStream == null)
                    {
                        break;
                    }

                    //File exists, load the frame
                    MovieFrame frame = new MovieFrame();

                    using (StreamReader reader = new StreamReader(_fileStream))
                    {
                        string thisLine;

                        frame.scanLines = new List<string>();

                        while ((thisLine = reader.ReadLine()) != null)
                        {
                            frame.scanLines.Add(thisLine);
                        }

                        //Set width and height

                        //Calculate dimensions
                        frame.width = 0;

                        foreach (string row in frame.scanLines)
                        {
                            if (row.Length > frame.width)
                                frame.width = row.Length;
                        }

                        frame.height = frame.scanLines.Count;

                        //Add the frame
                        movieFrames.Add(frame);

                        //Increment the frame no
                        frameNo++;
                    }
                } while (true);

            }
            catch (Exception e)
            {
                LogFile.Log.LogEntry("Failed to load movie: " + e.Message);
            }
        }

        /// <summary>
        /// Draws and updates the screen. Doesn't run message queue, since that's in RogueBase and not accessible
        /// </summary>
        public void DrawAndFlush()
        {
            Screen.Instance.Draw();
            Screen.Instance.FlushConsole();
        }

        //Draw the current dungeon map and objects
        public void Draw()
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            Dungeon dungeon = Game.Dungeon;
            Player player = dungeon.Player;

            //Clear screen
            rootConsole.Clear();

            //Draw the map screen

            //Draw terrain
            DrawMap(dungeon.PCMap);

            //Draw fixed features
            DrawFeatures(dungeon.Features);

            //Draw items (will appear on top of staircases etc.)
            DrawItems(dungeon.Items);

            //Draw creatures
            DrawCreatures(dungeon.Monsters);

            //Draw PC

            Point PClocation = player.LocationMap;

            rootConsole.ForegroundColor = PCColor;
            rootConsole.PutChar(mapTopLeft.x + PClocation.x, mapTopLeft.y + PClocation.y, player.Representation);
            rootConsole.ForegroundColor = ColorPresets.White;

            //Draw Stats
            DrawStats(dungeon.Player);

            //Draw targetting cursor
            if (targettingMode)
                DrawTargettingCursor();

            //If we're in town draw town overlays
            if (Game.Dungeon.Player.LocationLevel == 0)
            {
                DrawCalendar();
                DrawStatsBox();
            }

            //Draw any overlay screens
            if (displayInventory)
                DrawInventory();
            else if (displayEquipment)
                DrawEquipment();
            else if (displayEquipmentSelect)
                DrawEquipmentSelect();
            else if (displaySpecialMoveMovies)
                DrawMovieOverlay();
            else if (displaySpells)
                DrawSpellOverlay();
            else if (displayTrainingUI)
                DrawTrainingOverlay();

        }

        private void DrawTargettingCursor()
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            int xLoc = mapTopLeft.x + Target.x;
            int yLoc = mapTopLeft.y + Target.y;

            //Get what's there
            char charAtPoint = rootConsole.GetChar(xLoc, yLoc);

            //Replace with the same but with targetting background

            rootConsole.BackgroundColor = targetBackground;
            rootConsole.ForegroundColor = targetForeground;

            rootConsole.PutChar(xLoc, yLoc, charAtPoint);

            rootConsole.BackgroundColor = normalBackground;
            rootConsole.ForegroundColor = normalForeground;

        }

        /// <summary>
        /// Screen for player death
        /// </summary>
        public void DrawDeathScreen()
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            //Clear screen
            rootConsole.Clear();

            //Draw frame
            rootConsole.DrawFrame(DeathTL.x, DeathTL.y, DeathWidth, DeathHeight, true);

            //Draw title
            rootConsole.PrintLineRect("And it was all going so well...", DeathTL.x + DeathWidth / 2, DeathTL.y, DeathWidth, 1, LineAlignment.Center);

            //Draw preamble
            int count = 0;
            foreach (string s in DeathPreamble)
            {
                rootConsole.PrintLineRect(s, DeathTL.x + 2, DeathTL.y + 2 + count, DeathWidth - 4, 1, LineAlignment.Left);
                count++;
            }

            //Draw kills

            rootConsole.PrintLineRect("Total Kills", DeathTL.x + DeathWidth / 2, DeathTL.y + 2 + count + 2, DeathWidth, 1, LineAlignment.Center);

            foreach (string s in TotalKills)
            {
                rootConsole.PrintLineRect(s, DeathTL.x + 2, DeathTL.y + 2 + count + 4, DeathWidth - 4, 1, LineAlignment.Left);
                count++;
            }

            //Draw instructions

            rootConsole.PrintLineRect("Press any key to exit...", DeathTL.x + DeathWidth / 2, DeathTL.y + DeathHeight - 1, DeathWidth, 1, LineAlignment.Center);
        }

        /// <summary>
        /// Screen for player victory
        /// </summary>
        public void DrawVictoryScreen()
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            //Clear screen
            rootConsole.Clear();

            //Draw frame
            rootConsole.DrawFrame(DeathTL.x, DeathTL.y, DeathWidth, DeathHeight, true);

            //Draw title
            rootConsole.PrintLineRect("VICTORY!", DeathTL.x + DeathWidth / 2, DeathTL.y, DeathWidth, 1, LineAlignment.Center);

            //Draw preamble
            int count = 0;
            foreach (string s in DeathPreamble)
            {
                rootConsole.PrintLineRect(s, DeathTL.x + 2, DeathTL.y + 2 + count, DeathWidth - 4, 1, LineAlignment.Left);
                count++;
            }

            //Draw kills

            rootConsole.PrintLineRect("Total Kills", DeathTL.x + DeathWidth / 2, DeathTL.y + 2 + count + 2, DeathWidth, 1, LineAlignment.Center);

            foreach (string s in TotalKills)
            {
                rootConsole.PrintLineRect(s, DeathTL.x + 2, DeathTL.y + 2 + count + 4, DeathWidth - 4, 1, LineAlignment.Left);
                count++;
            }

            //Draw instructions

            rootConsole.PrintLineRect("Press any key to exit...", DeathTL.x + DeathWidth / 2, DeathTL.y + DeathHeight - 1, DeathWidth, 1, LineAlignment.Center);
        }

        /// <summary>
        /// Display inventory overlay
        /// </summary>
        private void DrawInventory()
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            //Draw frame
            rootConsole.DrawFrame(inventoryTL.x, inventoryTL.y, inventoryTR.x - inventoryTL.x + 1, inventoryBL.y - inventoryTL.y + 1, true);

            //Draw title
            rootConsole.PrintLineRect(inventoryTitle, (inventoryTL.x + inventoryTR.x) / 2, inventoryTL.y, inventoryTR.x - inventoryTL.x, 1, LineAlignment.Center);

            //Draw instructions
            rootConsole.PrintLineRect(inventoryInstructions, (inventoryTL.x + inventoryTR.x) / 2, inventoryBL.y, inventoryTR.x - inventoryTL.x, 1, LineAlignment.Center);

            //List the inventory
            
            //Inventory area is slightly reduced from frame
            int inventoryListX = inventoryTL.x + 2;
            int inventoryListW = inventoryTR.x - inventoryTL.x - 4;
            int inventoryListY = inventoryTL.y + 2;
            int inventoryListH = inventoryBL.y - inventoryTL.y - 4;

            List<InventoryListing> inventoryList = currentInventory.InventoryListing;

            for (int i = 0; i < inventoryListH; i++)
            {
                int inventoryIndex = topInventoryIndex + i;

                //End of inventory
                if (inventoryIndex == inventoryList.Count)
                    break;

                //Create entry string
                char selectionChar = (char)((int)'a' + i);
                string entryString = "(" + selectionChar.ToString() + ") " + inventoryList[inventoryIndex].Description;

                //Print entry
                rootConsole.PrintLineRect(entryString, inventoryListX, inventoryListY + i, inventoryListW, 1, LineAlignment.Left);
            }
        }

        /// <summary>
        /// Draw a calendar overlay
        /// </summary>
        private void DrawCalendar()
        {

            Point calendarTL = new Point(58, 6);
            Point calendarBR = new Point(81, 16);


            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            //Draw frame
            rootConsole.DrawFrame(calendarTL.x, calendarTL.y, calendarBR.x - calendarTL.x + 1, calendarBR.y - calendarTL.y + 1, true);

            //Draw title
            //rootConsole.PrintLineRect("Calendar", (calendarTL.x + calendarBR.x) / 2, calendarTL.y, calendarBR.x - calendarTL.x, 1, LineAlignment.Center);

            //Draw calendar

            int monthNo = Game.Dungeon.GetDateMonth();
            int dayNo = Game.Dungeon.GetDateDay();

            //Draw month name
            string [] monthNames = { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };
            rootConsole.PrintLineRect(monthNames[monthNo - 1], (calendarTL.x + calendarBR.x) / 2, calendarTL.y, calendarBR.x - calendarTL.x, 1, LineAlignment.Center);

            Point calendarOffset = new Point(2, 2);

            Color selectedDayColor = ColorPresets.Red;
            Color normalDayColor = ColorPresets.White;

            //Draw days
            for (int j = 0; j < 4; j++)
            {
                for (int i = 0; i < 7; i++)
                {
                    int thisDay = (j * 7 + i) + 1;

                    if (thisDay == dayNo)
                    {
                        rootConsole.ForegroundColor = selectedDayColor;
                    }

                    rootConsole.PrintLine(thisDay.ToString(), (calendarTL.x + calendarOffset.x + i * 3), calendarTL.y + calendarOffset.y + 2 * j, LineAlignment.Left);
                    rootConsole.ForegroundColor = normalDayColor;
                }
            }
        }

        /// <summary>
        /// Draw a stats box overlay
        /// </summary>
        private void DrawStatsBox()
        {

            Point statsBoxTL = new Point(58, 18);
            Point statsBoxBR = new Point(81, 27);

            Point statTitleOffset = new Point(3, 2);
            Point statDataOffset = new Point(17, 2);

            Color statNumberColor = ColorPresets.CadetBlue;

            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            //Draw frame
            rootConsole.DrawFrame(statsBoxTL.x, statsBoxTL.y, statsBoxBR.x - statsBoxTL.x + 1, statsBoxBR.y - statsBoxTL.y + 1, true);

            //Draw title
            rootConsole.PrintLineRect("Statistics", (statsBoxTL.x + statsBoxBR.x) / 2, statsBoxTL.y, statsBoxBR.x - statsBoxTL.x, 1, LineAlignment.Center);

            //Draw PrincessRL training stats

            Player player = Game.Dungeon.Player;

            string trainHitpointsString = player.HitpointsStat.ToString();
            string trainMaxHitpointsString = player.MaxHitpointsStat.ToString();
            string trainAttackString = player.AttackStat.ToString();
            string trainSpeedString = player.SpeedStat.ToString();
            string trainCharmString = player.CharmStat.ToString();
            string trainMagicString = player.MagicStat.ToString();

            rootConsole.PrintLine("Stamina:", statsBoxTL.x + statTitleOffset.x, statsBoxTL.y + statTitleOffset.y + 0, LineAlignment.Left);
            rootConsole.PrintLine("Health:", statsBoxTL.x + statTitleOffset.x, statsBoxTL.y + statTitleOffset.y + 1, LineAlignment.Left);
            rootConsole.PrintLine("Combat Skill:", statsBoxTL.x + statTitleOffset.x, statsBoxTL.y + statTitleOffset.y + 2, LineAlignment.Left);
            rootConsole.PrintLine("Dexterity:", statsBoxTL.x + statTitleOffset.x, statsBoxTL.y + statTitleOffset.y + 3, LineAlignment.Left);
            rootConsole.PrintLine("Charm:", statsBoxTL.x + statTitleOffset.x, statsBoxTL.y + statTitleOffset.y + 4, LineAlignment.Left);
            rootConsole.PrintLine("Magic skill:", statsBoxTL.x + statTitleOffset.x, statsBoxTL.y + statTitleOffset.y + 5, LineAlignment.Left);

            rootConsole.ForegroundColor = statNumberColor;

            rootConsole.PrintLine(trainHitpointsString, statsBoxTL.x + statDataOffset.x, statsBoxTL.y + statDataOffset.y + 0, LineAlignment.Left);
            rootConsole.PrintLine(trainMaxHitpointsString, statsBoxTL.x + statDataOffset.x, statsBoxTL.y + statDataOffset.y + 1, LineAlignment.Left);
            rootConsole.PrintLine(trainAttackString, statsBoxTL.x + statDataOffset.x, statsBoxTL.y + statDataOffset.y + 2, LineAlignment.Left);
            rootConsole.PrintLine(trainSpeedString, statsBoxTL.x + statDataOffset.x, statsBoxTL.y + statDataOffset.y + 3, LineAlignment.Left);
            rootConsole.PrintLine(trainCharmString, statsBoxTL.x + statDataOffset.x, statsBoxTL.y + statDataOffset.y + 4, LineAlignment.Left);
            rootConsole.PrintLine(trainMagicString, statsBoxTL.x + statDataOffset.x, statsBoxTL.y + statDataOffset.y + 5, LineAlignment.Left);

            rootConsole.ForegroundColor = ColorPresets.White;
        }

        /// <summary>
        /// Display equipment select overview
        /// </summary>
        private void DrawEquipmentSelect()
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            //Draw frame
            rootConsole.DrawFrame(inventoryTL.x, inventoryTL.y, inventoryTR.x - inventoryTL.x + 1, inventoryBL.y - inventoryTL.y + 1, true);

            //Draw title
            rootConsole.PrintLineRect(inventoryTitle, (inventoryTL.x + inventoryTR.x) / 2, inventoryTL.y, inventoryTR.x - inventoryTL.x, 1, LineAlignment.Center);

            //Draw instructions
            rootConsole.PrintLineRect(inventoryInstructions, (inventoryTL.x + inventoryTR.x) / 2, inventoryBL.y, inventoryTR.x - inventoryTL.x, 1, LineAlignment.Center);

            //List the inventory

            //Inventory area is slightly reduced from frame
            int inventoryListX = inventoryTL.x + 2;
            int inventoryListW = inventoryTR.x - inventoryTL.x - 4;
            int inventoryListY = inventoryTL.y + 2;
            int inventoryListH = inventoryBL.y - inventoryTL.y - 4;

            List<InventoryListing> inventoryList = currentInventory.InventoryListing;

            for (int i = 0; i < inventoryListH; i++)
            {
                int inventoryIndex = topInventoryIndex + i;

                //End of inventory
                if (inventoryIndex == inventoryList.Count)
                    break;

                //Create entry string
                char selectionChar = (char)((int)'a' + i);
                string entryString = "(" + selectionChar.ToString() + ") " + inventoryList[inventoryIndex].Description;

                //Add equipped status
                //Only consider the first item in a stack, since equipped items can't stack
                Item firstItemInStack = currentInventory.Items[inventoryList[inventoryIndex].ItemIndex[0]];

                EquipmentSlotInfo equippedInSlot = currentEquipment.Find(x => x.equippedItem == firstItemInStack);

                if (equippedInSlot != null)
                {
                    entryString += " (equipped: " + StringEquivalent.EquipmentSlots[equippedInSlot.slotType] + ")";
                }

                //Print entry
                rootConsole.PrintLineRect(entryString, inventoryListX, inventoryListY + i, inventoryListW, 1, LineAlignment.Left);
            }
        }

        /// <summary>
        /// Display equipment select overview
        /// </summary>
        private void DrawEquipment()
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            //Draw frame
            rootConsole.DrawFrame(inventoryTL.x, inventoryTL.y, inventoryTR.x - inventoryTL.x + 1, inventoryBL.y - inventoryTL.y + 1, true);

            //Draw title
            rootConsole.PrintLineRect(inventoryTitle, (inventoryTL.x + inventoryTR.x) / 2, inventoryTL.y, inventoryTR.x - inventoryTL.x, 1, LineAlignment.Center);

            //Draw instructions
            rootConsole.PrintLineRect(inventoryInstructions, (inventoryTL.x + inventoryTR.x) / 2, inventoryBL.y, inventoryTR.x - inventoryTL.x, 1, LineAlignment.Center);

            //List the inventory

            //Inventory area is slightly reduced from frame
            int inventoryListX = inventoryTL.x + 2;
            int inventoryListW = inventoryTR.x - inventoryTL.x - 4;
            int inventoryListY = inventoryTL.y + 2;
            int inventoryListH = inventoryBL.y - inventoryTL.y - 4;

            List<InventoryListing> inventoryList = currentInventory.EquipmentListing;

            for (int i = 0; i < inventoryListH; i++)
            {
                int inventoryIndex = topInventoryIndex + i;

                //End of inventory
                if (inventoryIndex == inventoryList.Count)
                    break;


                //Add equipped status
                //Only consider the first item in a stack, since equipped items can't stack
                Item firstItemInStack = currentInventory.Items[inventoryList[inventoryIndex].ItemIndex[0]];


                //Create entry string
                char selectionChar = (char)((int)'a' + i);
                string entryString = "(" + selectionChar.ToString() + ") " + firstItemInStack.SingleItemDescription; //+" (equipped)";

                //EquipmentSlotInfo equippedInSlot = currentEquipment.Find(x => x.equippedItem == firstItemInStack);

                //if (equippedInSlot != null)
                //{
                 //   entryString += " (equipped: " + StringEquivalent.EquipmentSlots[equippedInSlot.slotType] + ")";
                //}

                //Print entry
                rootConsole.PrintLineRect(entryString, inventoryListX, inventoryListY + i, inventoryListW, 1, LineAlignment.Left);
            }
        }

        /// <summary>
        /// Display movie screen overlay
        /// </summary>
        private void DrawMovieOverlay()
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            //Draw frame - same as inventory
            rootConsole.DrawFrame(inventoryTL.x, inventoryTL.y, inventoryTR.x - inventoryTL.x + 1, inventoryBL.y - inventoryTL.y + 1, true);

            //Draw title
            rootConsole.PrintLineRect("Special moves known", (inventoryTL.x + inventoryTR.x) / 2, inventoryTL.y, inventoryTR.x - inventoryTL.x, 1, LineAlignment.Center);

            //Draw instructions
            rootConsole.PrintLineRect("Select move to replay move pattern or (x) to exit", (inventoryTL.x + inventoryTR.x) / 2, inventoryBL.y, inventoryTR.x - inventoryTL.x, 1, LineAlignment.Center);

            //List the special moves known

            //Active area is slightly reduced from frame
            int inventoryListX = inventoryTL.x + 2;
            int inventoryListW = inventoryTR.x - inventoryTL.x - 4;
            int inventoryListY = inventoryTL.y + 2;
            int inventoryListH = inventoryBL.y - inventoryTL.y - 4;

            int moveIndex = 0;
            List<SpecialMove> knownMoves = new List<SpecialMove>();

            foreach (SpecialMove move in Game.Dungeon.SpecialMoves) {

                //Run out of room - won't happen as written
                if (moveIndex == inventoryListH)
                    break;

                //Don't list unknown moves
                if (!move.Known)
                    continue;

                knownMoves.Add(move);

                char selectionChar = (char)((int)'a' + moveIndex);
                string entryString = "(" + selectionChar.ToString() + ") " + move.MoveName(); //+" (equipped)";

                //Print entry
                rootConsole.PrintLineRect(entryString, inventoryListX, inventoryListY + moveIndex, inventoryListW, 1, LineAlignment.Left);

                moveIndex++;
            }
        }

        /// <summary>
        /// Display spell screen overlay
        /// </summary>
        private void DrawSpellOverlay()
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            //Draw frame - same as inventory
            rootConsole.DrawFrame(inventoryTL.x, inventoryTL.y, inventoryTR.x - inventoryTL.x + 1, inventoryBL.y - inventoryTL.y + 1, true);

            //Draw title
            rootConsole.PrintLineRect("Spells known", (inventoryTL.x + inventoryTR.x) / 2, inventoryTL.y, inventoryTR.x - inventoryTL.x, 1, LineAlignment.Center);

            //Draw instructions
            rootConsole.PrintLineRect("Select a spell to cast or (x) to exit", (inventoryTL.x + inventoryTR.x) / 2, inventoryBL.y, inventoryTR.x - inventoryTL.x, 1, LineAlignment.Center);

            //List the special moves known

            //Active area is slightly reduced from frame
            int inventoryListX = inventoryTL.x + 2;
            int inventoryListW = inventoryTR.x - inventoryTL.x - 4;
            int inventoryListY = inventoryTL.y + 2;
            int inventoryListH = inventoryBL.y - inventoryTL.y - 4;

            int spellIndex = 0;
            List<Spell> knownSpells = new List<Spell>();

            foreach (Spell spell in Game.Dungeon.Spells)
            {

                //Run out of room - won't happen as written
                if (spellIndex == inventoryListH)
                    break;

                //Don't list unknown moves
                if (!spell.Known)
                    continue;

                knownSpells.Add(spell);

                char selectionChar = (char)((int)'a' + spellIndex);
                string entryString = "(" + selectionChar.ToString() + ") " + spell.SpellName() + " MP: " + spell.MPCost().ToString(); //+" (equipped)";

                //Print entry
                rootConsole.PrintLineRect(entryString, inventoryListX, inventoryListY + spellIndex, inventoryListW, 1, LineAlignment.Left);

                spellIndex++;
            }
        }

        public string TrainingTypeString { get; set; }

        bool trainingPause = true;

        public bool TrainingPause
        {
            get
            {
                return trainingPause;
            }
            set
            {
                trainingPause = value;
            }
        }

        List<TrainStats> trainingStatsRecord;

        public void ClearTrainingStatsRecord()
        {
            trainingStatsRecord.Clear();
        }

        public void AddTrainingStatsRecord(TrainStats newStats)
        {
            trainingStatsRecord.Add(newStats);
        }

        int trainingXTemp;
        int trainingYTemp;

        /// <summary>
        /// Display training overlay. Just put up the border and write some text. Calls from the caller will add info.
        /// </summary>
        private void DrawTrainingOverlay()
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            Point statsHeaderOffset = new Point(10, 0);
            Point statsModOffset = new Point(15, 0);
            Point statsDayOffset = new Point(2, 0);

            Point fitnessOffset = new Point(0, 0);
            Point healthOffset = new Point(8, 0);
            Point speedOffset = new Point(16, 0);
            Point combatOffset = new Point(23, 0);
            Point charmOffset = new Point(30, 0);
            Point magicOffset = new Point(36, 0);

            //Draw frame - same as inventory
            rootConsole.DrawFrame(trainingTL.x, trainingTL.y, trainingTR.x - trainingTL.x + 1, trainingBL.y - trainingTL.y + 1, true);

            //Draw title
            rootConsole.PrintLineRect("Training!", (trainingTL.x + trainingTR.x) / 2, trainingTL.y, trainingTR.x - trainingTL.x, 1, LineAlignment.Center);

            //Draw instructions
            rootConsole.PrintLineRect("Press (x) to exit", (trainingTL.x + trainingTR.x) / 2, trainingBL.y, trainingTR.x - trainingTL.x, 1, LineAlignment.Center);

            //Draw headings
            rootConsole.PrintLineRect(TrainingTypeString, (trainingTL.x + trainingTR.x) / 2, trainingTL.y + 2, trainingTR.x - trainingTL.x, 1, LineAlignment.Center);

            //Draw stats
            /*
            int headerY = trainingTL.y + 4;

            rootConsole.PrintLine("Stamina", trainingTL.x + statsHeaderOffset.x + fitnessOffset.x, headerY, LineAlignment.Left);
            rootConsole.PrintLine("Health", trainingTL.x + statsHeaderOffset.x + healthOffset.x, headerY, LineAlignment.Left);
            rootConsole.PrintLine("Speed", trainingTL.x + statsHeaderOffset.x + speedOffset.x, headerY, LineAlignment.Left);
            rootConsole.PrintLine("Combat", trainingTL.x + statsHeaderOffset.x + combatOffset.x, headerY, LineAlignment.Left);
            rootConsole.PrintLine("Charm", trainingTL.x + statsHeaderOffset.x + charmOffset.x, headerY, LineAlignment.Left);
            rootConsole.PrintLine("Magic", trainingTL.x + statsHeaderOffset.x + magicOffset.x, headerY, LineAlignment.Left);
            */
            //Work out the start day
            List<string> dayNames = new List<string>();
            
            if(Game.Dungeon.IsWeekday()) {
                dayNames.Add("Monday");
                dayNames.Add("Tuesday");
                dayNames.Add("Wednesday");
                dayNames.Add("Thursday");
                dayNames.Add("Friday");
            }
            else {
                dayNames.Add("Saturday");
                dayNames.Add("Sunday");
            }

            //Draw all updates
            int lineCount = 0;

            foreach (TrainStats stats in trainingStatsRecord)
            {
                //Pause
                if (trainingPause)
                    TCODSystem.Sleep(400);

                FlushConsole();

                string dayName;
                if (lineCount < dayNames.Count)
                    dayName = dayNames[lineCount];
                else
                {
                    dayName = "";
                    LogFile.Log.LogEntryDebug("Error - couldn't find right day name in training", LogDebugLevel.High);
                }



                trainingYTemp = trainingTL.y + 6 + lineCount;

                //Concatenate display string
                trainingXTemp = statsModOffset.x;

                rootConsole.ForegroundColor = ColorPresets.White;
                rootConsole.PrintLine(dayName + " : ", trainingTL.x + statsDayOffset.x, trainingYTemp, LineAlignment.Left);

                ProcessDelta("Stamina", stats.HitpointsStatDelta);
                ProcessDelta("Health", stats.MaxHitpointsStatDelta);
                ProcessDelta("Combat", stats.AttackStatDelta);
                ProcessDelta("Speed", stats.SpeedStatDelta);
                ProcessDelta("Charm", stats.CharmStatDelta);
                ProcessDelta("Magic", stats.MagicStatDelta);

                //No change
                if (trainingXTemp == statsModOffset.x)
                {
                    rootConsole.ForegroundColor = ColorPresets.White;
                    rootConsole.PrintLine("No change!", trainingTL.x + trainingXTemp, trainingYTemp, LineAlignment.Left);
                }

                lineCount++;
                
            }

            rootConsole.ForegroundColor = ColorPresets.White;
           
        }

        private void ProcessDelta(string statName, int delta)
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            string outString;
            Color colorToPrint;

            if (delta == 0)
                return;

            if (delta > 0)
            {
                outString = statName+ "(+" + delta.ToString() + ")";
                colorToPrint = ColorPresets.Green;
            }
            else
            {
                outString = statName + "(" + delta.ToString() + ")";
                colorToPrint = ColorPresets.Red;
            }
            rootConsole.ForegroundColor = colorToPrint;
            rootConsole.PrintLine(outString, trainingTL.x + trainingXTemp, trainingYTemp, LineAlignment.Left);

            trainingXTemp += outString.Length + 1;
        }
        /*
        /// <summary>
        /// Display training overlay. Just put up the border and write some text. Calls from the caller will add info.
        /// </summary>
        private void DrawTrainingOverlay()
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            //Draw frame - same as inventory
            rootConsole.DrawFrame(trainingTL.x, trainingTL.y, trainingTR.x - trainingTL.x + 1, trainingBL.y - trainingTL.y + 1, true);

            //Draw title
            rootConsole.PrintLineRect("Training!", (trainingTL.x + trainingTR.x) / 2, trainingTL.y, trainingTR.x - trainingTL.x, 1, LineAlignment.Center);

            //Draw headings
            rootConsole.PrintLineRect(TrainingTypeString, (trainingTL.x + trainingTR.x) / 2, trainingTL.y + 2, trainingTR.x - trainingTL.x, 1, LineAlignment.Center);

            //Draw stats
            string statsRow = "Fitness  Health  Speed  Combat  Charm  Magic";

            rootConsole.PrintLineRect(TrainingTypeString, (trainingTL.x + trainingTR.x) / 2, trainingTL.y + 4, trainingTR.x - trainingTL.x, 1, LineAlignment.Center);
        }*/


        /// <summary>
        /// Display equipment overlay
        /// </summary>
        private void DrawEquipmentOld()
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            //Use frame and strings from inventory for now

            //Draw frame
            rootConsole.DrawFrame(inventoryTL.x, inventoryTL.y, inventoryTR.x - inventoryTL.x + 1, inventoryBL.y - inventoryTL.y + 1, true);

            //Draw title
            rootConsole.PrintLineRect(inventoryTitle, (inventoryTL.x + inventoryTR.x) / 2, inventoryTL.y, inventoryTR.x - inventoryTL.x, 1, LineAlignment.Center);

            //Draw instructions
            rootConsole.PrintLineRect(inventoryInstructions, (inventoryTL.x + inventoryTR.x) / 2, inventoryBL.y, inventoryTR.x - inventoryTL.x, 1, LineAlignment.Center);

            //List current slots & items if filled

            //Equipment area is slightly reduced from frame
            int inventoryListX = inventoryTL.x + 2;
            int inventoryListW = inventoryTR.x - inventoryTL.x - 4;
            int inventoryListY = inventoryTL.y + 2;
            int inventoryListH = inventoryBL.y - inventoryTL.y - 4;

            for (int i = 0; i < inventoryListH; i++)
            {
                int inventoryIndex = topInventoryIndex + i;

                //End of inventory
                if (inventoryIndex == currentEquipment.Count)
                    break;

                //Create entry string
                EquipmentSlotInfo currentSlot = currentEquipment[inventoryIndex];

                char selectionChar = (char)((int)'a' + i);
                string entryString = "(" + selectionChar.ToString() + ") " + StringEquivalent.EquipmentSlots[currentSlot.slotType] + ": ";
                if (currentSlot.equippedItem == null)
                    entryString += "Empty";
                else
                    entryString += currentSlot.equippedItem.SingleItemDescription;

                //Print entry
                rootConsole.PrintLineRect(entryString, inventoryListX, inventoryListY + i, inventoryListW, 1, LineAlignment.Left);
            }
        }

        private void DrawStats(Player player)
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            statsDisplayTopLeft = new Point(2, 31);

            hitpointsOffset = new Point(2, 0);
            maxHitpointsOffset = new Point(9, 0);
            overdriveHitpointsOffset = new Point(16, 0);
            armourOffset = new Point(27, 0);
            damageOffset = new Point(34, 0);
            speedOffset = new Point(71, 0);
            playerLevelOffset = new Point(55, 0);
            worldTickOffset = new Point(69, 0);
            levelOffset = new Point(63, 0);
            Point magicOffset = new Point(14, 0);
            Point maxMagicOffset = new Point(21, 0);

            //Draw status line

            string hitpointsString = "HP: " + player.Hitpoints.ToString();
            string maxHitpointsString = "/" + player.MaxHitpoints.ToString();
            //string overdriveHitpointsString = "(" + player.OverdriveHitpoints.ToString() + ")";

            rootConsole.PrintLine(hitpointsString, statsDisplayTopLeft.x + hitpointsOffset.x, statsDisplayTopLeft.y + hitpointsOffset.y, LineAlignment.Left);
            rootConsole.PrintLine(maxHitpointsString, statsDisplayTopLeft.x + maxHitpointsOffset.x, statsDisplayTopLeft.y + maxHitpointsOffset.y, LineAlignment.Left);
            //rootConsole.PrintLine(overdriveHitpointsString, statsDisplayTopLeft.x + overdriveHitpointsOffset.x, statsDisplayTopLeft.y + overdriveHitpointsOffset.y, LineAlignment.Left);


            string mpString = "MP: " + player.MagicPoints.ToString();
            string maxMPString = "/" + player.MaxMagicPoints.ToString();

            rootConsole.PrintLine(mpString, statsDisplayTopLeft.x + magicOffset.x, statsDisplayTopLeft.y + magicOffset.y, LineAlignment.Left);
            rootConsole.PrintLine(maxMPString, statsDisplayTopLeft.x + maxMagicOffset.x, statsDisplayTopLeft.y + maxMagicOffset.y, LineAlignment.Left);

            string armourString = "AC: " + player.ArmourClass().ToString();

            rootConsole.PrintLine(armourString, statsDisplayTopLeft.x + armourOffset.x, statsDisplayTopLeft.y + armourOffset.y, LineAlignment.Left);

            string damageString = "Hit: +" + player.HitModifier() + " Dam: 1d" + player.DamageBase() + "+" + player.DamageModifier();

            rootConsole.PrintLine(damageString, statsDisplayTopLeft.x + damageOffset.x, statsDisplayTopLeft.y + damageOffset.y, LineAlignment.Left);
            string speedString = "Normal";
            
            if (player.Speed > 100)
                speedString = "Fast";
            if (player.Speed > 200)
                speedString = "Very Fast";

            rootConsole.PrintLine(speedString, statsDisplayTopLeft.x + speedOffset.x, statsDisplayTopLeft.y + speedOffset.y, LineAlignment.Left);

            string pLvlString = "Lvl: " + player.Level;

            rootConsole.PrintLine(pLvlString, statsDisplayTopLeft.x + playerLevelOffset.x, statsDisplayTopLeft.y + playerLevelOffset.y, LineAlignment.Left);

            string ticksString = "Tk: " + Game.Dungeon.WorldClock.ToString();

            //rootConsole.PrintLine(ticksString, statsDisplayTopLeft.x + worldTickOffset.x, statsDisplayTopLeft.y + worldTickOffset.y, LineAlignment.Left);

            string levelString = "DL: " + Game.Dungeon.Player.LocationLevel.ToString();

            rootConsole.PrintLine(levelString, statsDisplayTopLeft.x + levelOffset.x, statsDisplayTopLeft.y + levelOffset.y, LineAlignment.Left);

            //Draw PrincessRL training stats

            string trainHitpointsString = "Stamina: " + player.HitpointsStat.ToString();
            string trainMaxHitpointsString = "Health: " + player.MaxHitpointsStat.ToString();
            string trainAttackString = "Attack: " + player.AttackStat.ToString();
            string trainSpeedString = "Speed: " + player.SpeedStat.ToString();
            string trainCharmString = "Charm: " + player.CharmStat.ToString();
            string trainMagicString = "Magic: " + player.MagicStat.ToString();

            rootConsole.PrintLine(trainHitpointsString, trainStatsLine.x + 0, trainStatsLine.y, LineAlignment.Left);
            rootConsole.PrintLine(trainMaxHitpointsString, trainStatsLine.x + 13, trainStatsLine.y, LineAlignment.Left);
            rootConsole.PrintLine(trainAttackString, trainStatsLine.x + 25, trainStatsLine.y, LineAlignment.Left);
            rootConsole.PrintLine(trainSpeedString, trainStatsLine.x + 37, trainStatsLine.y, LineAlignment.Left);
            rootConsole.PrintLine(trainCharmString, trainStatsLine.x + 49, trainStatsLine.y, LineAlignment.Left);
            rootConsole.PrintLine(trainMagicString, trainStatsLine.x + 61, trainStatsLine.y, LineAlignment.Left);

            //Draw PrincessRL specific line

            Point charmPointOffset = new Point(20, 0);

            string charmedString = "ChmMax: " + Game.Dungeon.Player.CurrentCharmedCreatures.ToString() + "/" + Game.Dungeon.Player.MaxCharmedCreatures.ToString();
            rootConsole.PrintLine(charmedString, princessStatsTopLeft.x + charmOffset.x, princessStatsTopLeft.y + charmOffset.y, LineAlignment.Left);

            string charmAbilityStr = "Chm: " + Game.Dungeon.Player.CharmPoints.ToString();
            rootConsole.PrintLine(charmAbilityStr, princessStatsTopLeft.x + charmPointOffset.x, princessStatsTopLeft.y + charmPointOffset.y, LineAlignment.Left);

            //string calendarString = "Month: " + Game.Dungeon.GetDateMonth() + " Day: " + Game.Dungeon.GetDateDay();
            //if (Game.Dungeon.IsWeekday())
            //    calendarString += " Monday";
            //else if (Game.Dungeon.IsNormalWeekend())
            //    calendarString += " Saturday";
            //else if (Game.Dungeon.IsAdventureWeekend())
            //    calendarString += " End of Month";

            //rootConsole.PrintLine(calendarString, princessStatsTopLeft.x + calendarOffset.x, princessStatsTopLeft.y + calendarOffset.y, LineAlignment.Left);

            //Draw moves line

            //Count special moves
            int totalSpecialMoves = Game.Dungeon.SpecialMoves.Count;

            //Abbreviations are 4 characters long + space
            int totalSpecialMoveWidth = 4 * totalSpecialMoves + totalSpecialMoves - 1;

            int specialMoveLineX = (Width - totalSpecialMoveWidth) / 2;
            Point specialMoveDraw = new Point(specialMoveLineX, specialMoveStatusLine.y);

            //Draw each special move status
            foreach (SpecialMove move in Game.Dungeon.SpecialMoves)
            {
                Color drawColor = new Color();

                //Calculate the colour
                
                //Not known - black
                if (!move.Known)
                {
                    drawColor = ColorPresets.DarkGray;
                }
                
                //Known but not in progress, white

                else if (move.CurrentStage() == 0)
                {
                    drawColor = ColorPresets.White;
                }

                //In progress, get increasingly red with stage
                else
                {
                    double percentDone = move.CurrentStage() / (double)move.TotalStages();

                    if (percentDone > 1)
                        percentDone = 1;

                    //Interpolate between red and white
                    drawColor = Color.Interpolate(ColorPresets.White, ColorPresets.Red, percentDone);
                }

                //Draw name of move
                rootConsole.ForegroundColor = drawColor;
                rootConsole.PrintLine(move.Abbreviation(), specialMoveDraw.x, specialMoveDraw.y, LineAlignment.Left);

                //Move along
                specialMoveDraw.x += 5;
            }

            //Draw moves line

            //Count special moves
            int totalSpells = Game.Dungeon.Spells.Count;

            //Abbreviations are 4 characters long + space
            totalSpecialMoveWidth = 4 * totalSpells + totalSpells - 1;

            specialMoveLineX = (Width - totalSpecialMoveWidth) / 2;
            specialMoveDraw = new Point(specialMoveLineX, spellStatusLine.y);

            //Draw each spells status
            foreach (Spell spell in Game.Dungeon.Spells)
            {
                Color drawColor = new Color();

                //Calculate the colour

                //Not known - black
                if (!spell.Known)
                {
                    drawColor = ColorPresets.DarkGray;
                }

                //Known, white

                else
                {
                    drawColor = ColorPresets.White;
                }

                //Draw name of move
                rootConsole.ForegroundColor = drawColor;
                rootConsole.PrintLine(spell.Abbreviation(), specialMoveDraw.x, specialMoveDraw.y, LineAlignment.Left);

                //Move along
                specialMoveDraw.x += 5;
            }

            //Restore to normal colour - not nice
            rootConsole.ForegroundColor = ColorPresets.White;
        }

        private void DrawItems(List<Item> itemList)
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            //Set default colour
            rootConsole.ForegroundColor = itemColor;

            //Could consider storing here and sorting to give an accurate representation of multiple objects

            foreach (Item item in itemList)
            {
                //Don't draw items on creatures
                if (item.InInventory)
                    continue;

                //Don't draw items on other levels
                if (item.LocationLevel != Game.Dungeon.Player.LocationLevel)
                    continue;

                //Colour depending on FOV (for development)
                MapSquare itemSquare = Game.Dungeon.Levels[item.LocationLevel].mapSquares[item.LocationMap.x, item.LocationMap.y];

                //Use the item's colour if it has one
                Color itemColorToUse = item.GetColour();

                //Color itemColorToUse = itemColor;

                if (itemSquare.InPlayerFOV)
                {
                    //In FOV
                    //rootConsole.ForegroundColor = inFOVTerrainColor;
                }
                else if (itemSquare.SeenByPlayer)
                {
                    //Not in FOV but seen
                    //Not in FOV but seen
                    itemColorToUse = Color.Interpolate(itemColor, ColorPresets.Black, 0.5);
                    //rootConsole.ForegroundColor = seenNotInFOVTerrainColor;
                }
                else
                {
                    //Never in FOV
                    if (debugMode)
                        itemColorToUse = itemColor;
                    else
                        itemColorToUse = hiddenColor;
                }

                rootConsole.ForegroundColor = itemColorToUse;
                rootConsole.PutChar(mapTopLeft.x + item.LocationMap.x, mapTopLeft.y + item.LocationMap.y, item.Representation);

                //rootConsole.Flush();
                //KeyPress userKey = Keyboard.WaitForKeyPress(true);
            }

        }

        private void DrawFeatures(List<Feature> featureList)
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            //Set default colour
            //rootConsole.ForegroundColor = featureColor;

            //Could consider storing here and sorting to give an accurate representation of multiple objects

            foreach (Feature feature in featureList)
            {
                //Don't draw features on other levels
                if (feature.LocationLevel != Game.Dungeon.Player.LocationLevel)
                    continue;

                //Colour depending on FOV (for development)
                MapSquare featureSquare = Game.Dungeon.Levels[feature.LocationLevel].mapSquares[feature.LocationMap.x, feature.LocationMap.y];

                Color featureColor = ColorPresets.White;

                if (featureSquare.InPlayerFOV)
                {
                    //In FOV
                    //rootConsole.ForegroundColor = inFOVTerrainColor;
                }
                else if (featureSquare.SeenByPlayer)
                {
                    //Not in FOV but seen
                    featureColor = Color.Interpolate(featureColor, ColorPresets.Black, 0.3);

                    //rootConsole.ForegroundColor = seenNotInFOVTerrainColor;
                }
                else
                {
                    //Never in FOV
                    if(debugMode)
                        featureColor = neverSeenFOVTerrainColor;
                    else
                        featureColor = hiddenColor;
                }

                rootConsole.ForegroundColor = featureColor;
                rootConsole.PutChar(mapTopLeft.x + feature.LocationMap.x, mapTopLeft.y + feature.LocationMap.y, feature.Representation);
            }

        }

        private void DrawCreatures(List<Monster> creatureList)
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            //Set default colour
            //rootConsole.ForegroundColor = creatureColor;

            foreach (Monster creature in creatureList)
            {
                //Not on this level
                if (creature.LocationLevel != Game.Dungeon.Player.LocationLevel)
                    continue;

                if (!creature.Alive)
                    continue;

                //Colour depending on FOV (for development)
                MapSquare creatureSquare = Game.Dungeon.Levels[creature.LocationLevel].mapSquares[creature.LocationMap.x, creature.LocationMap.y];
                Color creatureColor = creature.CreatureColor();

                bool drawCreature = true;

                if (creatureSquare.InPlayerFOV)
                {
                    //In FOV
                    //rootConsole.ForegroundColor = creature.CreatureColor();
                }
                else if (creatureSquare.SeenByPlayer)
                {
                    //Not in FOV but seen
                    if (!debugMode)
                        drawCreature = false;
                        //creatureColor = hiddenColor;
                }
                else
                {
                    //Never in FOV
                    if(!debugMode)
                        drawCreature = false;
                    
                }
                if (drawCreature)
                {
                    rootConsole.ForegroundColor = creatureColor;
                    //Set background depending on status
                    if (creature.Charmed)
                        rootConsole.BackgroundColor = charmBackground;
                    else if (creature.Passive)
                        rootConsole.BackgroundColor = passiveBackground;
                    else
                        rootConsole.BackgroundColor = normalBackground;

                    rootConsole.PutChar(mapTopLeft.x + creature.LocationMap.x, mapTopLeft.y + creature.LocationMap.y, creature.Representation);
                }
            }

            //Reset the background
            rootConsole.BackgroundColor = normalBackground;
        }

        public void DrawFOVDebug(int levelNo)
        {
            Map map = Game.Dungeon.Levels[levelNo];
            TCODFov fov = Game.Dungeon.FOVs[levelNo];

            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            //Clear screen
            rootConsole.Clear();

            for (int i = 0; i < map.width; i++)
            {
                for (int j = 0; j < map.height; j++)
                {
                    int screenX = mapTopLeft.x + i;
                    int screenY = mapTopLeft.y + j;

                    bool trans;
                    bool walkable;

                    fov.GetCell(i, j, out trans, out walkable);

                    Color drawColor = inFOVTerrainColor;

                    if (walkable)
                    {
                        drawColor = inFOVTerrainColor;
                    }
                    else
                    {
                        drawColor = inMonsterFOVTerrainColor;
                    }

                    rootConsole.ForegroundColor = drawColor;
                    char screenChar = StringEquivalent.TerrainChars[map.mapSquares[i, j].Terrain];
                    screenChar = '#';
                    rootConsole.PutChar(screenX, screenY, screenChar);

                    rootConsole.Flush();
                }
            }

        }

        //Draw a map only (useful for debugging)
        public void DrawMapDebug(Map map)
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            //Clear screen
            rootConsole.Clear();

            for (int i = 0; i < map.width; i++)
            {
                for (int j = 0; j < map.height; j++)
                {
                    int screenX = mapTopLeft.x + i;
                    int screenY = mapTopLeft.y + j;

                    char screenChar = StringEquivalent.TerrainChars[map.mapSquares[i, j].Terrain];

                    Color drawColor = inFOVTerrainColor;

                    if (!map.mapSquares[i, j].BlocksLight)
                    {
                        //In FOV
                        rootConsole.ForegroundColor = inFOVTerrainColor;
                    }
                    else
                    {
                        //Not in FOV but seen
                        rootConsole.ForegroundColor = seenNotInFOVTerrainColor;
                    }
                    rootConsole.PutChar(screenX, screenY, screenChar);
                }
            }
            
            //Flush the console
            rootConsole.Flush();
        }

        //Draw a map only (useful for debugging)
        public void DrawMapDebugHighlight(Map map, int x1, int y1, int x2, int y2)
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            //Clear screen
            rootConsole.Clear();

            for (int i = 0; i < map.width; i++)
            {
                for (int j = 0; j < map.height; j++)
                {
                    int screenX = mapTopLeft.x + i;
                    int screenY = mapTopLeft.y + j;

                    char screenChar = StringEquivalent.TerrainChars[map.mapSquares[i, j].Terrain];

                    Color drawColor = inFOVTerrainColor;

                    if (i == x1 && j == y1)
                    {
                        drawColor = ColorPresets.Red;
                    }

                    if (i == x2 && j == y2)
                    {
                        drawColor = ColorPresets.Red;
                    }
                    rootConsole.ForegroundColor = drawColor;
                    /*
                    if (!map.mapSquares[i, j].BlocksLight)
                    {
                        //In FOV
                        rootConsole.ForegroundColor = inFOVTerrainColor;
                    }
                    else
                    {
                        //Not in FOV but seen
                        rootConsole.ForegroundColor = seenNotInFOVTerrainColor;
                    }*/
                    rootConsole.PutChar(screenX, screenY, screenChar);
                }
            }

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

                    char screenChar;
                    Color drawColor;

                    //Exception for literals
                    if (map.mapSquares[i, j].Terrain == MapTerrain.Literal)
                    {
                        screenChar = map.mapSquares[i, j].terrainLiteral;
                        drawColor = literalColor;
                    }
                    else
                    {
                        screenChar = StringEquivalent.TerrainChars[map.mapSquares[i, j].Terrain];
                        drawColor = StringEquivalent.TerrainColors[map.mapSquares[i, j].Terrain];
                    }
                    
                   
                    if (map.mapSquares[i, j].InPlayerFOV || Game.Dungeon.Player.LocationLevel == 0)
                    {
                        //In FOV or in town
                        //rootConsole.ForegroundColor = drawColor;
                    }
                    else if (map.mapSquares[i, j].SeenByPlayer)
                    {
                        //Not in FOV but seen
                        drawColor = Color.Interpolate(drawColor, ColorPresets.Black, 0.5);

                        //rootConsole.ForegroundColor = seenNotInFOVTerrainColor;
                    }
                    else if (map.mapSquares[i, j].InMonsterFOV)
                    {
                        //Monster can see it
                        if (debugMode)
                            drawColor = inMonsterFOVTerrainColor;
                        else
                            drawColor = hiddenColor;
                    }
                    else
                    {
                        //Never in FOV
                        if (debugMode)
                            drawColor = Color.Interpolate(drawColor, ColorPresets.Black, 0.6);
                        else
                            drawColor = hiddenColor;
                    }
                    rootConsole.ForegroundColor = drawColor;
                    rootConsole.PutChar(screenX, screenY, screenChar);
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
        }

        /// <summary>
        /// Print message in message bar
        /// </summary>
        /// <param name="message"></param>
        internal void PrintMessage(string message)
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            //Update state
            lastMessage = message;

            //Clear message bar
            ClearMessageBar();

            //Display new message
            rootConsole.PrintLineRect(message, msgDisplayTopLeft.x, msgDisplayTopLeft.y, Width - msgDisplayTopLeft.x, msgDisplayNumLines, LineAlignment.Left);
        }

        /// <summary>
        /// Print message at any point on screen
        /// </summary>
        /// <param name="message"></param>
        internal void PrintMessage(string message, Point topLeft, int width)
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            //Update state
            lastMessage = message;

            //Clear message bar
            rootConsole.DrawRect(topLeft.x, topLeft.y, width, 1, true);

            //Display new message
            rootConsole.PrintLineRect(message, topLeft.x, topLeft.y, width, 1, LineAlignment.Left);
        }

        void ClearMessageBar()
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            rootConsole.DrawRect(msgDisplayTopLeft.x, msgDisplayTopLeft.y, Width - msgDisplayTopLeft.x, msgDisplayNumLines, true);
        }

        private void ResetOverlayScreens() {
            displayEquipment = false;
            displayEquipmentSelect = false;
            displayInventory = false;
            displaySpecialMoveMovies = false;
            displaySpells = false;
            displayTrainingUI = false;
        }

        public bool DisplayTrainingUI
        {
            set
            {
                if (value == true)
                {
                    ResetOverlayScreens();
                }

                displayTrainingUI = value;
            }
        }

        public bool DisplayInventory
        {
            set
            {
                if (value == true)
                {
                    ResetOverlayScreens();
                }
                
                displayInventory = value;
            }
        }

        public bool DisplayEquipment
        {
            set
            {
                if (value == true)
                {
                   ResetOverlayScreens();
                }

                displayEquipment = value;
            }
        }

        public bool DisplayEquipmentSelect
        {
            set
            {
                if (value == true)
                {
                    ResetOverlayScreens();
                }

                displayEquipmentSelect = value;
            }
        }

        public bool DisplaySpecialMoveMovies
        {
            set
            {
                if (value == true)
                {
                    ResetOverlayScreens();
                }
                displaySpecialMoveMovies = value;
            }
        }

        public bool DisplaySpells
        {
            set
            {
                if (value == true)
                {
                    ResetOverlayScreens();
                }
                displaySpells = value;
            }
        }

        public int SelectedInventoryIndex
        {
            set
            {
                selectedInventoryIndex = value;
            }
        }

        public int TopInventoryIndex
        {
            set
            {
                topInventoryIndex = value;
            }
        }

        public Inventory CurrentInventory
        {
            set
            {
                currentInventory = value;
            }
        }

        public List<EquipmentSlotInfo> CurrentEquipment
        {
            set
            {
                currentEquipment = value;
            }
        }

        /// <summary>
        /// String displayed at the top of the inventory
        /// </summary>
        public string InventoryTitle
        {
            set
            {
                inventoryTitle = value;
            }
        }

        /// <summary>
        /// String displayed at the bottom of the inventory
        /// </summary>
        public string InventoryInstructions
        {
            set
            {
                inventoryInstructions = value;
            }
        }

        /// <summary>
        /// Get a string from the user. Uses the message bar
        /// </summary>
        /// <returns></returns>
       
        internal string GetUserString(string introMessage)
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            ClearMessageLine();

            PrintMessage(introMessage + ": ");
            FlushConsole();

            bool continueInput = true;

            int maxChars = 40;

            string userString = "";

            do
            {
                //Get user input
                KeyPress userKey = Keyboard.WaitForKeyPress(true);

                //Each state has different keys

                        if (userKey.KeyCode == KeyCode.TCODK_CHAR)
                        {
                            char keyCode = (char)userKey.Character;
                            if (userString.Length < maxChars)
                            {
                                userString += keyCode.ToString();
                            }
                        }
                        else {
                            //Special keys
                            switch (userKey.KeyCode)
                            {
                                case KeyCode.TCODK_0:
                                    if (userString.Length < maxChars)
                                    {
                                        userString += "0";
                                    }
                                    break;
                                case KeyCode.TCODK_1:
                                    if (userString.Length < maxChars)
                                    {
                                        userString += "1";
                                    }
                                    break;
                                case KeyCode.TCODK_2:
                                    if (userString.Length < maxChars)
                                    {
                                        userString += "2";
                                    }
                                    break;
                                case KeyCode.TCODK_3:
                                    if (userString.Length < maxChars)
                                    {
                                        userString += "3";
                                    }
                                    break;
                                case KeyCode.TCODK_4:
                                    if (userString.Length < maxChars)
                                    {
                                        userString += "4";
                                    }
                                    break;
                                case KeyCode.TCODK_5:
                                    if (userString.Length < maxChars)
                                    {
                                        userString += "5";
                                    }
                                    break;
                                case KeyCode.TCODK_6:
                                    if (userString.Length < maxChars)
                                    {
                                        userString += "6";
                                    }
                                    break;
                                case KeyCode.TCODK_7:
                                    if (userString.Length < maxChars)
                                    {
                                        userString += "7";
                                    }
                                    break;
                                case KeyCode.TCODK_8:
                                    if (userString.Length < maxChars)
                                    {
                                        userString += "8";
                                    }
                                    break;
                                case KeyCode.TCODK_9:
                                    if (userString.Length < maxChars)
                                    {
                                        userString += "9";
                                    }
                                    break;
                                case KeyCode.TCODK_SPACE:
                                    if (userString.Length < maxChars)
                                    {
                                        userString += " ";
                                    }
                                    break;


                                case KeyCode.TCODK_ESCAPE:
                                    //Exit
                                    return null;
                                case KeyCode.TCODK_BACKSPACE:
                                    if (userString.Length != 0)
                                    {
                                        userString = userString.Substring(0, userString.Length - 1);
                                    }
                                    break;
                                case KeyCode.TCODK_ENTER:
                                case KeyCode.TCODK_KPENTER:
                                    //Exit with what we have
                                    return userString;
                            }
                        }

                        PrintMessage(introMessage + ": " + userString + "_");
                        FlushConsole();

            } while (continueInput);

            return null;
        }

        /// <summary>
        /// Get a string from the user. One line only.
        /// maxChars is the max length of the input string (not including the introMessage)
        /// </summary>
        /// <returns></returns>

        internal string GetUserString(string introMessage, Point topLeft, int maxChars)
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            ClearMessageLine();

            PrintMessage(introMessage + ": ", topLeft, introMessage.Length + 2 + maxChars);
            FlushConsole();

            bool continueInput = true;

            string userString = "";

            do
            {
                //Get user input
                KeyPress userKey = Keyboard.WaitForKeyPress(true);

                //Each state has different keys

                if (userKey.KeyCode == KeyCode.TCODK_CHAR)
                {
                    char keyCode = (char)userKey.Character;
                    if (userString.Length < maxChars)
                    {
                        userString += keyCode.ToString();
                    }
                }
                else
                {
                    //Special keys
                    switch (userKey.KeyCode)
                    {
                        case KeyCode.TCODK_0:
                            if (userString.Length < maxChars)
                            {
                                userString += "0";
                            }
                            break;
                        case KeyCode.TCODK_1:
                            if (userString.Length < maxChars)
                            {
                                userString += "1";
                            }
                            break;
                        case KeyCode.TCODK_2:
                            if (userString.Length < maxChars)
                            {
                                userString += "2";
                            }
                            break;
                        case KeyCode.TCODK_3:
                            if (userString.Length < maxChars)
                            {
                                userString += "3";
                            }
                            break;
                        case KeyCode.TCODK_4:
                            if (userString.Length < maxChars)
                            {
                                userString += "4";
                            }
                            break;
                        case KeyCode.TCODK_5:
                            if (userString.Length < maxChars)
                            {
                                userString += "5";
                            }
                            break;
                        case KeyCode.TCODK_6:
                            if (userString.Length < maxChars)
                            {
                                userString += "6";
                            }
                            break;
                        case KeyCode.TCODK_7:
                            if (userString.Length < maxChars)
                            {
                                userString += "7";
                            }
                            break;
                        case KeyCode.TCODK_8:
                            if (userString.Length < maxChars)
                            {
                                userString += "8";
                            }
                            break;
                        case KeyCode.TCODK_9:
                            if (userString.Length < maxChars)
                            {
                                userString += "9";
                            }
                            break;
                        case KeyCode.TCODK_SPACE:
                            if (userString.Length < maxChars)
                            {
                                userString += " ";
                            }
                            break;


                        case KeyCode.TCODK_ESCAPE:
                            //Exit
                            return null;
                        case KeyCode.TCODK_BACKSPACE:
                            if (userString.Length != 0)
                            {
                                userString = userString.Substring(0, userString.Length - 1);
                            }
                            break;
                        case KeyCode.TCODK_ENTER:
                            //Exit with what we have
                            return userString;
                    }
                }

                PrintMessage(introMessage + ": " + userString + "_", topLeft, introMessage.Length + 2 + maxChars);
                FlushConsole();

            } while (continueInput);

            return null;
        }

        internal bool YesNoQuestion(string introMessage, Point topLeft)
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            //ClearMessageLine();

            PrintMessage(introMessage + " (y / n):", topLeft, introMessage.Length + 8);
            FlushConsole();

            do
            {
                //Get user input
                KeyPress userKey = Keyboard.WaitForKeyPress(true);

                //Each state has different keys

                if (userKey.KeyCode == KeyCode.TCODK_CHAR)
                {

                   char keyCode = (char)userKey.Character;
                   switch (keyCode)
                   {
                       case 'y':
                           ClearMessageLine();
                           return true;
                           
                       case 'n':
                           ClearMessageLine();
                           return false;
                           
                   }
                }
            } while(true);
        }

        internal bool YesNoQuestion(string introMessage)
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            ClearMessageLine();

            PrintMessage(introMessage + " (y / n):");
            FlushConsole();

            do
            {
                //Get user input
                KeyPress userKey = Keyboard.WaitForKeyPress(true);

                //Each state has different keys

                if (userKey.KeyCode == KeyCode.TCODK_CHAR)
                {

                    char keyCode = (char)userKey.Character;
                    switch (keyCode)
                    {
                        case 'y':
                            ClearMessageLine();
                            return true;

                        case 'n':
                            ClearMessageLine();
                            return false;

                    }
                }
            } while (true);
        }

        internal GameDifficulty DifficultyQuestion(string introMessage, Point topLeft)
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            //ClearMessageLine();

            PrintMessage(introMessage + " (e / m / h)", topLeft, introMessage.Length + 14);
            FlushConsole();

            do
            {
                //Get user input
                KeyPress userKey = Keyboard.WaitForKeyPress(true);

                //Each state has different keys

                if (userKey.KeyCode == KeyCode.TCODK_CHAR)
                {

                   char keyCode = (char)userKey.Character;
                   switch (keyCode)
                   {
                       case 'e':
                           ClearMessageLine();
                           return GameDifficulty.Easy;
                           
                       case 'm':
                           ClearMessageLine();
                           return GameDifficulty.Medium;

                       case 'h':
                           ClearMessageLine();
                           return GameDifficulty.Hard;
                           
                   }
                }
            } while(true);
        }
    }
}
