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
 * Le jeu de demineur suivant contient une grille de taille 8x8, avec 8 bombes disposés et donc 8 drapeaux
 * Les fonctionnalités sont : 
 * - Click simple pour ouvrir une case
 * - Click droit pour déposer un drapeau rouge (on peut reclicker sur une case pour retirer le drapeau)
 * - Boutton rejouer pour recommencer une partie
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


        private void Form1_Load(object sender, EventArgs e)
        {

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

                    //button.MouseUp += new MouseEventHandler(OnButtonPressedMouse);
                    button.Tag = $"{i},{j}";
                    flowLayoutPanel1.Controls.Add(button);
                    buttons[i, j] = button;
                }
            }
        }

        //Evenement de click simple sur une case:
        //- Revele une bombe
        //- Revele plusieurs cases vides
        //- Revele une case ne contenant pas de bombe et non vide
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

        //Ouvrir les cases adjacentes vides
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
                    //Pas besoin de verifier plus si les indices sont out of bound
                    if (checkerI == -1 || checkerJ == -1 || checkerI > gridSize - 1 || checkerJ > gridSize - 1)
                        continue;

                    if (checkerJ == j && checkerI == i)
                        continue; //pas besoin de checker la case elle meme

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
                    else if (value != 10) //Pour les boutons non vide et qui ne sont pas des bombes
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

        //Poser ou retirer un drapeau
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


        //Generer les bombes
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
                    grid[posI, posJ] = 10; //on affecte une bombe a cette case  
                    i++;
                }

            }
        }

        //Generer les valeurs des cases :
        //10 pour indiquer une bombe (gérer à la fonction GenerateBombs)
        //20 pour indiquer une case vide
        //Un nombre entre 1 et 8 pour indiquer le nombre de bombes adjacentes
        private void GenerateGridValues()
        {
            for (byte i = 0; i < gridSize; i++)
            {
                for (byte j = 0; j < gridSize; j++)
                {
                    if (grid[i, j] == 10)
                        continue; //evite de checker une case contenant une bombe

                    byte bombCounts = 0;

                    for (int countI = -1; countI < 2; countI++)
                    {
                        int checkerI = i + countI;

                        for (int countJ = -1; countJ < 2; countJ++)
                        {

                            int checkerJ = j + countJ;
                            //Pas besoin de verifier plus si les indices sont out of bound
                            if (checkerI == -1 || checkerJ == -1 || checkerI > gridSize - 1 || checkerJ > gridSize - 1)
                                continue;

                            if (checkerJ == j && checkerI == i)
                                continue; //pas besoin de checker la case elle meme

                            if (grid[checkerI, checkerJ] == 10) //il y a une bombe
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

       
        //Boutton rejouer
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
