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
        public enum TileLevel {
            Terrain = 0,
            Features = 1,
            Items = 2,
            CreatureDecoration = 3,
            Creatures = 4,
            Animations = 5,
            TargettingUI = 6
        }

        static Screen instance = null;

        //Console/screen size
        public int Width { get; set; }
        public int Height { get; set; }

        public bool DebugMode { get; set; }

        /// <summary>
        /// Show flashes on attacks and thrown projectiles
        /// </summary>
        public bool CombatAnimations { get; set; }

        public bool SetTargetInRange = false;

        //Top left coord to start drawing the map at
        //Set by DrawMap
        Point mapTopLeft;
        Point mapBotRight;

        Point mapTopLeftBase;
        Point mapBotRightBase;

        /// <summary>
        /// Dimensions of message display area
        /// </summary>
        Point msgDisplayTopLeft;
        Point msgDisplayBotRight;
        public int msgDisplayNumLines;

        Point statsDisplayTopLeft;
        Point statsDisplayBotRight;
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

       

        Point specialMoveStatusLine;
        Point spellStatusLine;
        Point trainStatsLine;

        Point calendarOffset;

        Color inFOVTerrainColor = ColorPresets.White;
        Color seenNotInFOVTerrainColor = ColorPresets.Gray;
        Color neverSeenFOVTerrainColor;
        Color inMonsterFOVTerrainColor = ColorPresets.Blue;


        Color statsColor = ColorPresets.White;
        Color nothingColor = ColorPresets.Gray;

        Color creatureColor = ColorPresets.White;
        Color itemColor = ColorPresets.Red ;
        Color featureColor = ColorPresets.White;

        Color hiddenColor = ColorPresets.Black;

        Color charmBackground = ColorPresets.DarkKhaki;
        Color passiveBackground = ColorPresets.DarkMagenta;
        Color uniqueBackground = ColorPresets.DarkCyan;
        Color inRangeBackground = ColorPresets.DeepSkyBlue;
        Color inRangeAndAggressiveBackground = ColorPresets.Purple;
        Color stunnedBackground = ColorPresets.DarkCyan;
        Color investigateBackground = ColorPresets.DarkGreen;
        Color pursuitBackground = ColorPresets.DarkRed;
        Color normalBackground = ColorPresets.Black;
        Color normalForeground = ColorPresets.White;
        Color targettedBackground = ColorPresets.DarkGray;

        Color frameColor = ColorPresets.Gray;

        Color targetBackground = ColorPresets.White;
        Color targetForeground = ColorPresets.Black;

        Color literalColor = ColorPresets.BurlyWood;
        Color literalTextColor = ColorPresets.White;

        Color headingColor = ColorPresets.Yellow;

        Color messageColor = ColorPresets.White;

        Color soundColor = ColorPresets.Yellow;

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

        //For examining
        public Monster CreatureToView { get; set; }
        public Item ItemToView { get; set; }

        bool displayInventory;

        const int missileDelay = 250;
        const int meleeDelay = 100;
        
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

        public int MsgLogWrapWidth { get; set; }

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

        public TargettingType TargetType { get; set; }

        public int TargetRange { get; set; }
        public double TargetPermissiveAngle { get; set; }

        //Current movie
        List <MovieFrame> movieFrames;

        public Color PCColor { get; set;}

        public bool SeeAllMonsters { get; set; }
        public bool SeeAllMap { get; set; }


        public uint MessageQueueWidth { get; private set; }

        public static Screen Instance
        {
            get
            {
                if (instance == null)
                    instance = new Screen();
                return instance;
            }
        }

        /// <summary>
        /// Master tile map for displaying the screen
        /// </summary>
        TileEngine.TileMap tileMap;

        Screen()
        {
            Width = 90;
            Height = 35;

            DebugMode = false;
            CombatAnimations = true;

            msgDisplayTopLeft = new Point(2, 1);
            msgDisplayBotRight = new Point(87, 3);

            MessageQueueWidth = (uint)(msgDisplayBotRight.y - msgDisplayBotRight.x);

            msgDisplayNumLines = 3;

            //Max 60 * 25 map

            mapTopLeftBase = new Point(2, 6);
            mapBotRightBase = new Point(61, 32);

            statsDisplayTopLeft = new Point(64, 6);
            statsDisplayBotRight = new Point(87, 32);
                      

            inventoryTL = new Point(5, 5);
            inventoryTR = new Point(85, 5);
            inventoryBL = new Point(5, 30);

            trainingTL = new Point(15, 10);
            trainingTR = new Point(75, 10);
            trainingBL = new Point(15, 25);

            MsgLogWrapWidth = inventoryTR.x - inventoryTL.x - 4;

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

            SeeAllMonsters = true;
            SeeAllMap = true;
        }

        //Setup the screen
        public void InitialSetup()
        {

            CustomFontRequest fontReq = new CustomFontRequest("tallfont.png", 8, 16, CustomFontRequestFontTypes.LayoutAsciiInColumn);
            RootConsole.Width = Width;
            RootConsole.Height = Height;
            RootConsole.WindowTitle = "FlatlineRL";
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
        /// Returns the points in a triangular target from origin to target
        /// </summary>
        /// <param name="location"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public List<Point> GetPointsForTriangularTarget(Point origin, Point target, int range, double fovAngle)
        {
            List<Point> triangularPoints = new List<Point>();

            double angle = DirectionUtil.AngleFromOriginToTarget(origin, target);

            for (int i = origin.x - range; i < origin.x + range; i++)
            {
                for (int j = origin.y - range; j < origin.y + range; j++)
                {
                    if (i >= 0 && i < this.Width && j >= 0 && j < this.Height)
                    {
                        if (CreatureFOV.TriangularFOV(origin, angle, range, i, j, fovAngle))
                        {
                            triangularPoints.Add(new Point(i, j));
                        }
                    }
                }
            }

            return triangularPoints;
        }

        /// <summary>
        /// Returns the points in a circular target
        /// </summary>
        /// <param name="location"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public List<Point> GetPointsForCircularTarget(Point location, int size)
        {
            List<Point> splashSquares = new List<Point>();

            for (int i = location.x - size; i < location.x + size; i++)
            {
                for (int j = location.y - size; j < location.y + size; j++)
                {
                    if (i >= 0 && i < Width && j >= 0 && j < Height)
                    {

                        if (Math.Pow(i - location.x, 2) + Math.Pow(j - location.y, 2) < Math.Pow(size, 2))
                        {
                            splashSquares.Add(new Point(i, j));
                        }
                    }
                }
            }

            return splashSquares;
        }

        /// <summary>
        /// ASCII line character for a direction
        /// </summary>
        /// <param name="deltaX"></param>
        /// <param name="deltaY"></param>
        /// <returns></returns>
        protected char LineChar(int deltaX, int deltaY) {

            char drawChar = '-';

            if (deltaX < 0 && deltaY < 0)
                drawChar = '\\';
            else if (deltaX < 0 && deltaY > 0)
                drawChar = '/';
            else if (deltaX > 0 && deltaY < 0)
                drawChar = '/';
            else if (deltaX > 0 && deltaY > 0)
                drawChar = '\\';
            else if (deltaY == 0)
                drawChar = '-';
            else if (deltaX == 0)
                drawChar = '|';

            return drawChar;
        }

        /// <summary>
        /// Draw a line following a path on a tile layer.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="color"></param>
        protected void DrawPathLine(TileLevel layerNo, Point start, Point end, Color foregroundColor, Color backgroundColor)
        {
            DrawPathLine(layerNo, start, end, foregroundColor, backgroundColor, (char)0);
        }

        /// <summary>
        /// Draw a line following a path on a tile layer. Override default line drawing
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="color"></param>
        protected void DrawPathLine(TileLevel layerNo, Point start, Point end, Color foregroundColor, Color backgroundColor, char drawChar)
        {
            //Draw the line overlay

            //Cast a line between the start and end

            int lastX = start.x;
            int lastY = start.y;

            foreach(Point p in Utility.GetPointsOnLine(start.x, start.y, end.x, end.y)) {

                //Don't draw the first char (where the player is)
                if(p == start)
                    continue;

                char c;
                if (drawChar == 0)
                    c = LineChar(p.x - lastX, p.y - lastY);
                else
                    c = drawChar;

                lastX = p.x;
                lastY = p.y;

                tileMapLayer(layerNo).Rows[p.y].Columns[p.x] = new TileEngine.TileCell(c);
                tileMapLayer(layerNo).Rows[p.y].Columns[p.x].TileFlag = new LibtcodColorFlags(foregroundColor, backgroundColor);
            }           
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
        /// Get the text from a movie
        /// </summary>
        /// <param name="movieRoot"></param>
        /// <returns></returns>
        public List<string> GetMovieText(string movieRoot)
        {
            bool loadSuccess = Screen.Instance.LoadMovie(movieRoot);

            if (!loadSuccess)
            {
                LogFile.Log.LogEntryDebug("Failed to load movie file: " + movieRoot, LogDebugLevel.High);
                return new List<string>();
            }

            List<string> outputText = new List<string>();

            //Concatenate the movie into a string list
            foreach (MovieFrame frame in movieFrames)
            {
                if (outputText.Count > 0)
                    outputText.Add("\n");

                outputText.AddRange(frame.scanLines);
            }

            return outputText;

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
                bool loadSuccess = LoadMovie(filenameRoot);

                if (!loadSuccess)
                {
                    LogFile.Log.LogEntryDebug("Failed to load movie file: " + filenameRoot, LogDebugLevel.High);
                    return;
                }

                
                
                int frameNo = 0;

                //Draw each frame of the movie
                foreach (MovieFrame frame in movieFrames)
                {
                    //Flatline - centre on each frame
                    int width = frame.width;
                    int height = frame.height;

                    int xOffset = (movieWidth - width) / 2;
                    int yOffset = (movieHeight - height) / 2;

                    Point frameTL = new Point(movieTL.x + xOffset, movieTL.y + yOffset);

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

                UpdateNoMsgQueue();
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

                if (userKey.KeyCode == KeyCode.TCODK_ENTER
                    || userKey.KeyCode == KeyCode.TCODK_KPENTER)
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

                    //Reset flash color at the end of the line
                    rootConsole.ForegroundColor = normalMovieColor;
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

        public bool LoadMovie(string filenameRoot)
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

                return true;
            }
            catch (Exception e)
            {
                LogFile.Log.LogEntry("Failed to load movie: " + e.Message);
                return false;
            }
        }

        /// <summary>
        /// Draws and updates the screen. Doesn't run message queue. Is this function really used? I think all the calls don't
        /// </summary>
        public void UpdateNoMsgQueue()
        {
            Screen.Instance.Draw();
            Screen.Instance.FlushConsole();
        }

        private void DrawPC(Player player)
        {
         
            Point PClocation = player.LocationMap;
            Color PCDrawColor = PCColor;

            if (DebugMode)
            {
                MapSquare pcSquare = Game.Dungeon.Levels[player.LocationLevel].mapSquares[player.LocationMap.x, player.LocationMap.y];

                if (pcSquare.InMonsterFOV)
                {
                    PCDrawColor = Color.Interpolate(PCDrawColor, ColorPresets.Red, 0.4);
                }
            }

            tileMapLayer(TileLevel.Creatures).Rows[player.LocationMap.y].Columns[player.LocationMap.x] = new TileEngine.TileCell(player.Representation);
            tileMapLayer(TileLevel.Creatures).Rows[player.LocationMap.y].Columns[player.LocationMap.x].TileFlag = new LibtcodColorFlags(PCDrawColor);
        }


        /// <summary>
        /// Fully rebuild the layered, tiled map. All levels, excluding animations
        /// </summary>
        private void BuildTiledMap()
        {
            Dungeon dungeon = Game.Dungeon;
            Player player = dungeon.Player;

            tileMap = new TileEngine.TileMap(7, dungeon.PCMap.height, dungeon.PCMap.width);

            //Draw the map screen

            //Draw terrain (must be done first since sets some params)
            //First level in tileMap
            DrawMap(dungeon.PCMap);

            //Draw fixed features
            DrawFeatures(dungeon.Features);

            //Draw items (will appear on top of staircases etc.)
            DrawItems(dungeon.Items);

            //Draw creatures
            DrawCreatures(dungeon.Monsters);

            //Draw PC
            DrawPC(player);

            //Draw targetting cursor
            if (targettingMode)
                DrawTargettingCursor();

        }

        //Draw the current dungeon map and objects
        private void Draw()
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            Dungeon dungeon = Game.Dungeon;
            Player player = dungeon.Player;

            //Clear screen
            rootConsole.Clear();

            //Build a tile map to display the screen
            //In future, we probably don't want to pull this down each time

            //Either use a dirty tile system, or simply have a flag to not change typical levels
            //E.g. an animation only changes anim, targetting only changes targetting

            //Build the full tiled map representation
            BuildTiledMap();
            
            //Render tiled map to screen
            MapRendererLibTCod.RenderMap(tileMap, new Point(0, 0), new System.Drawing.Rectangle(mapTopLeft.x, mapTopLeft.y, mapBotRightBase.x - mapTopLeftBase.x + 1, mapBotRightBase.y - mapTopLeftBase.y + 1));

            rootConsole.ForegroundColor = ColorPresets.White;
            rootConsole.BackgroundColor = ColorPresets.Black;

            //Draw Stats
            DrawStats(dungeon.Player);

            if (ShowMsgHistory)
                DrawMsgHistory();

        }

        /// <summary>
        /// Draws an animated attack. This a top level function which is used instead of Draw() as an entry to screen
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="target"></param>
        /// <param name="size"></param>
        public void DrawAreaAttackAnimation(List <Point> targetSquares, Color color)
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            rootConsole.ForegroundColor = color;
            rootConsole.BackgroundColor = ColorPresets.Black;

            //Clone the list since we mangle it
            List<Point> mangledPoints = new List<Point>();
            foreach (Point p in targetSquares)
            {
                mangledPoints.Add(new Point(p));
            }

            //Don't rebuild the static map (items, creatures etc.) since it hasn't changed
            
            //Clear targetting
            tileMap.ClearLayer((int)TileLevel.TargettingUI);

            //Add animation points into the animation layer

            foreach (Point p in mangledPoints)
            {
                tileMapLayer(TileLevel.Animations).Rows[p.y].Columns[p.x] = new TileEngine.TileCell('*');
                tileMapLayer(TileLevel.Animations).Rows[p.y].Columns[p.x].TileFlag = new LibtcodColorFlags(color, ColorPresets.Black);
            }

            //Render the full layered map (with these animations) on screen
            MapRendererLibTCod.RenderMap(tileMap, new Point(0, 0), new System.Drawing.Rectangle(mapTopLeft.x, mapTopLeft.y, mapBotRightBase.x - mapTopLeftBase.x + 1, mapBotRightBase.y - mapTopLeftBase.y + 1));
            FlushConsole();

            //Wait
            TCODSystem.Sleep(missileDelay);

            //Wipe the animation layer
            tileMap.ClearLayer((int)TileLevel.Animations);

            //Draw again without animations
            MapRendererLibTCod.RenderMap(tileMap, new Point(0, 0), new System.Drawing.Rectangle(mapTopLeft.x, mapTopLeft.y, mapBotRightBase.x - mapTopLeftBase.x + 1, mapBotRightBase.y - mapTopLeftBase.y + 1));
            FlushConsole();
        }


        private void DrawTargettingCursor()
        {
            Player player = Game.Dungeon.Player;

            //Draw the area of effect

            switch (TargetType)
            {
                case TargettingType.Line:

                    //Draw a line up to the target
                    DrawPathLine(TileLevel.TargettingUI, new Point(player.LocationMap.x, player.LocationMap.y), new Point(Target.x, Target.y), targetForeground, targetBackground);
                    //Should improve the getlinesquare function to give nicer output so we could use it here too

                    break;

                case TargettingType.LineThrough:

                    //Cast a line which terminates on the edge of the map
                    Point projectedLine = Game.Dungeon.GetEndOfLine(player.LocationMap, Target, player.LocationLevel);

                    //Get the in-FOV points up to that end point
                    TCODFov currentFOV2 = Game.Dungeon.CalculateAbstractFOV(Game.Dungeon.Player.LocationLevel, Game.Dungeon.Player.LocationMap, 80);
                    List<Point> lineSquares = Game.Dungeon.GetPathLinePointsInFOV(Game.Dungeon.Player.LocationMap, projectedLine, currentFOV2);

                    DrawExplosionOverSquaresAndCreatures(lineSquares);      

                    break;
                    
                case TargettingType.Rocket:
                    {
                        //Get potention explosion points
                        int size = 2;

                        List<Point> splashSquares = GetPointsForCircularTarget(Target, size);

                        //Draw a line up to the target square
                        DrawPathLine(TileLevel.TargettingUI, new Point(player.LocationMap.x, player.LocationMap.y), new Point(Target.x, Target.y), targetForeground, targetBackground);

                        DrawExplosionOverSquaresAndCreatures(splashSquares); 

                    }
                    break;

                case TargettingType.Shotgun:
                    {
                        int size = TargetRange;
                        double spreadAngle = TargetPermissiveAngle;

                        CreatureFOV currentFOV = Game.Dungeon.CalculateCreatureFOV(Game.Dungeon.Player);
                        List<Point> splashSquares = currentFOV.GetPointsForTriangularTargetInFOV(player.LocationMap, Target, Game.Dungeon.Levels[player.LocationLevel], size, spreadAngle);

                        DrawExplosionOverSquaresAndCreatures(splashSquares);
                    }
                    break;
            }

            //Highlight target if in range
            
            Color backgroundColor = targetBackground;
            Color foregroundColor = targetForeground;

            if (SetTargetInRange)
            {
                backgroundColor = ColorPresets.White;
            }

            char toDraw = '.';
            int monsterIdInSquare = tileMapLayer(TileLevel.Creatures).Rows[Target.y].Columns[Target.x].TileID;

            if (monsterIdInSquare != -1)
                toDraw = (char)monsterIdInSquare;

            tileMapLayer(TileLevel.TargettingUI).Rows[Target.y].Columns[Target.x] = new TileEngine.TileCell(toDraw);
            tileMapLayer(TileLevel.TargettingUI).Rows[Target.y].Columns[Target.x].TileFlag = new LibtcodColorFlags(ColorPresets.Red, backgroundColor);
            
        }

        private void DrawExplosionOverSquaresAndCreatures(List<Point> splashSquares)
        {
            //Draw each point as targetted
            foreach (Point p in splashSquares)
            {
                //If there's a monster in the square, draw it in red in the animation layer. Otherwise, draw an explosion
                char toDraw = '*';
                int monsterIdInSquare = tileMapLayer(TileLevel.Creatures).Rows[p.y].Columns[p.x].TileID;

                if (monsterIdInSquare != -1)
                    toDraw = (char)monsterIdInSquare;

                tileMapLayer(TileLevel.TargettingUI).Rows[p.y].Columns[p.x] = new TileEngine.TileCell(toDraw);
                tileMapLayer(TileLevel.TargettingUI).Rows[p.y].Columns[p.x].TileFlag = new LibtcodColorFlags(ColorPresets.Red);
            }
        }


        /// <summary>
        /// Screen for end of game info
        /// </summary>
        public void DrawEndOfGameInfo(List<string> stuffToDisplay)
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            //Clear screen
            rootConsole.Clear();

            //Draw frame
            rootConsole.DrawFrame(DeathTL.x, DeathTL.y, DeathWidth, DeathHeight, true);

            //Draw title
            rootConsole.PrintLineRect("End of game summary", DeathTL.x + DeathWidth / 2, DeathTL.y, DeathWidth, 1, LineAlignment.Center);

            //Draw preamble
            int count = 0;
            foreach (string s in stuffToDisplay)
            {
                rootConsole.PrintLineRect(s, DeathTL.x + 2, DeathTL.y + 2 + count, DeathWidth - 4, 1, LineAlignment.Left);
                count++;
            }

            //Draw instructions

            rootConsole.PrintLineRect("Press ENTER to continue...", DeathTL.x + DeathWidth / 2, DeathTL.y + DeathHeight - 1, DeathWidth, 1, LineAlignment.Center);
            Screen.Instance.FlushConsole();

            WaitForEnterKey();
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
         
            //Clear screen
            ClearScreen();

            //Draw frame
            DrawFrame(DeathTL.x, DeathTL.y, DeathWidth, DeathHeight, true);

            //Draw title
            PrintLineRect("VICTORY!", DeathTL.x + DeathWidth / 2, DeathTL.y, DeathWidth, 1, LineAlignment.Center);

            //Draw preamble
            int count = 0;
            foreach (string s in DeathPreamble)
            {
                PrintLineRect(s, DeathTL.x + 2, DeathTL.y + 2 + count, DeathWidth - 4, 1, LineAlignment.Left);
                count++;
            }

            //Draw kills

            PrintLineRect("Total Kills", DeathTL.x + DeathWidth / 2, DeathTL.y + 2 + count + 2, DeathWidth, 1, LineAlignment.Center);

            foreach (string s in TotalKills)
            {
                PrintLineRect(s, DeathTL.x + 2, DeathTL.y + 2 + count + 4, DeathWidth - 4, 1, LineAlignment.Left);
                count++;
            }

            //Draw instructions

            PrintLineRect("Press any key to exit...", DeathTL.x + DeathWidth / 2, DeathTL.y + DeathHeight - 1, DeathWidth, 1, LineAlignment.Center);
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
            rootConsole.PrintLine("Speed:", statsBoxTL.x + statTitleOffset.x, statsBoxTL.y + statTitleOffset.y + 3, LineAlignment.Left);
            rootConsole.PrintLine("Charm:", statsBoxTL.x + statTitleOffset.x, statsBoxTL.y + statTitleOffset.y + 4, LineAlignment.Left);
            rootConsole.PrintLine("Magic Skill:", statsBoxTL.x + statTitleOffset.x, statsBoxTL.y + statTitleOffset.y + 5, LineAlignment.Left);

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
        /// Display movie screen overlay
        /// </summary>
        private void DrawMovieOverlay()
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            //Draw frame - same as inventory
            rootConsole.DrawFrame(inventoryTL.x, inventoryTL.y, inventoryTR.x - inventoryTL.x + 1, inventoryBL.y - inventoryTL.y + 1, true);

            //Draw title
            rootConsole.PrintLineRect("Special moves and spells known", (inventoryTL.x + inventoryTR.x) / 2, inventoryTL.y, inventoryTR.x - inventoryTL.x, 1, LineAlignment.Center);

            //Draw instructions
            rootConsole.PrintLineRect("Select move to replay movie or (x) to exit", (inventoryTL.x + inventoryTR.x) / 2, inventoryBL.y, inventoryTR.x - inventoryTL.x, 1, LineAlignment.Center);

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

            List<Spell> knownSpells = new List<Spell>();

            foreach (Spell move in Game.Dungeon.Spells)
            {

                //Run out of room - won't happen as written
                if (moveIndex == inventoryListH)
                    break;

                //Don't list unknown moves
                if (!move.Known)
                    continue;

                knownSpells.Add(move);

                char selectionChar = (char)((int)'a' + moveIndex);
                string entryString = "(" + selectionChar.ToString() + ") " + move.SpellName(); //+" (equipped)";

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


        public bool ShowMsgHistory { get; set; }

        enum Direction { up, down, none };

        void ClearScreen()
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            rootConsole.Clear();
        }

        /// <summary>
        /// Draws a frame on the screen
        /// </summary>
        void DrawFrame(int x, int y, int width, int height, bool clear)
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            //Draw frame - same as inventory
            rootConsole.DrawFrame(x, y, width, height, clear);
        }

        /// <summary>
        /// Draws a frame on the screen in a particular color
        /// </summary>
        void DrawFrame(int x, int y, int width, int height, bool clear, Color color)
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            rootConsole.ForegroundColor = color;

            //Draw frame - same as inventory
            rootConsole.DrawFrame(x, y, width, height, clear);

            rootConsole.ForegroundColor = ColorPresets.White;
        }

        /// <summary>
        /// Character-based drawing. Kept only for stats etc. in transitional period. All map stuff now works in the tile layer
        /// </summary>
        void PutChar(int x, int y, char c, Color color)
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();
            rootConsole.ForegroundColor = color;

            rootConsole.PutChar(x, y, c);

            rootConsole.ForegroundColor = ColorPresets.White;
        }

        /// <summary>
        /// Print a string in a rectangle
        /// </summary>
        void PrintLineRect(string msg, int x, int y, int width, int height, LineAlignment alignment)
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            rootConsole.PrintLineRect(msg, x, y, width, height, alignment);
        }

        /// <summary>
        /// Print a string at a location
        /// </summary>
        void PrintLine(string msg, int x, int y, LineAlignment alignment)
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            rootConsole.PrintLine(msg, x, y, alignment);
        }

        /// <summary>
        /// Print a string at a location
        /// </summary>
        void PrintLine(string msg, int x, int y, LineAlignment alignment, Color color)
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();
            rootConsole.ForegroundColor = color;

            rootConsole.PrintLine(msg, x, y, alignment);
            rootConsole.ForegroundColor = ColorPresets.White;
        }

        /// <summary>
        /// Draw rectangle
        /// </summary>
        void DrawRect(int x, int y, int width, int height, bool clear)
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            rootConsole.DrawRect(x, y, width, height, clear);
        }

        /// <summary>
        /// Draw the msg history and allow the player to scroll
        /// </summary>
        private void DrawMsgHistory()
        {
            //Draw frame - same as inventory
            DrawFrame(inventoryTL.x, inventoryTL.y, inventoryTR.x - inventoryTL.x + 1, inventoryBL.y - inventoryTL.y + 1, true);

            //Draw title
            PrintLineRect("Message History", (inventoryTL.x + inventoryTR.x) / 2, inventoryTL.y, inventoryTR.x - inventoryTL.x, 1, LineAlignment.Center);

            //Draw instructions
            PrintLineRect("Press (up) or (down) to scroll or (x) to exit", (inventoryTL.x + inventoryTR.x) / 2, inventoryBL.y, inventoryTR.x - inventoryTL.x, 1, LineAlignment.Center);

            //Active area is slightly reduced from frame
            int inventoryListX = inventoryTL.x + 2;
            int inventoryListW = inventoryTR.x - inventoryTL.x - 4;
            int inventoryListY = inventoryTL.y + 2;
            int inventoryListH = inventoryBL.y - inventoryTL.y - 4;

            LinkedList<string> msgHistory = Game.MessageQueue.messageHistory;

            //Display list
            LinkedListNode<string> displayedMsg;
            LinkedListNode<string> topLineDisplayed = null;
            
            LinkedListNode<string> bottomTopLineDisplayed = msgHistory.Last;

            if (msgHistory.Count > 0)
            {
                //Find the line at the top of the screen when the list is fully scrolled down
                for (int i = 0; i < inventoryListH - 1; i++)
                {
                    if (bottomTopLineDisplayed.Previous != null)
                        bottomTopLineDisplayed = bottomTopLineDisplayed.Previous;
                }
                topLineDisplayed = bottomTopLineDisplayed;

                //Display the message log
                displayedMsg = topLineDisplayed;
                for (int i = 0; i < inventoryListH; i++)
                {
                    PrintLineRect(displayedMsg.Value, inventoryListX, inventoryListY + i, inventoryListW, 1, LineAlignment.Left);
                    displayedMsg = displayedMsg.Next;
                    if (displayedMsg == null)
                        break;
                }
            }

            Screen.Instance.FlushConsole();

            bool keepLooping = true;

            do
            {
                //Get user input
                KeyPress userKey = Keyboard.WaitForKeyPress(true);
                Direction dir = Direction.none;

                //Each state has different keys

                if (userKey.KeyCode == KeyCode.TCODK_CHAR)
                {
                    char keyCode = (char)userKey.Character;

                    if (keyCode == 'x')
                        keepLooping = false;

                    if (keyCode == 'j')
                    {
                        dir = Direction.up;
                    }

                    if (keyCode == 'k')
                    {
                        dir = Direction.down;
                    }
                }

                else
                {
                    //Special keys
                    switch (userKey.KeyCode)
                    {
                        case KeyCode.TCODK_UP:
                        case KeyCode.TCODK_KP8:
                            dir = Direction.up;
                            break;

                        case KeyCode.TCODK_KP2:
                        case KeyCode.TCODK_DOWN:
                            dir = Direction.down;
                            break;
                    }
                }

                if (msgHistory.Count > 0)
                {
                    if (dir == Direction.up)
                    {
                        if (topLineDisplayed.Previous != null)
                            topLineDisplayed = topLineDisplayed.Previous;
                    }
                    else if (dir == Direction.down)
                    {
                        if (topLineDisplayed != bottomTopLineDisplayed)
                            topLineDisplayed = topLineDisplayed.Next;
                    }

                    //Clear the rectangle
                    DrawRect(inventoryTL.x + 1, inventoryTL.y + 1, inventoryTR.x - inventoryTL.x - 1, inventoryBL.y - inventoryTL.y - 1, true);

                    //Display the message log
                    displayedMsg = topLineDisplayed;
                    for (int i = 0; i < inventoryListH; i++)
                    {
                        PrintLineRect(displayedMsg.Value, inventoryListX, inventoryListY + i, inventoryListW, 1, LineAlignment.Left);
                        displayedMsg = displayedMsg.Next;
                        if (displayedMsg == null)
                            break;
                    }
                }
                Screen.Instance.FlushConsole();

            } while (keepLooping);
        }


        private void DrawStats(Player player)
        {

            //Blank stats area
            //rootConsole.DrawRect(statsDisplayTopLeft.x, statsDisplayTopLeft.y, Width - statsDisplayTopLeft.x, Height - statsDisplayTopLeft.y, true);
            DrawFrame(statsDisplayTopLeft.x, statsDisplayTopLeft.y - 1, statsDisplayBotRight.x - statsDisplayTopLeft.x + 2, statsDisplayBotRight.y - statsDisplayTopLeft.y + 3, false, frameColor);

            //Mission
            Point missionOffset = new Point(4, 1);
            hitpointsOffset = new Point(4, 4);
            Point weaponOffset = new Point(4, 6);
            Point utilityOffset = new Point(4, 11);
            Point viewOffset = new Point(4, 19);
            Point gameDataOffset = new Point(4, 24);

            PrintLine("ZONE: " + (player.LocationLevel + 1).ToString("00"), statsDisplayTopLeft.x + missionOffset.x, statsDisplayTopLeft.y + missionOffset.y, LineAlignment.Left);
            PrintLine(DungeonInfo.LookupMissionName(player.LocationLevel), statsDisplayTopLeft.x + missionOffset.x, statsDisplayTopLeft.y + missionOffset.y + 1, LineAlignment.Left);
            
            
            //Draw HP Status

            int hpBarLength = 10;
            double playerHPRatio = player.Hitpoints / (double)player.MaxHitpoints;
            int hpBarEntries = (int)Math.Ceiling(hpBarLength * playerHPRatio);
            
            //It's easy for the player - make sure we're exact
            hpBarEntries = player.Hitpoints;

            PrintLine("HP: ", statsDisplayTopLeft.x + hitpointsOffset.x, statsDisplayTopLeft.y + hitpointsOffset.y, LineAlignment.Left);

            for (int i = 0; i < hpBarLength; i++)
            {
                if (i < hpBarEntries)
                {
                    PutChar(statsDisplayTopLeft.x + hitpointsOffset.x + 5 + i, statsDisplayTopLeft.y + hitpointsOffset.y, '*', ColorPresets.Green);
                }
                else
                {
                    PutChar(statsDisplayTopLeft.x + hitpointsOffset.x + 5 + i, statsDisplayTopLeft.y + hitpointsOffset.y, '*', ColorPresets.Gray);
                }
            }

            //Draw equipped weapon

            Item weapon = Game.Dungeon.Player.GetEquippedWeaponAsItem();

            string weaponStr = "Weapon: ";

            PrintLine(weaponStr, statsDisplayTopLeft.x + weaponOffset.x, statsDisplayTopLeft.y + weaponOffset.y, LineAlignment.Left);

            if (weapon != null)
            {
                IEquippableItem weaponE = weapon as IEquippableItem;
                
                weaponStr = weapon.SingleItemDescription;
                PrintLine(weaponStr, statsDisplayTopLeft.x + weaponOffset.x, statsDisplayTopLeft.y + weaponOffset.y + 1, LineAlignment.Left, weapon.GetColour());

                //Ammo
                if (weaponE.HasFireAction())
                {
                    PrintLine("Am: ", statsDisplayTopLeft.x + weaponOffset.x, statsDisplayTopLeft.y + weaponOffset.y + 2, LineAlignment.Left);
        
                    //TODO infinite ammo?
                    int ammoBarLength = 10;
                    double weaponAmmoRatio = weaponE.RemainingAmmo() / (double) weaponE.MaxAmmo();
                    int ammoBarEntries = (int)Math.Ceiling(ammoBarLength * weaponAmmoRatio);

                    for (int i = 0; i < ammoBarLength; i++)
                    {
                        if (i < ammoBarEntries)
                        {
                            PutChar(statsDisplayTopLeft.x + weaponOffset.x + 5 + i, statsDisplayTopLeft.y + weaponOffset.y + 2, '*', ColorPresets.Blue);
                        }
                        else
                        {
                            PutChar(statsDisplayTopLeft.x + weaponOffset.x + 5 + i, statsDisplayTopLeft.y + weaponOffset.y + 2, '*', ColorPresets.Gray);
                        }
                    }
                }

                //Uses

                string uses = "";
                if (weaponE.HasMeleeAction())
                {
                    uses += "melee ";
                }

                if (weaponE.HasFireAction())
                {
                    uses += "(f)ire ";
                }

                if (weaponE.HasThrowAction())
                {
                    uses += "(t)hrow ";
                }

                if (weaponE.HasOperateAction())
                {
                    uses += "(u)se";
                }

                PrintLine(uses, statsDisplayTopLeft.x + weaponOffset.x, statsDisplayTopLeft.y + weaponOffset.y + 3, LineAlignment.Left);
            }
            else
            {
                weaponStr = "None";
                PrintLine(weaponStr, statsDisplayTopLeft.x + weaponOffset.x, statsDisplayTopLeft.y + weaponOffset.y + 1, LineAlignment.Left, nothingColor);
            }
            

            //Draw equipped utility

            Item utility = Game.Dungeon.Player.GetEquippedUtilityAsItem();

            string utilityStr = "Utility: ";
            PrintLine(utilityStr, statsDisplayTopLeft.x + utilityOffset.x, statsDisplayTopLeft.y + utilityOffset.y, LineAlignment.Left);

            if (utility != null)
            {
                utilityStr = utility.SingleItemDescription;
                PrintLine(utilityStr, statsDisplayTopLeft.x + utilityOffset.x, statsDisplayTopLeft.y + utilityOffset.y + 1, LineAlignment.Left, utility.GetColour());

                IEquippableItem utilityE = utility as IEquippableItem;

                string uses = "";
                
                if (utilityE.HasOperateAction())
                {
                    uses += "(U)se";
                }

                if (utilityE.HasThrowAction())
                {
                    uses += "(T)hrow ";
                }

                PrintLine(uses, statsDisplayTopLeft.x + utilityOffset.x, statsDisplayTopLeft.y + utilityOffset.y + 2, LineAlignment.Left);
            }
             
            else
            {
                utilityStr = "Nothing";
                PrintLine(utilityStr, statsDisplayTopLeft.x + utilityOffset.x, statsDisplayTopLeft.y + utilityOffset.y + 1, LineAlignment.Left, nothingColor);
            }

            //Effect active (add ors)
            if (player.effects.Count > 0)
            {
                PlayerEffect thisEffect = player.effects[0];

                if(thisEffect is PlayerEffectSimpleDuration) {

                    PlayerEffectSimpleDuration durationEffect = thisEffect as PlayerEffectSimpleDuration;

                    string effectName = thisEffect.GetName();
                    int effectRemainingDuration = durationEffect.GetRemainingDuration();
                    int effectTotalDuration = durationEffect.GetDuration();
                    Color effectColor = thisEffect.GetColor();

                    //Effect name

                    PrintLine("Effect: ", statsDisplayTopLeft.x + utilityOffset.x, statsDisplayTopLeft.y + utilityOffset.y + 3, LineAlignment.Left);

                    PrintLine(effectName, statsDisplayTopLeft.x + utilityOffset.x, statsDisplayTopLeft.y + utilityOffset.y + 4, LineAlignment.Left, effectColor);
                    //Duration

                    PrintLine("Tm: ", statsDisplayTopLeft.x + utilityOffset.x, statsDisplayTopLeft.y + utilityOffset.y + 5, LineAlignment.Left);

                    int ammoBarLength = 10;
                    double weaponAmmoRatio = effectRemainingDuration / (double) effectTotalDuration;
                    int ammoBarEntries = (int)Math.Ceiling(ammoBarLength * weaponAmmoRatio);

                    for (int i = 0; i < ammoBarLength; i++)
                    {
                        if (i < ammoBarEntries)
                        {
                            PutChar(statsDisplayTopLeft.x + utilityOffset.x + 5 + i, statsDisplayTopLeft.y + utilityOffset.y + 5, '*', ColorPresets.Gold);
                        }
                        else
                        {
                            PutChar(statsDisplayTopLeft.x + utilityOffset.x + 5 + i, statsDisplayTopLeft.y + utilityOffset.y + 5, '*', ColorPresets.Gray);
                        }
                    }
                }

            }

            //Draw what we can see
            
            //Creature takes precidence


            string viewStr = "Target: ";
            PrintLine(viewStr, statsDisplayTopLeft.x + viewOffset.x, statsDisplayTopLeft.y + viewOffset.y, LineAlignment.Left);

            if (CreatureToView != null && CreatureToView.Alive == true)
            {
                String nameStr = CreatureToView.SingleDescription;// +"(" + CreatureToView.Representation + ")";
                PrintLine(nameStr, statsDisplayTopLeft.x + viewOffset.x, statsDisplayTopLeft.y + viewOffset.y, LineAlignment.Left);

                //Monster hp

                int mhpBarLength = 10;
                double mplayerHPRatio = CreatureToView.Hitpoints / (double)CreatureToView.MaxHitpoints;
                int mhpBarEntries = (int)Math.Ceiling(mhpBarLength * mplayerHPRatio);

                PrintLine("HP: ", statsDisplayTopLeft.x + viewOffset.x, statsDisplayTopLeft.y + viewOffset.y + 2, LineAlignment.Left);

                for (int i = 0; i < mhpBarLength; i++)
                {
                    if (i < mhpBarEntries)
                    {
                        PutChar(statsDisplayTopLeft.x + viewOffset.x + 5 + i, statsDisplayTopLeft.y + viewOffset.y + 2, '*', ColorPresets.Red);
                    }
                    else
                    {
                        PutChar(statsDisplayTopLeft.x + viewOffset.x + 5 + i, statsDisplayTopLeft.y + viewOffset.y + 2, '*', ColorPresets.Gray);
                    }
                }
                

                //Behaviour

                if (CreatureToView.StunnedTurns > 0)
                {
                    PrintLine("(Stunned: " + CreatureToView.StunnedTurns + ")", statsDisplayTopLeft.x + viewOffset.x, statsDisplayTopLeft.y + viewOffset.y + 3, LineAlignment.Left, stunnedBackground);
                }
                else if (CreatureToView.InPursuit())
                {
                    PrintLine("(In Pursuit)", statsDisplayTopLeft.x + viewOffset.x, statsDisplayTopLeft.y + viewOffset.y + 3, LineAlignment.Left, pursuitBackground);
                }
                else if (!CreatureToView.OnPatrol())
                {
                    PrintLine("(Investigating)", statsDisplayTopLeft.x + viewOffset.x, statsDisplayTopLeft.y + viewOffset.y + 3, LineAlignment.Left, investigateBackground);
                }
                else {
                    PrintLine("(Neutral)", statsDisplayTopLeft.x + viewOffset.x, statsDisplayTopLeft.y + viewOffset.y + 3, LineAlignment.Left);
                }
            }
            else if (ItemToView != null)
            {
                String nameStr = ItemToView.SingleItemDescription;// +"(" + ItemToView.Representation + ")";
                PrintLine(nameStr, statsDisplayTopLeft.x + viewOffset.x, statsDisplayTopLeft.y + viewOffset.y + 1, LineAlignment.Left, ItemToView.GetColour());

                IEquippableItem itemE = ItemToView as IEquippableItem;
                if (itemE != null)
                {
                    EquipmentSlot weaponSlot = itemE.EquipmentSlots.Find(x => x == EquipmentSlot.Weapon);
                    if(weaponSlot != null) {
                        PrintLine("(Weapon)", statsDisplayTopLeft.x + viewOffset.x, statsDisplayTopLeft.y + viewOffset.y + 2, LineAlignment.Left);
                    }
                    else
                        PrintLine("(Utility)", statsDisplayTopLeft.x + viewOffset.x, statsDisplayTopLeft.y + viewOffset.y + 2, LineAlignment.Left);
                }
            }
            else
            {
                utilityStr = "None";
                PrintLine(utilityStr, statsDisplayTopLeft.x + viewOffset.x, statsDisplayTopLeft.y + viewOffset.y + 1, LineAlignment.Left, nothingColor);
            }

            //Game data
            PrintLine("Droids:", statsDisplayTopLeft.x + gameDataOffset.x, statsDisplayTopLeft.y + gameDataOffset.y, LineAlignment.Left);

            int noDroids = Game.Dungeon.DungeonInfo.MaxDeaths - Game.Dungeon.DungeonInfo.NoDeaths;

            for (int i = 0; i < noDroids; i++)
            {
                PutChar(statsDisplayTopLeft.x + gameDataOffset.x + 8 + i, statsDisplayTopLeft.y + gameDataOffset.y, Game.Dungeon.Player.Representation, Game.Dungeon.Player.RepresentationColor());
            }

            PrintLine("Aborts:", statsDisplayTopLeft.x + gameDataOffset.x, statsDisplayTopLeft.y + gameDataOffset.y + 1, LineAlignment.Left);

            int noAborts = Game.Dungeon.DungeonInfo.MaxAborts - Game.Dungeon.DungeonInfo.NoAborts;

            for (int i = 0; i < noAborts; i++)
            {
                PutChar(statsDisplayTopLeft.x + gameDataOffset.x + 8 + i, statsDisplayTopLeft.y + gameDataOffset.y + 1, 'X',ColorPresets.Red);
            }
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

                IEquippableItem equipItem = item as IEquippableItem;
                if (equipItem != null)
                {
                    //Show no ammo items in a neutral colour
                    if (equipItem.HasFireAction() && equipItem.RemainingAmmo() == 0)
                        itemColorToUse = ColorPresets.Gray;
                }

                //Color itemColorToUse = itemColor;

                bool drawItem = true;

                if (itemSquare.InPlayerFOV || SeeAllMap)
                {
                   
                }
                else if (itemSquare.SeenByPlayerThisRun)
                {
                    //Not in FOV now but seen this adventure
                    //Don't draw items in squares seen in previous adventures (since the items have respawned)
                    itemColorToUse = Color.Interpolate(item.GetColour(), ColorPresets.Black, 0.5);
                }
                else
                {
                    //Never in FOV
                    if (DebugMode)
                    {
                        itemColorToUse = itemColor;
                    }
                    else
                    {
                        //Can't see it, don't draw it
                        drawItem = false;
                    }
                }

                if (drawItem)
                {
                    tileMapLayer(TileLevel.Items).Rows[item.LocationMap.y].Columns[item.LocationMap.x] = new TileEngine.TileCell(item.Representation);
                    tileMapLayer(TileLevel.Items).Rows[item.LocationMap.y].Columns[item.LocationMap.x].TileFlag = new LibtcodColorFlags(itemColorToUse);
                }
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

                Color featureColor = feature.RepresentationColor();

                bool drawFeature = true;

                if (featureSquare.InPlayerFOV || SeeAllMap)
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
                    if (DebugMode)
                    {
                        featureColor = neverSeenFOVTerrainColor;
                    }
                    else
                    {
                        //Used to be draw it in black. This is no different but nicer.
                        drawFeature = false;
                    }
                }

                if (drawFeature)
                {
                    tileMapLayer(TileLevel.Features).Rows[feature.LocationMap.y].Columns[feature.LocationMap.x] = new TileEngine.TileCell(feature.Representation);
                    tileMapLayer(TileLevel.Features).Rows[feature.LocationMap.y].Columns[feature.LocationMap.x].TileFlag = new LibtcodColorFlags(featureColor);
                }
            }

        }

        private void DrawCreatures(List<Monster> creatureList)
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            //Set default colour
            //rootConsole.ForegroundColor = creatureColor;

            //Draw stuff about creatures which should be overwritten by other creatures
            foreach (Monster creature in creatureList)
            {
                if (creature.LocationLevel != Game.Dungeon.Player.LocationLevel)
                    continue;

                if (!creature.Alive)
                    continue;

                int monsterX = mapTopLeft.x + creature.LocationMap.x;
                int monsterY = mapTopLeft.y + creature.LocationMap.y;

                Color creatureColor = creature.RepresentationColor();
                rootConsole.ForegroundColor = creatureColor;

                MapSquare creatureSquare = Game.Dungeon.Levels[creature.LocationLevel].mapSquares[creature.LocationMap.x, creature.LocationMap.y];
                bool drawCreature = true;

                if (creatureSquare.InPlayerFOV)
                {
                    //In FOV
                    //rootConsole.ForegroundColor = creature.CreatureColor();
                }
                else if (creatureSquare.SeenByPlayer)
                {
                    //Not in FOV but seen
                    if (!DebugMode)
                        drawCreature = false;
                    //creatureColor = hiddenColor;
                }
                else
                {
                    //Never in FOV
                    if (!DebugMode)
                        drawCreature = false;

                }

                //In many levels in FlatlineRL, we can see all the monsters
                if (SeeAllMonsters)
                {
                    drawCreature = true;
                }

                if (!drawCreature)
                    continue;

                //Heading

                if (creature.ShowHeading())
                {
                    List<Point> headingMarkers;

                    if (creature.FOVType() == CreatureFOV.CreatureFOVType.Triangular)
                    {
                        headingMarkers = DirectionUtil.SurroundingPointsFromDirection(creature.Heading, creature.LocationMap, 3);
                    }
                    else
                    {
                        //Base
                        headingMarkers = DirectionUtil.SurroundingPointsFromDirection(creature.Heading, creature.LocationMap, 1);
                        
                        //Reverse first one
                        Point oppositeMarker = new Point(creature.LocationMap.x - (headingMarkers[0].x - creature.LocationMap.x), creature.LocationMap.y - (headingMarkers[0].y - creature.LocationMap.y));
                        headingMarkers.Add(oppositeMarker);
                    }

                    foreach (Point headingLoc in headingMarkers)
                    {
                        //Check heading is drawable

                        Map map = Game.Dungeon.Levels[creature.LocationLevel];

                        //LogFile.Log.LogEntryDebug("heading: " + creature.Representation + " loc: x: " + headingLoc.x.ToString() + " y: " + headingLoc.y.ToString(), LogDebugLevel.Low);

                        if (headingLoc.x >= 0 && headingLoc.x < map.width
                            && headingLoc.y >= 0 && headingLoc.y < map.height)// && Game.Dungeon.MapSquareIsWalkable(creature.LocationLevel, new Point(headingLoc.x, headingLoc.y))
                        {
                            //Draw as a colouring on terrain

                            int terrainChar = tileMapLayer(TileLevel.Terrain).Rows[headingLoc.y].Columns[headingLoc.x].TileID;

                            if (terrainChar != -1)
                            {
                                char charToOverwrite = (char)terrainChar;
                                //Dot is too hard to see
                                if (charToOverwrite == '.')
                                    charToOverwrite = '\x9';

                                tileMapLayer(TileLevel.CreatureDecoration).Rows[headingLoc.y].Columns[headingLoc.x] = new TileEngine.TileCell(charToOverwrite);
                                tileMapLayer(TileLevel.CreatureDecoration).Rows[headingLoc.y].Columns[headingLoc.x].TileFlag = new LibtcodColorFlags(creature.RepresentationColor());
                            }
                        }
                    }
                }
            }

            foreach (Monster creature in creatureList)
            {
                //Not on this level
                if (creature.LocationLevel != Game.Dungeon.Player.LocationLevel)
                    continue;

                if (!creature.Alive)
                    continue;

                //Colour depending on FOV (for development)
                MapSquare creatureSquare = Game.Dungeon.Levels[creature.LocationLevel].mapSquares[creature.LocationMap.x, creature.LocationMap.y];
                Color creatureColor = creature.RepresentationColor();

                Color foregroundColor;
                Color backgroundColor;

                //Shouldn't really do this here but see if we can get away with it
                CreatureFOV currentFOV = Game.Dungeon.CalculateCreatureFOV(Game.Dungeon.Player);

                bool drawCreature = true;

                if (creatureSquare.InPlayerFOV)
                {
                    //In FOV
                    //rootConsole.ForegroundColor = creature.CreatureColor();
                }
                else if (creatureSquare.SeenByPlayer)
                {
                    //Not in FOV but seen
                    if (!DebugMode)
                        drawCreature = false;
                    //creatureColor = hiddenColor;
                }
                else
                {
                    //Never in FOV
                    if (!DebugMode)
                        drawCreature = false;

                }

                //In many levels in FlatlineRL, we can see all the monsters
                if (SeeAllMonsters)
                {
                    drawCreature = true;
                }


                if (DebugMode)
                {
                    if (creatureSquare.InMonsterFOV)
                    {
                        creatureColor = Color.Interpolate(creatureColor, ColorPresets.Red, 0.4);
                    }

                    //Draw waypoints
                    MonsterFightAndRunAI monsterWithWP = creature as MonsterFightAndRunAI;
                    if (monsterWithWP != null &&
                        monsterWithWP.Waypoints.Count > 0)
                    {
                        int wpNo = 0;
                        foreach (Point wp in monsterWithWP.Waypoints)
                        {
                            int wpX = mapTopLeft.x + wp.x;
                            int wpY = mapTopLeft.y + wp.y;

                            rootConsole.PutChar(wpX, wpY, wpNo.ToString()[0]);
                            wpNo++;
                        }
                    }
                }

                if (drawCreature)
                {
                    foregroundColor = creatureColor;
                    backgroundColor = ColorPresets.Black;

                    bool newBackground = false;
                    //Set background depending on status
                    if (creature.Charmed)
                    {
                        backgroundColor = charmBackground;
                        newBackground = true;
                    }
                    else if (creature.Passive)
                    {
                        backgroundColor = passiveBackground;
                        newBackground = true;
                    }
                    else if (creature == CreatureToView)
                    {
                        //targetted
                        backgroundColor = targettedBackground;
                        newBackground = true;
                    }
                    else if (creature.StunnedTurns > 0)
                    {
                        backgroundColor = stunnedBackground;
                        newBackground = true;
                    }

                    if (newBackground == false)
                    {

                        IEquippableItem weapon = Game.Dungeon.Player.GetEquippedWeapon();

                        if (weapon != null)
                        {

                            //In range firing
                            if (weapon.HasFireAction() && Dungeon.TestRangeFOVForWeapon(Game.Dungeon.Player, creature, weapon.RangeFire(), currentFOV))
                            {
                                backgroundColor = inRangeBackground;
                                newBackground = true;
                            }
                            else
                            {
                                //In throwing range
                                if (weapon.HasThrowAction() && Dungeon.TestRangeFOVForWeapon(Game.Dungeon.Player, creature, weapon.RangeFire(), currentFOV))
                                {
                                    backgroundColor = inRangeBackground;
                                    newBackground = true;
                                }
                            }

                            //Also agressive
                            if (newBackground == true && creature.InPursuit())
                            {
                                backgroundColor = inRangeAndAggressiveBackground;
                            }
                        }
                    }

                    if (newBackground == false)
                    {
                        if (creature.InPursuit())
                        {
                            backgroundColor = pursuitBackground;
                            newBackground = true;
                        }
                        else if (!creature.OnPatrol())
                        {
                            backgroundColor = investigateBackground;
                            newBackground = true;
                        }
                        else if (creature.Unique)
                            backgroundColor = uniqueBackground;
                        else
                            backgroundColor = normalBackground;
                    }

                    //Creature
                    int monsterX = mapTopLeft.x + creature.LocationMap.x;
                    int monsterY = mapTopLeft.y + creature.LocationMap.y;

                    rootConsole.PutChar(monsterX, monsterY, creature.Representation);

                    tileMapLayer(TileLevel.Creatures).Rows[creature.LocationMap.y].Columns[creature.LocationMap.x] = new TileEngine.TileCell(creature.Representation);
                    tileMapLayer(TileLevel.Creatures).Rows[creature.LocationMap.y].Columns[creature.LocationMap.x].TileFlag = new LibtcodColorFlags(foregroundColor, backgroundColor);

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
        public void SaveCurrentLevelToDisk()
        {
            Map map = Game.Dungeon.Levels[Game.Dungeon.Player.LocationLevel];

            StreamWriter outFile = new StreamWriter("outfile.txt");

            for (int i = 0; i < map.height; i++)
            {
                StringBuilder sb = new StringBuilder();
                for (int j = 0; j < map.width; j++)
                {
                    sb.Append(StringEquivalent.TerrainChars[map.mapSquares[j, i].Terrain]);

                }
                outFile.WriteLine(sb.ToString());
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

            //Calculate where to draw the map

            int width = map.width;
            int height = map.height;

            int widthAvail = mapBotRightBase.x - mapTopLeftBase.x;
            int heightAvail = mapBotRightBase.y - mapTopLeftBase.y;

            //Draw frame
            rootConsole.ForegroundColor = frameColor;
            rootConsole.DrawFrame(mapTopLeftBase.x - 1, mapTopLeftBase.y - 1, widthAvail + 3, heightAvail + 3, false);

            //Draw frame for msg here too
            rootConsole.DrawFrame(msgDisplayTopLeft.x - 1, msgDisplayTopLeft.y - 1, msgDisplayBotRight.x - msgDisplayTopLeft.x + 3, msgDisplayBotRight.y - msgDisplayTopLeft.y + 3, false);

            //Put the map in the centre
            mapTopLeft = new Point(mapTopLeftBase.x + (widthAvail - width) / 2, mapTopLeftBase.y + (heightAvail - height) / 2);

            for (int i = 0; i < map.width; i++)
            {
                for (int j = 0; j < map.height; j++)
                {
                    int screenX = mapTopLeft.x + i;
                    int screenY = mapTopLeft.y + j;

                    char screenChar;
                    Color baseDrawColor;
                    Color drawColor;

                    //Exception for literals
                    if (map.mapSquares[i, j].Terrain == MapTerrain.Literal)
                    {
                        screenChar = map.mapSquares[i, j].terrainLiteral;
                        if (screenChar >= 'A' && screenChar <= 'Z')
                            baseDrawColor = literalTextColor;
                        else if (screenChar >= 'a' && screenChar <= 'z')
                            baseDrawColor = literalTextColor;
                        else
                            baseDrawColor = literalColor;
                    }
                    else
                    {
                        screenChar = StringEquivalent.TerrainChars[map.mapSquares[i, j].Terrain];
                        baseDrawColor = StringEquivalent.TerrainColors[map.mapSquares[i, j].Terrain];
                    }

                    //In FlatlineRL you can normally see the whole map
                    if (map.mapSquares[i, j].InPlayerFOV || SeeAllMap)
                    {
                        //In FOV or in town
                        drawColor = baseDrawColor;
                    }
                    else if (map.mapSquares[i, j].SeenByPlayer)
                    {
                        //Not in FOV but seen
                        drawColor = Color.Interpolate(baseDrawColor, ColorPresets.Black, 0.4);

                        //rootConsole.ForegroundColor = seenNotInFOVTerrainColor;
                    }
                    else
                    {
                        //Never in FOV
                        if (DebugMode)
                            drawColor = Color.Interpolate(baseDrawColor, ColorPresets.Black, 0.6);
                        else
                            drawColor = hiddenColor;
                    }

                    //Monster FOV in debug mode
                    if (DebugMode)
                    {
                        //Draw player FOV explicitally
                        if (map.mapSquares[i, j].InPlayerFOV)
                        {
                            drawColor = Color.Interpolate(baseDrawColor, ColorPresets.Blue, 0.6);
                        }


                        //Draw monster FOV
                        if (map.mapSquares[i, j].InMonsterFOV)
                        {
                            drawColor = Color.Interpolate(baseDrawColor, ColorPresets.Red, 0.6);
                        }

                        //Draw sounds
                        if (map.mapSquares[i, j].SoundMag > 0.0001)
                        {
                            drawColor = Color.Interpolate(baseDrawColor, ColorPresets.Yellow, map.mapSquares[i, j].SoundMag);
                        }
                    }

                    if (Game.Dungeon.Player.IsEffectActive(typeof(PlayerEffects.SeeFOV)) && map.mapSquares[i, j].InMonsterFOV)
                    {
                        drawColor = Color.Interpolate(baseDrawColor, ColorPresets.Green, 0.7);
                    }

                    tileMapLayer(TileLevel.Terrain).Rows[j].Columns[i] = new TileEngine.TileCell(screenChar);
                    tileMapLayer(TileLevel.Terrain).Rows[j].Columns[i].TileFlag = new LibtcodColorFlags(drawColor);

                    //rootConsole.ForegroundColor = drawColor;
                    //rootConsole.PutChar(screenX, screenY, screenChar);
                }
            }

        }

        /// <summary>
        /// Returns the requested tile layer for the master map
        /// </summary>
        /// <param name="levelId"></param>
        /// <returns></returns>
        internal TileEngine.TileLayer tileMapLayer(TileLevel levelId)
        {
            return tileMap.Layer[(int)levelId];
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
            PrintMessage(message, messageColor);
        }

        /// <summary>
        /// Print message in message bar
        /// </summary>
        /// <param name="message"></param>
        internal void PrintMessage(string message, Color color)
        {
            //Get screen handle
            RootConsole rootConsole = RootConsole.GetInstance();

            //Update state
            lastMessage = message;

            //Clear message bar
            //ClearMessageBar();

            //Draw frame
            //Done earlier

            //Display new message
            rootConsole.ForegroundColor = color;
            rootConsole.PrintLineRect(message, msgDisplayTopLeft.x, msgDisplayTopLeft.y, msgDisplayBotRight.x - msgDisplayTopLeft.x + 1, msgDisplayNumLines, LineAlignment.Left);
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

            rootConsole.DrawRect(msgDisplayTopLeft.x, msgDisplayTopLeft.y, msgDisplayBotRight.x - msgDisplayTopLeft.x - 1, msgDisplayBotRight.y - msgDisplayTopLeft.y - 1, true);
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

            PrintMessage(introMessage + "", topLeft, introMessage.Length + 2 + maxChars);
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

                PrintMessage(introMessage + "" + userString + "_", topLeft, introMessage.Length + 2 + maxChars);
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

        /// <summary>
        /// Draw the screen and run the message queue
        /// </summary>
        public void Update()
        {
            //Draw screen 
            Draw();

            //Message queue - requires keyboard to advance messages - not sure about this yet
            Game.MessageQueue.RunMessageQueue();
        }



        /// <summary>
        /// Do a missile attack animation. creature firing from start to finish in color.
        /// Currently checks if the target and origin creature are in FOV but not the path itself
        /// Currently only used for MvP attacks
        /// </summary>
        /// <param name="LocationMap"></param>
        /// <param name="point"></param>
        /// <param name="point_3"></param>
        /// <param name="color"></param>
        internal void DrawMissileAttack(Creature originCreature, Creature target, CombatResults result, Color color)
        {
            if (!CombatAnimations)
                return;

            //Check that the player can see the action

            MapSquare creatureSquare = Game.Dungeon.Levels[originCreature.LocationLevel].mapSquares[originCreature.LocationMap.x, originCreature.LocationMap.y];
            MapSquare targetSquare = Game.Dungeon.Levels[target.LocationLevel].mapSquares[target.LocationMap.x, target.LocationMap.y];

            if (!creatureSquare.InPlayerFOV && !targetSquare.InPlayerFOV)
                return;
            
            //Draw the screen as normal
            Draw();
            FlushConsole();

            //Flash the attacker
            /*
            if (creatureSquare.InPlayerFOV)
            {
                rootConsole.ForegroundColor = ColorPresets.White;
                rootConsole.PutChar(mapTopLeft.x + originCreature.LocationMap.x, mapTopLeft.y + originCreature.LocationMap.y, originCreature.Representation);
            }*/

            //Draw animation to animation layer

            //Calculate and draw the line overlay
            DrawPathLine(TileLevel.Animations, originCreature.LocationMap, target.LocationMap, color, ColorPresets.Black);

            //Flash the target if they were damaged
            //Draw them in either case so that we overwrite the missile animation on the target square with the creature

            if (targetSquare.InPlayerFOV)
            {
                Color colorToDraw = ColorPresets.Red;

                if (result == CombatResults.DefenderDamaged || result == CombatResults.DefenderDied)
                {
                    
                }
                else
                {
                    colorToDraw = target.RepresentationColor();
                }

                tileMapLayer(TileLevel.Animations).Rows[target.LocationMap.y].Columns[target.LocationMap.x] = new TileEngine.TileCell(target.Representation);
                tileMapLayer(TileLevel.Animations).Rows[target.LocationMap.y].Columns[target.LocationMap.x].TileFlag = new LibtcodColorFlags(colorToDraw);
            }

            //Render the full layered map (with these animations) on screen
            MapRendererLibTCod.RenderMap(tileMap, new Point(0, 0), new System.Drawing.Rectangle(mapTopLeft.x, mapTopLeft.y, mapBotRightBase.x - mapTopLeftBase.x + 1, mapBotRightBase.y - mapTopLeftBase.y + 1));
            FlushConsole();

            //Wait
            TCODSystem.Sleep(missileDelay);

            //Wipe the animation layer
            tileMap.ClearLayer((int)TileLevel.Animations);

            //Draw the map normally
            MapRendererLibTCod.RenderMap(tileMap, new Point(0, 0), new System.Drawing.Rectangle(mapTopLeft.x, mapTopLeft.y, mapBotRightBase.x - mapTopLeftBase.x + 1, mapBotRightBase.y - mapTopLeftBase.y + 1));
            FlushConsole();  
        }

        /// <summary>
        /// Do a melee attack animation
        /// </summary>
        /// <param name="monsterFightAndRunAI"></param>
        /// <param name="newTarget"></param>
        internal void DrawMeleeAttack(Creature creature, Creature newTarget, CombatResults result)
        {
            if (!CombatAnimations)
                return;

            //Check that the player can see the action

            MapSquare creatureSquare = Game.Dungeon.Levels[creature.LocationLevel].mapSquares[creature.LocationMap.x, creature.LocationMap.y];
            MapSquare targetSquare = Game.Dungeon.Levels[newTarget.LocationLevel].mapSquares[newTarget.LocationMap.x, newTarget.LocationMap.y];

            if (!creatureSquare.InPlayerFOV && !targetSquare.InPlayerFOV)
                return;

            //Draw screen normally
            //Necessary since on a player move, his old position will show unless we do this
            Draw();
            FlushConsole();

            //Flash the attacker
            /*
            Color creatureColor = creature.RepresentationColor();

            if (creatureSquare.InPlayerFOV)
            {
                rootConsole.ForegroundColor = ColorPresets.White;
                rootConsole.PutChar(mapTopLeft.x + creature.LocationMap.x, mapTopLeft.y + creature.LocationMap.y, creature.Representation);
            }
            */

            //Flash the attacked creature
            //Add flash to animation layer

            if (targetSquare.InPlayerFOV)
            {
                if (result == CombatResults.DefenderDamaged || result == CombatResults.DefenderDied)
                {
                    tileMapLayer(TileLevel.Animations).Rows[newTarget.LocationMap.y].Columns[newTarget.LocationMap.x] = new TileEngine.TileCell('*');
                    tileMapLayer(TileLevel.Animations).Rows[newTarget.LocationMap.y].Columns[newTarget.LocationMap.x].TileFlag = new LibtcodColorFlags(ColorPresets.Red);
                }
            }

            //Render the full layered map (with these animations) on screen
            MapRendererLibTCod.RenderMap(tileMap, new Point(0, 0), new System.Drawing.Rectangle(mapTopLeft.x, mapTopLeft.y, mapBotRightBase.x - mapTopLeftBase.x + 1, mapBotRightBase.y - mapTopLeftBase.y + 1));
            FlushConsole();

            //Wait
            TCODSystem.Sleep(meleeDelay);

            //Wipe the animation layer
            tileMap.ClearLayer((int)TileLevel.Animations);
                        
            //Draw the map normally
            MapRendererLibTCod.RenderMap(tileMap, new Point(0, 0), new System.Drawing.Rectangle(mapTopLeft.x, mapTopLeft.y, mapBotRightBase.x - mapTopLeftBase.x + 1, mapBotRightBase.y - mapTopLeftBase.y + 1));
            FlushConsole();
        }
    }
}
