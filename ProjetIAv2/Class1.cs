using System;
using System.CodeDom;
using System.Collections.Generic;
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
                return ((100-energy)*Form1.tempscaserecharge/100.0);
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

            for (int dx=-1; dx <= 1; dx++)
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
            if (Form1.matrice[x,y] == Form1.recharge)
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
    
            // NOTRE HEURISTIQUE
            // avec 6*sqrt(2)*(xf - xi) + 6*(yf - yi - d1)
                int d = Math.Abs(Form1.xfinal - x);
                int b = Math.Abs(Form1.yfinal - y);
                double h;

                if (d<b)
                { h = Form1.tempscasedepartementale * (Math.Abs(Form1.yfinal - y) + d * (Math.Pow(2, 0.5) - 1)); }
                else
                { h = Form1.tempscasedepartementale * (Math.Abs(Form1.xfinal - x) + b * (Math.Pow(2, 0.5) - 1)); }

                return h;
                
            // HEURISTIQUE DU PROF
                double hProf = Math.Pow((Math.Pow((Form1.xfinal - x), 2) + Math.Pow((Form1.yfinal - y), 2)), 0.5);

            return h;
        }

        public override string ToString()
        {
            return Convert.ToString(x)+","+ Convert.ToString(y)+","
                   +Convert.ToString(Math.Round(energy));
        }
    }
}
