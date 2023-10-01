using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

/**
 *  The minesweeper game below contains an 8x8 grid, with 8 bombs and 8 flags.
 *  The features are : 
 *  Single click to open a square
 *  Right click to drop a red flag (you can reclick on a square to remove the flag)
 *  Replay button to restart a game
 */


namespace Jeu_du_demineur
{
    public partial class Form1 : Form
    {

        public const int gridSize = 8;
        public const int cellSize = 35;
        public const int bombs = 8;
        public int flags = 8;

        public Button[,] buttons = new Button[gridSize, gridSize];
        public byte[,] grid = new byte[gridSize, gridSize];

        public Form1()
        {
            InitializeComponent();
            Start();
        }


        private void Form1_Load_1(object sender, EventArgs e)
        {
            this.AutoSize = false;
            this.Height = 420;
            this.Width = 470;
        }


        public void Start()
        {
            ConfigureMapSize();
            GenerateBombs();
            GenerateGridValues();
            InitButtons();
        }

        private void ConfigureMapSize()
        {
            this.Width = gridSize * cellSize + 20;
            this.Height = (gridSize + 1) * cellSize;
        }

        private void InitButtons()
        {
            label1.Text = "8";

            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    Button button = new Button();
                    button.Location = new Point(j * cellSize, i * cellSize);
                    button.Size = new Size(cellSize, cellSize);
                    button.Click += new EventHandler(OnButtonPressed);
                    button.MouseUp += ButMouseUp;

                    button.Tag = $"{i},{j}";
                    flowLayoutPanel1.Controls.Add(button);
                    buttons[i, j] = button;
                }
            }
        }

        /*
         * Single click event on a square:
         * Reveals a bomb
         * Reveals several empty squares
         * Reveals a square that does not contain a bomb and is not empty
         */
        private void OnButtonPressed(object sender, EventArgs e)
        {
            Button pressedButton = sender as Button;

            int i = Convert.ToInt32(pressedButton.Tag.ToString().Split(',').GetValue(0));
            int j = Convert.ToInt32(pressedButton.Tag.ToString().Split(',').GetValue(1));

            int value = grid[i, j];

            if (value == 10)
            {
                pressedButton.Image = Properties.Resources.bomb;
                MessageBox.Show("Game over !");
                flowLayoutPanel1.Enabled = false;
            }
            else if (value == 20)
            {
                pressedButton.FlatStyle = FlatStyle.Flat;
                pressedButton.FlatAppearance.BorderSize = 0;
                pressedButton.Enabled = false;

                OpenAdjacentCasesEmpty(pressedButton);
            }
            else
            {
                pressedButton.Text = value.ToString();
                pressedButton.Image = null;

            }

            pressedButton.Click -= OnButtonPressed;
        }

        //Open adjacent empty boxes
        private void OpenAdjacentCasesEmpty(Button but)
        {
            int i = Convert.ToInt32(but.Tag.ToString().Split(',').GetValue(0));
            int j = Convert.ToInt32(but.Tag.ToString().Split(',').GetValue(1));
            List<Button> emptyButtons = new List<Button>();

            for (int countI = -1; countI < 2; countI++)
            {
                int checkerI = i + countI;

                for (int countJ = -1; countJ < 2; countJ++)
                {
                    int checkerJ = j + countJ;
                    //No need to check more if the indices are out of bound
                    if (checkerI == -1 || checkerJ == -1 || checkerI > gridSize - 1 || checkerJ > gridSize - 1)
                        continue;

                    if (checkerJ == j && checkerI == i)
                        continue; //no need to check the square itself

                    Button butAdj = buttons[checkerI, checkerJ];
                    int iAdj = Convert.ToInt32(butAdj.Tag.ToString().Split(',').GetValue(0));
                    int jAdj = Convert.ToInt32(butAdj.Tag.ToString().Split(',').GetValue(1));

                    byte value = grid[iAdj, jAdj];
                    if (value == 20)
                    {
                        if (butAdj.FlatStyle != FlatStyle.Flat)
                        {
                            butAdj.FlatStyle = FlatStyle.Flat;
                            butAdj.FlatAppearance.BorderSize = 0;
                            butAdj.Enabled = false;
                            emptyButtons.Add(butAdj);
                            butAdj.Image = null;

                        }
                    }
                    else if (value != 10) // For non - empty buttons that are not bombs
                    {
                        butAdj.PerformClick();
                        butAdj.Image = null;
                    }
                }
            }


            foreach (var butEmpty in emptyButtons)
            {
                OpenAdjacentCasesEmpty(butEmpty);
            }

        }

        //Place or remove a flag
        private void ButMouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                Button but = sender as Button;
                if (but.Text == "")
                {

                    if (but.Image == null)
                    {
                        but.Image = Properties.Resources.flag3;
                        flags--;
                    }
                    else
                    {
                        but.Image = null;
                        flags++;

                    }
                    label1.Text = flags.ToString();

                }
            }
        }


        //Generating bombs
        private void GenerateBombs()
        {
            Random r = new Random();

            int i = 0;
            while (i < bombs)
            {
                int posI = r.Next(0, gridSize - 1);
                int posJ = r.Next(0, gridSize - 1);

                if (grid[posI, posJ] == 0)
                {
                    grid[posI, posJ] = 10; //we assign a bomb to this square  
                    i++;
                }

            }
        }

        /*Generate box values :
         * 10 to indicate a bomb (managed by the GenerateBombs function)
         * 20 to indicate an empty box
         * A number between 1 and 8 to indicate the number of adjacent bombs
         */
        private void GenerateGridValues()
        {
            for (byte i = 0; i < gridSize; i++)
            {
                for (byte j = 0; j < gridSize; j++)
                {
                    if (grid[i, j] == 10)
                        continue; //avoid checking a square containing a bomb

                    byte bombCounts = 0;

                    for (int countI = -1; countI < 2; countI++)
                    {
                        int checkerI = i + countI;

                        for (int countJ = -1; countJ < 2; countJ++)
                        {

                            int checkerJ = j + countJ;
                            //No need to check more if the indices are out of bound
                            if (checkerI == -1 || checkerJ == -1 || checkerI > gridSize - 1 || checkerJ > gridSize - 1)
                                continue;

                            if (checkerJ == j && checkerI == i)
                                continue; //no need to check the box itself

                            if (grid[checkerI, checkerJ] == 10) //there's a bomb
                            {
                                bombCounts++;
                            }

                        }
                    }

                    if (bombCounts == 0)
                    {
                        grid[i, j] = 20;
                    }
                    else
                    {
                        grid[i, j] = bombCounts;
                    }
                }
            }
        }


        //Boutton replay
        private void button1_Click(object sender, EventArgs e)
        {
            flags = 8;
            for(int i=0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    buttons[i, j].Dispose();
                }
            }

            flowLayoutPanel1.Controls.Clear();
            flowLayoutPanel1.Enabled = true;

            label1.Controls.Clear();
            label1.Text = "8";

            grid = new byte[gridSize, gridSize];

            GenerateBombs();
            GenerateGridValues();
            InitButtons();
        }
    }
}
