using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace SpaceInvaderConsoleGame
{
    class Program
    {
        static int consoleWidth = 40;
        static int consoleHeight = 20;
        static int playerX = consoleWidth / 2;
        static int playerY = consoleHeight - 2;
        static int playerWidth = 3; // Width of the player ship "<A>"
        static List<EnemyX> enemiesX = new List<EnemyX>();
        static List<EnemyY> enemiesY = new List<EnemyY>();
        static List<int[]> bullets = new List<int[]>();
        static List<int[]> enemyBullets = new List<int[]>(); // List for enemy bullets
        static List<Star> stars = new List<Star>();
        static Random random = new Random();
        static DateTime startTime;
        static int score = 0; // Score variable
        static int enemyMoveCounter = 0;
        static int enemyMoveInterval = 10; // Increase this value to make enemies move slower | base 20
        static int enemySpawnCounter = 0;
        static int enemySpawnInterval = 50; // Adjust this value to control the frequency of enemy spawns
        static int enemyShootCounter = 0;
        static int enemyShootInterval = 20; // Interval for enemy shooting | 30 : 3s |20 : 2s
        static int playerLives = 3; // Player lives
        static int enemyYMoveCounter = 0; // Counter for enemy Y lateral movement
        static int enemyYMoveInterval = 10; // Interval for enemy Y lateral movement

        static void Main(string[] args)
        {
            Console.SetWindowSize(consoleWidth, consoleHeight + 2);
            Console.SetBufferSize(consoleWidth, consoleHeight + 2);
            Console.CursorVisible = false;
            InitStars();
            startTime = DateTime.Now;

            while (true)
            {
                if ((DateTime.Now - startTime).TotalMinutes >= 2)
                {
                    EndGame("TIME'S UP! YOU WIN!");
                    break;
                }

                UpdateStars();
                Draw();
                Input();
                Logic();
                Thread.Sleep(100);
            }
        }

        static void InitStars()
        {
            stars.Clear();
            for (int i = 0; i < 50; i++)
            {
                stars.Add(new Star(random.Next(consoleWidth), random.Next(consoleHeight), GetRandomStarType()));
            }
        }

        static void UpdateStars()
        {
            foreach (var star in stars)
            {
                star.Y++;
                if (star.Y >= consoleHeight)
                {
                    star.Y = 0;
                    star.X = random.Next(consoleWidth);
                    star.Type = GetRandomStarType();
                }
            }
        }

        static char GetRandomStarType()
        {
            int rand = random.Next(100);
            if (rand < 85)
                return '.';
            else if (rand < 95)
                return '*';
            else
                return 'o';
        }

        static void Draw()
        {
            StringBuilder buffer = new StringBuilder();

            for (int y = 0; y < consoleHeight; y++)
            {
                for (int x = 0; x < consoleWidth; x++)
                {
                    buffer.Append(' ');
                }
                buffer.AppendLine();
            }

            foreach (var star in stars)
            {
                buffer[(star.Y * (consoleWidth + 2)) + star.X] = star.Type;
            }

            Console.SetCursorPosition(0, 0);
            Console.Write(buffer.ToString());

            // Draw player
            Console.SetCursorPosition(playerX, playerY);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("<A>");

            // Draw enemies X
            Console.ForegroundColor = ConsoleColor.Red;
            foreach (var enemyX in enemiesX)
            {
                if (enemyX.X >= 0 && enemyX.X < consoleWidth && enemyX.Y >= 0 && enemyX.Y < consoleHeight)
                {
                    Console.SetCursorPosition(enemyX.X, enemyX.Y);
                    Console.Write('X');
                }
            }

            // Draw enemies Y
            Console.ForegroundColor = ConsoleColor.Magenta;
            foreach (var enemyY in enemiesY)
            {
                if (enemyY.X >= 0 && enemyY.X < consoleWidth && enemyY.Y >= 0 && enemyY.Y < consoleHeight)
                {
                    Console.SetCursorPosition(enemyY.X, enemyY.Y);
                    Console.Write('Y');
                }
            }

            // Draw bullets
            Console.ForegroundColor = ConsoleColor.Cyan;
            foreach (var bullet in bullets)
            {
                if (bullet[1] >= 0 && bullet[1] < consoleHeight)
                {
                    Console.SetCursorPosition(bullet[0], bullet[1]);
                    Console.Write('|');
                }
            }

            // Draw enemy bullets
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            foreach (var bullet in enemyBullets)
            {
                if (bullet[1] >= 0 && bullet[1] < consoleHeight)
                {
                    Console.SetCursorPosition(bullet[0], bullet[1]);
                    Console.Write('|');
                }
            }

            // Draw UI
            Console.SetCursorPosition(0, consoleHeight);
            Console.ForegroundColor = ConsoleColor.White;
            string timeLeft = $"Time left: {120 - (int)(DateTime.Now - startTime).TotalSeconds}s";
            Console.Write(timeLeft.PadRight(consoleWidth));

            // Draw score
            Console.SetCursorPosition(0, consoleHeight + 1);
            string scoreText = $"Score: {score}";
            Console.Write(scoreText.PadRight(consoleWidth / 2));

            // Draw player lives
            string livesText = $"Lives: [{new string('♥', playerLives)}]";
            Console.SetCursorPosition(consoleWidth - livesText.Length, consoleHeight + 1);
            Console.Write(livesText);
        }


        static void Input()
        {
            if (Console.KeyAvailable)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);

                switch (keyInfo.Key)
                {
                    case ConsoleKey.LeftArrow:
                        if (playerX > 0)
                            playerX--;
                        break;

                    case ConsoleKey.RightArrow:
                        if (playerX < consoleWidth - playerWidth)
                            playerX++;
                        break;

                    case ConsoleKey.UpArrow:
                        if (playerY > 0)
                            playerY--;
                        break;

                    case ConsoleKey.DownArrow:
                        if (playerY < consoleHeight - 1)
                            playerY++;
                        break;

                    case ConsoleKey.Spacebar:
                        bullets.Add(new int[] { playerX + 1, playerY - 1 }); // Adjust bullet position
                        break;
                }
            }
        }

        static void Logic()
        {
            // Move bullets
            for (int i = bullets.Count - 1; i >= 0; i--)
            {
                bullets[i][1]--;

                if (bullets[i][1] < 0)
                {
                    bullets.RemoveAt(i);
                }
                else
                {
                    // Check collision with enemies X
                    for (int j = enemiesX.Count - 1; j >= 0; j--)
                    {
                        if (bullets[i][0] == enemiesX[j].X && bullets[i][1] == enemiesX[j].Y)
                        {
                            enemiesX.RemoveAt(j);
                            bullets.RemoveAt(i);
                            score += 10; // Increase score by 10 for each enemy destroyed
                            break;
                        }
                    }

                    // Check collision with enemies Y
                    for (int j = enemiesY.Count - 1; j >= 0; j--)
                    {
                        if (j < enemiesY.Count && bullets[i][0] == enemiesY[j].X && bullets[i][1] == enemiesY[j].Y)
                        {
                            enemiesY.RemoveAt(j);
                            bullets.RemoveAt(i);
                            score += 10; // Increase score by 10 for each enemy destroyed
                            break;
                        }
                    }
                }
            }


            // Move enemy bullets
            for (int i = enemyBullets.Count - 1; i >= 0; i--)
            {
                enemyBullets[i][1]++;

                if (enemyBullets[i][1] >= consoleHeight)
                {
                    enemyBullets.RemoveAt(i);
                }
                else
                {
                    // Check collision with player
                    if (enemyBullets[i][0] >= playerX && enemyBullets[i][0] < playerX + playerWidth && enemyBullets[i][1] == playerY)
                    {
                        enemyBullets.RemoveAt(i);
                        playerLives--; // Decrease player's lives
                        if (playerLives <= 0)
                        {
                            EndGame("GAME OVER");
                            return;
                        }
                    }
                }
            }

            // Move enemies X
            enemyMoveCounter++;
            if (enemyMoveCounter >= enemyMoveInterval)
            {
                enemyMoveCounter = 0;
                for (int i = enemiesX.Count - 1; i >= 0; i--)
                {
                    enemiesX[i].Y++;
                    if (enemiesX[i].Y >= consoleHeight)
                    {
                        // Remove enemy when it reaches bottom
                        enemiesX.RemoveAt(i);
                        // Decrease score by 20 when an enemy reaches bottom
                        score -= 20;
                        // Check if player still has lives
                        if (playerLives <= 0)
                        {
                            EndGame("GAME OVER");
                            return;
                        }
                    }

                    // Check collision with player
                    if (enemiesX[i].X >= playerX && enemiesX[i].X < playerX + playerWidth && enemiesX[i].Y == playerY)
                    {
                        enemiesX.RemoveAt(i);
                        playerLives--; // Decrease player's lives
                        if (playerLives <= 0)
                        {
                            EndGame("GAME OVER");
                            return;
                        }
                    }
                }
            }

            // Move enemies Y
            enemyYMoveCounter++;
            if (enemyYMoveCounter >= enemyYMoveInterval)
            {
                enemyYMoveCounter = 0;
                List<EnemyY> enemiesYToRemove = new List<EnemyY>();
                foreach (var enemyY in enemiesY)
                {
                    enemyY.MoveSideways(consoleWidth);
                    enemyY.Y++;

                    if (enemyY.Y >= consoleHeight)
                    {
                        enemiesYToRemove.Add(enemyY);
                        // Decrease score by 20 when an enemy reaches bottom
                        score -= 20;
                        // Check if player still has lives
                        if (playerLives <= 0)
                        {
                            EndGame("GAME OVER");
                            return;
                        }
                    }

                    // Check collision with player
                    if (enemyY.X >= playerX && enemyY.X < playerX + playerWidth && enemyY.Y == playerY)
                    {
                        enemiesYToRemove.Add(enemyY);
                        playerLives--; // Decrease player's lives
                        if (playerLives <= 0)
                        {
                            EndGame("GAME OVER");
                            return;
                        }
                    }
                }
                // Remove enemies Y after enumeration
                foreach (var enemyY in enemiesYToRemove)
                {
                    enemiesY.Remove(enemyY);
                }
            }

            // Spawn new enemies X
            enemySpawnCounter++;
            if (enemySpawnCounter >= enemySpawnInterval)
            {
                enemySpawnCounter = 0;
                int newX = random.Next(1, consoleWidth - 1); // Exclude first and last column
                enemiesX.Add(new EnemyX(newX, 0));
                // 10% chance to spawn enemy Y instead of X
                if (random.Next(10) == 0)
                {
                    newX = random.Next(1, consoleWidth - 1); // Exclude first and last column
                    enemiesY.Add(new EnemyY(newX, 0, consoleWidth));
                }
            }

            /*
               Changer l'intervalle de tir : Vous pouvez augmenter ou diminuer enemyShootInterval pour que les ennemis tirent plus ou moins souvent.
               Changer la probabilité de tir : Vous pouvez modifier random.Next(10) == 0 pour ajuster la probabilité que chaque ennemi tire.
               Par exemple, random.Next(5) == 0 donne une probabilité de 20%, tandis que random.Next(20) == 0 donne une probabilité de 5%.
            */

            // Enemies shooting
            enemyShootCounter++;
            if (enemyShootCounter >= enemyShootInterval)
            {
                enemyShootCounter = 0;
                foreach (var enemyX in enemiesX)
                {
                    if (random.Next(5) == 0) // 5: 20% chance to shoot | 10: 10%
                    {
                        enemyBullets.Add(new int[] { enemyX.X, enemyX.Y + 1 });
                    }
                }
                foreach (var enemyY in enemiesY)
                {
                    if (random.Next(5) == 0) // 20% chance to shoot | 10: 10%
                    {
                        enemyBullets.Add(new int[] { enemyY.X, enemyY.Y + 1 });
                    }
                }
            }
        }

        static void EndGame(string message)
        {
            Console.Clear();
            Console.SetCursorPosition(consoleWidth / 2 - message.Length / 2, consoleHeight / 2);
            Console.Write(message);
            Console.SetCursorPosition(consoleWidth / 2 - 9, consoleHeight / 2 + 1);
            Console.Write($"Final Score: {score}");
            Console.SetCursorPosition(consoleWidth / 2 - 12, consoleHeight / 2 + 2);
            Console.Write("Press any key to exit...");
            Console.ReadKey(true);
            Environment.Exit(0);
        }
    }

    class EnemyX
    {
        public int X { get; set; }
        public int Y { get; set; }

        public EnemyX(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    class EnemyY
    {
        public int X { get; set; }
        public int Y { get; set; }
        private int direction;
        private int consoleWidth;

        public EnemyY(int x, int y, int consoleWidth)
        {
            X = x;
            Y = y;
            this.consoleWidth = consoleWidth;
            direction = 1; // Start moving right
        }

        public void MoveSideways(int consoleWidth)
        {
            X += direction;
            if (X >= consoleWidth || X < 0)
            {
                direction *= -1; // Change direction when hitting the screen edge
            }
        }
    }

    class Star
    {
        public int X { get; set; }
        public int Y { get; set; }
        public char Type { get; set; }

        public Star(int x, int y, char type)
        {
            X = x;
            Y = y;
            Type = type;
        }
    }
}
