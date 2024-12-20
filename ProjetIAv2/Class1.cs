using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetIA2022
{
    public class Node2 : GenericNode
    {
        public int x;
        public int y;
        public double energy;  // énergie restante de la voiture
                               // entre 0 et 100 ; doit toujours être positif; 

        // Méthodes abstraites, donc à surcharger obligatoirement avec override dans une classe fille
        // version 1, fonctionnelle, mais pas optimale
        public override bool IsEqual(GenericNode N2)
        {
            Node2 N2bis = (Node2)N2;   // N2bis est le potentiel nouveau noeud
            if ((x == N2bis.x) && (y == N2bis.y) && (energy == N2bis.energy))
                return true;
            else return false;
        }
        // version 2 : si on découvre un noeud de même position, plusieurs cas se présentent :
        // si son énergie est supérieure, quel que soit son GCost, il faut considérer
        // que c'est un état jamais vu, car il va permettre d'aller plus loin
        // mais si son énergie est inférieure, c'est un noeud déjà vu, en pire : dans les fermés
        // on ne peut pas avoir mieux de toute façon ; dans les ouverts, attention,
        // si on découvre mieux en terme de distance, il ne faudrait pas perdre le potentiel
        // de l'autre ; en général, cela n'arrive pas, mais ... solution abandonnée pour
        // plus de sûreté

        // public override bool IsEqual(GenericNode N2)
        // {
        //    Node2 N2bis = (Node2)N2;   // N2bis est le potentiel nouveau noeud
        //    if ((x == N2bis.x) && (y == N2bis.y))
        //        if (energy >= N2bis.energy)
        //            return true;
        //        else return false;
        //    else return false;
        // }


        public override double GetArcCost(GenericNode N2)
        {
            // Ici, N2 ne peut être qu'1 des 8 voisins, inutile de le vérifier
            // Par contre, selon le type de case de départ, le coût est différent
            Node2 N2bis = (Node2)N2;     // On "cast" car on sait que c'est un objet de la classe Node2.

            if ((N2bis.y == y) && (N2bis.x == x))
            {
                // On est au même endroit, c'est pour refaire le
                // plein d'énergie, on reste ici un temps proportionnel à la qté
                // d'énergie manquante
                return ((100 - energy) * Form1.tempscaserecharge / 100.0);
            }
            else
            {
                double cost;
                if (Form1.matrice[x, y] == Form1.departementale)
                    cost = Form1.tempscasedepartementale; // autoroute, pas de perte de temps => 10mn
                else if (Form1.matrice[x, y] == Form1.nationale)
                    cost = Form1.tempscasenationale;  // Nationale   => 15mn
                else if (Form1.matrice[x, y] == Form1.autoroute)
                    cost = Form1.tempscaseautoroute; // Route de campagne => 20 mn
                else if (Form1.matrice[x, y] == Form1.recharge)
                    cost = Form1.tempscasenationale;   // cas particulier, on met un coût moyen;
                else
                    cost = 1000000;   // Ne doit jamais arriver ! Si on arrive là
                                      // c'est une erreur

                if ((N2bis.y == y) || (N2bis.x == x))
                    return cost;   // même ligne ou colonne, on se déplace d'1 case
                else return (1.414 * cost); // On est en diagonal, => 1,414 case
            }
        }

        public override bool EndState()
        {
            return (x == Form1.xfinal) && (y == Form1.yfinal);
        }

        public override List<GenericNode> GetListSucc()
        {
            List<GenericNode> lsucc = new List<GenericNode>();

            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if ((x + dx >= 0) && (x + dx < Form1.nbcolonnes)
                            && (y + dy >= 0) && (y + dy < Form1.nblignes)
                            && ((dx != 0) || (dy != 0)))
                        if (Form1.matrice[x + dx, y + dy] > -1)
                        {
                            Node2 newnode2 = new Node2();
                            newnode2.x = x + dx;
                            newnode2.y = y + dy;
                            // l'énergie consommée est supposée proport. à la distance
                            // parcourue
                            double dist = Math.Sqrt(dx * dx + dy * dy);
                            if (energy - dist * Form1.consoparcase >= 0)
                            {
                                newnode2.energy = energy - dist * Form1.consoparcase;
                                lsucc.Add(newnode2);
                            }
                        }
                }

            }
            // Dernier cas, on est sur une case recharge !
            if (Form1.matrice[x, y] == Form1.recharge)
                if (energy < 100)   // Pas totalement chargé
                {
                    Node2 newnode2 = new Node2();
                    newnode2.x = x;
                    newnode2.y = y;
                    newnode2.energy = 100.0;
                    lsucc.Add(newnode2);
                }
            return lsucc;
        }

        public override double CalculeHCost()
        {
            // C'est ici qu'il faut écrire le code de vos heuristiques et retourner une valeur

            // variables disponibles :
            // x, y : position de la voiture pour le noeud examiné, valeurs entre 0 et 19
            // energy : énergie de la voiture à ce noeud là, valeur entre 0 et 100
            // Form1.xinitial Form1.yinitial   coordonnées du point de départ de la voiture
            // Form1.xfinal   Form1.yfinal     coordonnées du point d'arrivée de la voiture
            //  Form1.powerstations : liste des positions des points de recharge
            // par exemple Form1.powerstations[0].x est l'abscisse du 1er point de recharge
            // et Form1.powerstations[0].y est l'ordonnée associée
            // Form1.powerstations.count   renvoie le nombre de points de recharge
            // Accès à certaines constantes :
            // Form1.tempscaseautoroute  : 10mn par déplacement d'1 case sur autoroute
            //  Form1.tempscasenationale  : 15mn par déplacement d'1 case sur nationale
            //   Form1.tempscasedepartementale : 20mn pour déplacement sur départementale
            //   Form1.tempscaserecharge = 30; // 30mn pour passer de 0 à 100 en énergie
            // et proportionnellement moins si on a déjà de l'énergie
            // Form1.matrice[x,y] indique le type de case : 1 pour départementale, 2 pour nationale
            // 3 pour autoroute et 8 pour recharge ; -1 dans la matrice est une case inaccessible

            double h;

            // VITESSE DE REFERENCE POUR L'ENVIRONNEMENT
            //dans l'environnement C, on se base sur le temps de la nationale
            int vitesse = Form1.tempscasenationale;
            //si on est dans l'environnement A et B, on utilise le temps de la départementale
            if (Form1.powerstations.Count() <= 1)
            {
                vitesse = Form1.tempscasedepartementale;
            }

            //DISTANCES
            int distHorizontale = Math.Abs(Form1.xfinal - x);
            int distVerticale = Math.Abs(Form1.yfinal - y);
            //double distanceManhattanEnMieux = CalculeDistanceManhattanEnMieux(x, y, Form1.xfinal, Form1.yfinal);
            double distanceManhattanEnMieux = CalculeDistanceEuclidienne(x, y, Form1.xfinal, Form1.yfinal);

            // on ne rentre dans ce if QUE si on est dans l'environnement C car il faut qu'un des points observés est sur l'autoroute
            // donc c'est imperméable aux environnements A et B
            if (((Form1.matrice[Form1.xfinal, Form1.yfinal] == 3) || (Form1.matrice[x, y] == 3)) && (distHorizontale < distVerticale))
            {
                h = CalculeTemps(x, y, Form1.xfinal, Form1.yfinal);
            }
            else
            {
                //environnements A et B passent toujours par là
                h = vitesse * distanceManhattanEnMieux;
            }

            double energieRestanteALArrivee = energy - distanceManhattanEnMieux * Form1.consoparcase;

            //imperméable à l'environnement A
            if ((Form1.powerstations.Count() > 0) && (energieRestanteALArrivee < 0))
            {
                // si on est dans l'environnement B avec une seule powerstation, SelectionnerPowerStation retourne cette powerstation
                Point powerstation = SelectionnerPowerStation(x, y, energy);
                if (powerstation != new Point())
                {
                    //le if est imperméable aux environnements A et B
                    
                    /* TEST 1 : distance diagonale + ligne droite en prenant en compte l'autoroute
                    if (((Form1.matrice[Form1.xfinal, Form1.yfinal] == 3) || (Form1.matrice[x, y] == 3)) && (distHorizontale < distVerticale))
                    {
                        h = CalculeTemps(x, y, powerstation.X, powerstation.Y) + CalculeTemps(powerstation.X, powerstation.Y, Form1.xfinal, Form1.yfinal);
                    } */

                    /* TEST 2 : distance euclidienne en multipliant par le temps de l'autoroute au lieu du temps de la nationale
                    if (((Form1.matrice[Form1.xfinal, Form1.yfinal] == 4) || (Form1.matrice[x, y] == 4)) && (distHorizontale < distVerticale))
                    {
                        h = Form1.tempscaseautoroute * (CalculeDistanceEuclidienne(x, y, powerstation.X, powerstation.Y) + CalculeDistanceEuclidienne(powerstation.X, powerstation.Y, Form1.xfinal, Form1.yfinal));
                    }*/

                    /*TEST 3 :
                    if (Form1.matrice[18, 0] == 3)
                    { 
                        if ((Form1.matrice[Form1.xfinal, Form1.yfinal] >= 15) || (Form1.matrice[x, y] >= 15))
                        {
                            h = Form1.tempscaseautoroute * (CalculeDistanceEuclidienne(x, y, powerstation.X, powerstation.Y) + CalculeDistanceEuclidienne(powerstation.X, powerstation.Y, Form1.xfinal, Form1.yfinal));
                        }
                        else
                        {
                            h = vitesse * (CalculeDistanceEuclidienne(x, y, powerstation.X, powerstation.Y) + CalculeDistanceEuclidienne(powerstation.X, powerstation.Y, Form1.xfinal, Form1.yfinal));
                        }
                    }*/

                    /*else
                    {*/
                        //h = vitesse * (CalculeDistanceManhattanEnMieux(x, y, powerstation.X, powerstation.Y) + CalculeDistanceManhattanEnMieux(powerstation.X, powerstation.Y, Form1.xfinal, Form1.yfinal));
                        h = vitesse * (CalculeDistanceEuclidienne(x, y, powerstation.X, powerstation.Y) + CalculeDistanceEuclidienne(powerstation.X, powerstation.Y, Form1.xfinal, Form1.yfinal));
                    //}
                }
            }

            return h;
        }

        private double CalculeDistanceManhattanEnMieux(int xactuel, int yactuel, int xfinal, int yfinal)
        {
            double h;
            int distHorizontale = Math.Abs(xfinal - xactuel);
            int distVerticale = Math.Abs(yfinal - yactuel);


            if (distHorizontale < distVerticale)
            { h = distVerticale + distHorizontale * (Math.Pow(2, 0.5) - 1); }
            else
            { h = distHorizontale + distVerticale * (Math.Pow(2, 0.5) - 1); }

            return h;
        }

        private double CalculeDistanceEuclidienne(int xactuel, int yactuel, int xfinal, int yfinal)
        {
            return Math.Pow((Math.Pow((xfinal - xactuel), 2) + Math.Pow((yfinal - yactuel), 2)), 0.5);
        }

        private double CalculeTemps(int xactuel, int yactuel, int xfinal, int yfinal)
        {
            int coteducarre = Math.Abs(xfinal - xactuel);
            double tempsdiagonale = Form1.tempscasenationale * Math.Sqrt(2) * coteducarre;
            double tempsrestant = Form1.tempscaseautoroute * (Math.Abs(yfinal - yactuel) - coteducarre);
            return tempsdiagonale + tempsrestant;
        }

        private Point SelectionnerPowerStation(int xactuel, int yactuel, double energy)
        {
            Point powerstationSelectionnee = new Point();
            double minDistanceArrivee = 40;

            foreach (Point powerstation in Form1.powerstations)
            {
                //double distanceManhattanEnMieux = CalculeDistanceManhattanEnMieux(xactuel, yactuel, powerstation.X, powerstation.Y);
                double distanceManhattanEnMieux = CalculeDistanceEuclidienne(xactuel, yactuel, powerstation.X, powerstation.Y);
                double energyRestanteALArrivee = energy - distanceManhattanEnMieux * Form1.consoparcase;

                if (energyRestanteALArrivee >= 0)
                {
                    //double distanceArrivee = CalculeDistanceManhattanEnMieux(xactuel, yactuel, powerstation.X, powerstation.Y) + CalculeDistanceManhattanEnMieux(powerstation.X, powerstation.Y, Form1.xfinal, Form1.yfinal);
                    double distanceArrivee = CalculeDistanceEuclidienne(xactuel, yactuel, powerstation.X, powerstation.Y) + CalculeDistanceEuclidienne(powerstation.X, powerstation.Y, Form1.xfinal, Form1.yfinal);

                    if (distanceArrivee < minDistanceArrivee)
                    {
                        minDistanceArrivee = distanceArrivee;
                        powerstationSelectionnee = powerstation;
                    }
                }
            }

            return powerstationSelectionnee;
        }

        public override string ToString()
        {
            return Convert.ToString(x) + "," + Convert.ToString(y) + ","
                   + Convert.ToString(Math.Round(energy));
        }
    }
}
