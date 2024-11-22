using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace ProjetIA2022
{
    public partial class Form1 : Form
    {

        static public double[,] matrice;
        static public int nblignes;
        static public int nbcolonnes;
        static public int xinitial;
        static public int yinitial;
        static public int xfinal;
        static public int yfinal;
        public const int autoroute = 3;
        public const int nationale = 2;
        public const int departementale = 1;
        public const int recharge = 8;
        public const int tempscaseautoroute = 10; // 10mn par case
        public const int tempscasenationale = 15;  // 15 mn par case
        public const int tempscasedepartementale = 20; // 20 mn par case
        public const int tempscaserecharge = 30; // 30mn pour passer de 0 à 100 en énergie
        public const int consoparcase = 6;  // 6% Donc 16 cases maximum de déplacement sans recharge
        static public List<Point> powerstations = null;

        static Graphics g;
        public Form1()
        {
            InitializeComponent();
            g = pictureBox1.CreateGraphics();
        }

        public void load_environment(string filename)
        {
            powerstations = new List<Point>();

            StreamReader monStreamReader = new StreamReader(filename);

            // Lecture du fichier avec un while, évidemment !
            // 1ère ligne : nombre de lignes de l'environnement
            string ligne = monStreamReader.ReadLine();
            int i = 0;
            while (ligne[i] != ':') i++;
            string strnblignes = "";
            i++; // On dépasse le ":"
            while (ligne[i] == ' ') i++; // on saute les blancs éventuels
            while (i < ligne.Length)
            {
                strnblignes = strnblignes + ligne[i];
                i++;
            }
            nblignes = Convert.ToInt32(strnblignes);

            // 2ème ligne du fichier, nombre de colonnes de l'environnement
            ligne = monStreamReader.ReadLine();
            i = 0;
            while (ligne[i] != ':') i++;
            string strnbcolonnes = "";
            i++; // On dépasse le ":"
            while (ligne[i] == ' ') i++; // on saute les blancs éventuels
            while (i < ligne.Length)
            {
                strnbcolonnes = strnbcolonnes + ligne[i];
                i++;
            }
            nbcolonnes = Convert.ToInt32(strnbcolonnes);

            // Par défaut, tout l'environnement est inaccessible, on met -1 dans la matrice
            matrice = new double[nbcolonnes, nblignes];
            for (i = 0; i < nbcolonnes; i++)
                for (int j = 0; j < nblignes; j++)
                    matrice[i, j] = -1;

            // Ensuite on répertorie toutes les routes et les points de recharge 
            ligne = monStreamReader.ReadLine();
            while (ligne != null)
            {
                string lignex = ligne;
                int typedecase = -1;
                if (lignex[1] == 'a') typedecase = autoroute;
                else if (lignex[1] == 'n') typedecase = nationale;
                else if (lignex[1] == 'd') typedecase = departementale;
                else if (lignex[1] == 'r') typedecase = recharge;
                
                // Récupérons les coordonnées
                i = 0;
                while (lignex[i] != ':') i++;
                i++; // on passe le :
                while (lignex[i] == ' ') i++; // on saute les blancs éventuels
                string strX = "";
                while (i < lignex.Length)
                {
                    strX = strX + lignex[i];
                    i++;
                }
                int x = Convert.ToInt32(strX);

                // On doit trouver le y associé
                string ligney = monStreamReader.ReadLine();
                i = 0;
                while (ligney[i] != ':') i++;
                i++; // On dépasse le ":"
                // On saute les blancs éventuels
                while (ligney[i] == ' ') i++;
                string strY = "";
                while (i < ligney.Length)
                {
                    strY = strY + ligney[i];
                    i++;
                }
                int y = Convert.ToInt32(strY);

                matrice[x, y] = typedecase;
                if (typedecase == recharge)
                    powerstations.Add(new Point(x, y));   // on mémorise les positions des
                                                        // points de recharge
                ligne = monStreamReader.ReadLine();  // On passe à la case suivante
            }
            // Fermeture du StreamReader (obligatoire) 
            monStreamReader.Close();

            // Affichage dans le picture box
            SolidBrush brush0 = new SolidBrush(Color.White);   // case interdite non routière
            SolidBrush brush1 = new SolidBrush(Color.Green);  // Couleur grise pour départementale
            SolidBrush brush2 = new SolidBrush(Color.Red);  //  Rouge pour nationales      
            SolidBrush brush3 = new SolidBrush(Color.Blue);  //  Rouge pour nationales     
            SolidBrush brush4 = new SolidBrush(Color.Yellow);  // jaune pour recharge   
            int largeur = pictureBox1.Width / nbcolonnes;
            int hauteur = pictureBox1.Height / nbcolonnes;


            for (i = 0; i < nbcolonnes; i++)
                for (int j = 0; j < nblignes; j++)
                    if (matrice[i, j] == departementale)
                    {
                        Rectangle rect = new Rectangle(i * largeur, j * hauteur, largeur - 1, hauteur - 1);
                        g.FillRectangle(brush1, rect);
                    }
                    else if (matrice[i, j] == nationale)
                    {
                        Rectangle rect = new Rectangle(i * largeur, j * hauteur, largeur - 1, hauteur - 1);
                        g.FillRectangle(brush2, rect);
                    }
                    else if (matrice[i, j] == autoroute)
                    {
                        Rectangle rect = new Rectangle(i * largeur, j * hauteur, largeur - 1, hauteur - 1);
                        g.FillRectangle(brush3, rect);
                    }
                    else if (matrice[i, j] == recharge)
                    {
                        Rectangle rect = new Rectangle(i * largeur, j * hauteur, largeur - 1, hauteur - 1);
                        g.FillRectangle(brush4, rect);
                    }
                    else
                    {
                        Rectangle rect = new Rectangle(i * largeur, j * hauteur, largeur - 1, hauteur - 1);
                        g.FillRectangle(brush0, rect);
                    }


        }

        private void buttonAstarBouees_Click(object sender, EventArgs e)
        {
            StartAEtoileElectricCars("environnementB.txt");
        }
        private void button3_Click(object sender, EventArgs e)
        {
            StartAEtoileElectricCars("environnementC.txt");
        }

        private void buttonAstar_Click(object sender, EventArgs e)
        {
            StartAEtoileElectricCars("environnementA.txt");

        }

        private void StartAEtoileElectricCars ( string env )
         {
            load_environment(env);
            xinitial = Convert.ToInt32(textBoxXInit.Text);
            yinitial = Convert.ToInt32(textBoxYinit.Text);
            xfinal = Convert.ToInt32(textBoxXFinal.Text);
            yfinal = Convert.ToInt32(textBoxYFinal.Text);
            if ((matrice[xinitial,yinitial] == -1) || (matrice[xfinal, yfinal] ==-1))
            {
                labeltpstotal.Text = "Pas de solution";
                return;
            }

            SearchTree search = new SearchTree();
            Node2 N0 = new Node2();
            N0.x = xinitial;
            N0.y = yinitial;
            N0.energy = 100.0;
            List<GenericNode> solution = search.RechercheSolutionAEtoile(N0);

            // Affichage de l'arbre d'exploration
            search.GetSearchTree(treeView1);

            // Affichage des noeuds explorés
            // Affichage dans le picture box
            SolidBrush brush1 = new SolidBrush(Color.LightBlue);
            SolidBrush brush2 = new SolidBrush(Color.RoyalBlue);
            SolidBrush brush3 = new SolidBrush(Color.Purple);
            SolidBrush brush4 = new SolidBrush(Color.DarkViolet);
            int largeur = pictureBox1.Width / nbcolonnes;
            int hauteur = pictureBox1.Height / nbcolonnes;
            Rectangle rect;
            // Les fermés
            for (int i = 0; i < search.L_Fermes.Count; i++)
            {
                Node2 noeudferme = (Node2)search.L_Fermes[i];
                rect = new Rectangle(noeudferme.x * largeur + 3, noeudferme.y * hauteur + 3, largeur - 9, hauteur - 9);
                g.FillEllipse(brush1, rect);
            }
            //    Les ouverts 
            for (int i = 0; i < search.L_Ouverts.Count; i++)
            {
                Node2 noeudouvert = (Node2)search.L_Ouverts[i];
                rect = new Rectangle(noeudouvert.x * largeur + 3, noeudouvert.y * hauteur + 3, largeur - 9, hauteur - 9);
                g.FillEllipse(brush2, rect);
            }

            // Affichage de la solution en violet
            listBox1.Items.Clear();
            Node2 N1 = N0;
            rect = new Rectangle(N1.x * largeur + 3, N1.y * hauteur + 3, largeur - 9, hauteur - 9);
            g.FillEllipse(brush3, rect);
            for (int i = 1; i < solution.Count; i++)
            {
                Node2 N2 = (Node2)solution[i];
                listBox1.Items.Add(N1.ToString() + " ---> " + N2.ToString());

                rect = new Rectangle(N2.x * largeur + 3, N2.y * hauteur + 3, largeur - 9, hauteur - 9);
                g.FillEllipse(brush3, rect);

                N1 = N2;
            }
            // Affichage du nb de noeuds dans ouverts et fermés
            labelOuverts.Text = Convert.ToString(search.L_Ouverts.Count);
            labelFermes.Text = Convert.ToString(search.L_Fermes.Count);
            if (solution.Count > 0)
            {
                int tpstotal = (int)(((Node2)solution[solution.Count - 1]).GetGCost());
                labeltpstotal.Text = Convert.ToString(tpstotal/60) + "h"
                     + Convert.ToString(tpstotal % 60) + "mn";
            }
            else
                labeltpstotal.Text = "Pas de solution";
        }

        private void buttonInit3_Click(object sender, EventArgs e)
        {
            load_environment("environnementC.txt");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            load_environment("environnementA.txt");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            load_environment("environnementB.txt");
        }

        // Le code ci-dessous a été exploité pour créer des environnements, il n'est plus utilisé
        private void button4_Click(object sender, EventArgs e)
        {
            Random rnd = new Random();
            StreamWriter monStreamWriter = new StreamWriter("environnementA.txt",true);
            monStreamWriter.WriteLine("nombre de lignes: 20");
            monStreamWriter.WriteLine("nombre de colonnes: 20");

            // Les départementales d'abord, quasi horizontales
            for (int y = 0; y < 20; y = y + 3)
                for (int x = 0; x < 19; x++)
                {
                    monStreamWriter.WriteLine("xdepartementale : " + Convert.ToString(x));
                    if (rnd.NextDouble() > 0.5)
                        monStreamWriter.WriteLine("ydepartementale : "+ Convert.ToString(y));
                    else
                        monStreamWriter.WriteLine("ydepartementale : "+ Convert.ToString(y + 1));
                }
            // quasi verticales
            for (int x = 2; x < 19; x = x + 4)
                for (int y = 0; y < 20; y++)
                {
                    if (rnd.NextDouble() > 0.5)
                        monStreamWriter.WriteLine("xdepartementale : "+ Convert.ToString(x));
                    else
                        monStreamWriter.WriteLine("xdepartementale : " + Convert.ToString(x + 1));

                    monStreamWriter.WriteLine("ydepartementale : " + Convert.ToString(y));
                }

            // 4  quasi diagonales
            for (int y = 0; y < 20; y++)
             {
                    monStreamWriter.WriteLine("xdepartementale : " + Convert.ToString(y));
                    monStreamWriter.WriteLine("ydepartementale : " + Convert.ToString(y));
                if (y - 6 > 0)
                {
                    monStreamWriter.WriteLine("xdepartementale : " + Convert.ToString(y-6));
                    monStreamWriter.WriteLine("ydepartementale : " + Convert.ToString(y));
                }
                if (y + 7 < 20)
                {
                    monStreamWriter.WriteLine("xdepartementale : " + Convert.ToString(y +7));
                    monStreamWriter.WriteLine("ydepartementale : " + Convert.ToString(y));
                }
                monStreamWriter.WriteLine("xdepartementale : " + Convert.ToString(19-y));
                monStreamWriter.WriteLine("ydepartementale : " + Convert.ToString(y));
            }
      

            monStreamWriter.Close();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            pictureBox2.Width = pictureBox1.Width;
            pictureBox3.Height = pictureBox1.Height;
        }
    }
}
