using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Epi.Statistics
{
    public static class LogisticInteraction
    {
        public static String IOR(double[,] cm, String[] bLabels, double[] B, double[,] DataArray)
        {
            String iorOut = "";
            Boolean noInteractions = true;

            for (int i = 0; i < bLabels.Length; i++)
                if (noInteractions && bLabels[i] != null && bLabels[i].IndexOf(" * ") > 0)
                    noInteractions = false;
            if (noInteractions)
                return iorOut;

            int iaTerms = 1;
            for (int i = 0; i < bLabels.Length; i++)
            {
                if (bLabels[i] != null && bLabels[i].IndexOf(" * ") > 0)
                {
                    int iat = countStars(bLabels[i]);
                    if (iat > iaTerms)
                        iaTerms = iat;
                }
            }

            if (iaTerms > 2)
                return iorOut;

            String lastVar1 = "";
            String lastVar2 = "";
            int interactions = 0;
            for (int i = 0; i < bLabels.Length; i++)
                if (bLabels[i] != null && bLabels[i].IndexOf(" * ") > 0)
                {
                    int v1b = 0;
                    int v1l = bLabels[i].IndexOf(" ");
                    int v2b = bLabels[i].IndexOf(" * ") + 3;
                    int v2l = bLabels[i].Substring(v2b).IndexOf(" ");
                    String var1 = bLabels[i].Substring(v1b, v1l);
                    String var2 = "";
                    if (v2l > 0)
                        var2 = bLabels[i].Substring(v2b, v2l);
                    else
                        var2 = bLabels[i].Substring(v2b);
                    if (!var1.Equals(lastVar1) || !var2.Equals(lastVar2))
                        interactions++;
                    lastVar1 = var1;
                    lastVar2 = var2;
                }

            if (iaTerms == 2 && interactions == 1)
            {
                try
                {
                    bool oneIsDummy = false;
                    bool twoIsDummy = false;
                    for (int i = 0; i < bLabels.Length; i++)
                    {
                        if (!oneIsDummy && bLabels[i] != null && bLabels[i].Length > lastVar1.Length + 1 && bLabels[i].Substring(0, lastVar1.Length).Equals(lastVar1) &&
                            bLabels[i].Substring(lastVar1.Length + 1, 1).Equals("("))
                            oneIsDummy = true;
                        if (!twoIsDummy && bLabels[i] != null && bLabels[i].Length > lastVar2.Length + 1 && bLabels[i].Substring(0, lastVar2.Length).Equals(lastVar2) &&
                            bLabels[i].Substring(lastVar2.Length + 1, 1).Equals("("))
                            twoIsDummy = true;
                    }
                    if (oneIsDummy && twoIsDummy)
                        iorOut = iorOut + TwoDummyVariables(cm, bLabels, B, lastVar1, lastVar2, interactions, iaTerms);
                    else if (oneIsDummy && !twoIsDummy)
                        iorOut = iorOut + DummyFirst(cm, bLabels, B, lastVar1, lastVar2, interactions, iaTerms, DataArray);
                    else if (twoIsDummy && !oneIsDummy)
                        iorOut = iorOut + DummyLast(cm, bLabels, B, lastVar2, lastVar1, interactions, iaTerms, DataArray);
                    else if (!oneIsDummy && !twoIsDummy)
                        iorOut = iorOut + NoDummyVariables(cm, bLabels, B, lastVar1, lastVar2, interactions, iaTerms, DataArray);
                }
                catch (Exception e)
                {
                }
            }

            else
            {
                return iorOut;
            }

            return "<br>" + iorOut;
        }

        public static String TwoDummyVariables(double[,] cm, String[] bLabels, double[] B, String lastVar1, String lastVar2, int interactions, int iaTerms)
        {
            String iorOut = "";
            double Z = SharedResources.ZFromP(0.025);
            String ref1 = getRef(lastVar1, bLabels);
            String ref2 = getRef(lastVar2, bLabels);
            String[] otherValues1 = getNonRef(lastVar1, bLabels);
            String[] otherValues2 = getNonRef(lastVar2, bLabels);
            iorOut = iorOut + "<table><tr><td class=\"stats\" colspan=4 align=\"left\"><strong>Odds Ratios for " + lastVar1 + " * " + lastVar2 + " Interaction</strong></td></tr><tr><td class=\"stats\" align=\"left\"><strong>Label</strong></td><td class=\"stats\" align=\"center\"><strong>Estimate</strong></td><td class=\"stats\" colspan=2 align=\"center\"><strong>95% Confidence Limits</strong></td></tr>";
            for (int i = 0; i < otherValues1.Length / 2; i++)
            {
                double est = -B[Int32.Parse(otherValues1[2 * i])];
                double lcl = est - Z * Math.Sqrt(cm[Int32.Parse(otherValues1[2 * i]), Int32.Parse(otherValues1[2 * i])]);
                double ucl = est + Z * Math.Sqrt(cm[Int32.Parse(otherValues1[2 * i]), Int32.Parse(otherValues1[2 * i])]);
                iorOut = iorOut + "<tr><td class=\"stats\"><strong>" + lastVar1 + " " + ref1 + " vs " + otherValues1[2 * i + 1] +
                    " at " + lastVar2 + "=" + ref2 + "</strong></td>";
                if (Math.Exp(est) < 1000) iorOut = iorOut + "<td  class=\"stats\" align=\"center\">" + Math.Exp(est).ToString("F6") + "</td>";
                else iorOut = iorOut + "<td  class=\"stats\" align=\"center\">" + Math.Exp(est).ToString("0.##E+0") + "</td>";
                if (Math.Exp(lcl) < 1000) iorOut = iorOut + "<td class=\"stats\" align=\"center\">" + Math.Exp(lcl).ToString("F6") + "</td>";
                else iorOut = iorOut + "<td class=\"stats\" align=\"center\">" + Math.Exp(lcl).ToString("0.##E+0") + "</td>";
                if (Math.Exp(ucl) < 1000) iorOut = iorOut + "<td class=\"stats\" align=\"center\">" + Math.Exp(ucl).ToString("F6") + "</td></tr>";
                else iorOut = iorOut + "<td class=\"stats\" align=\"center\">" + Math.Exp(ucl).ToString("0.##E+0") + "</td></tr>";
            }
            for (int i = 0; i < otherValues1.Length / 2; i++)
                for (int j = i + 1; j < otherValues1.Length / 2; j++)
                {
                    double est = B[Int32.Parse(otherValues1[2 * i])] - B[Int32.Parse(otherValues1[2 * j])];
                    double variance = cm[Int32.Parse(otherValues1[2 * i]), Int32.Parse(otherValues1[2 * i])] +
                        cm[Int32.Parse(otherValues1[2 * j]), Int32.Parse(otherValues1[2 * j])] -
                        2 * cm[Int32.Parse(otherValues1[2 * i]), Int32.Parse(otherValues1[2 * j])];
                    double lcl = est - Z * Math.Sqrt(variance);
                    double ucl = est + Z * Math.Sqrt(variance);
                    iorOut = iorOut + "<tr><td class=\"stats\"><strong>" + lastVar1 + " " + otherValues1[2 * i + 1] + " vs " +
                        otherValues1[2 * j + 1] + " at " + lastVar2 + "=" + ref2 + "</strong></td>";
                    if (Math.Exp(est) < 1000) iorOut = iorOut + "<td  class=\"stats\" align=\"center\">" + Math.Exp(est).ToString("F6") + "</td>";
                    else iorOut = iorOut + "<td  class=\"stats\" align=\"center\">" + Math.Exp(est).ToString("0.##E+0") + "</td>";
                    if (Math.Exp(lcl) < 1000) iorOut = iorOut + "<td class=\"stats\" align=\"center\">" + Math.Exp(lcl).ToString("F6") + "</td>";
                    else iorOut = iorOut + "<td class=\"stats\" align=\"center\">" + Math.Exp(lcl).ToString("0.##E+0") + "</td>";
                    if (Math.Exp(ucl) < 1000) iorOut = iorOut + "<td class=\"stats\" align=\"center\">" + Math.Exp(ucl).ToString("F6") + "</td></tr>";
                    else iorOut = iorOut + "<td class=\"stats\" align=\"center\">" + Math.Exp(ucl).ToString("0.##E+0") + "</td></tr>";
                }
            int[] interactionIndexes = getInteractionIndexes(interactions, iaTerms, bLabels);
            int multiple = otherValues2.Length / 2;
            for (int k = 0; k < otherValues2.Length / 2; k++)
            {
                Stack<double> betaStack = new Stack<double>();
                for (int i = 0; i < otherValues1.Length / 2; i++)
                {
                    double est = -(B[Int32.Parse(otherValues1[2 * i])] + B[interactionIndexes[multiple * i + k]]);
                    double variance = cm[Int32.Parse(otherValues1[2 * i]), Int32.Parse(otherValues1[2 * i])] +
                        cm[interactionIndexes[multiple * i + k], interactionIndexes[multiple * i + k]] +
                        2 * cm[Int32.Parse(otherValues1[2 * i]), interactionIndexes[multiple * i + k]];
                    double lcl = est - Z * Math.Sqrt(variance);
                    double ucl = est + Z * Math.Sqrt(variance);
                    betaStack.Push(est);
                    iorOut = iorOut + "<tr><td class=\"stats\"><strong>" + lastVar1 + " " + ref1 + " vs " +
                        otherValues1[2 * i + 1] + " at " + lastVar2 + "=" + otherValues2[2 * k + 1] + "</strong></td>";
                    if (Math.Exp(est) < 1000) iorOut = iorOut + "<td  class=\"stats\" align=\"center\">" + Math.Exp(est).ToString("F6") + "</td>";
                    else iorOut = iorOut + "<td  class=\"stats\" align=\"center\">" + Math.Exp(est).ToString("0.##E+0") + "</td>";
                    if (Math.Exp(lcl) < 1000) iorOut = iorOut + "<td class=\"stats\" align=\"center\">" + Math.Exp(lcl).ToString("F6") + "</td>";
                    else iorOut = iorOut + "<td class=\"stats\" align=\"center\">" + Math.Exp(lcl).ToString("0.##E+0") + "</td>";
                    if (Math.Exp(ucl) < 1000) iorOut = iorOut + "<td class=\"stats\" align=\"center\">" + Math.Exp(ucl).ToString("F6") + "</td></tr>";
                    else iorOut = iorOut + "<td class=\"stats\" align=\"center\">" + Math.Exp(ucl).ToString("0.##E+0") + "</td></tr>";
                }
                double[] betas = betaStack.ToArray();
                for (int i = 0; i < otherValues1.Length / 2; i++)
                    for (int j = i + 1; j < otherValues1.Length / 2; j++)
                    {
                        double est = (B[Int32.Parse(otherValues1[2 * i])] + B[interactionIndexes[multiple * i + k]]) -
                            (B[Int32.Parse(otherValues1[2 * j])] + B[interactionIndexes[multiple * j + k]]);
                        double variance = cm[Int32.Parse(otherValues1[2 * i]), Int32.Parse(otherValues1[2 * i])] +
                            cm[interactionIndexes[multiple * i + k], interactionIndexes[multiple * i + k]] +
                            cm[Int32.Parse(otherValues1[2 * j]), Int32.Parse(otherValues1[2 * j])] +
                            cm[interactionIndexes[multiple * j + k], interactionIndexes[multiple * j + k]] +
                            2 * cm[Int32.Parse(otherValues1[2 * i]), interactionIndexes[multiple * i + k]] +
                            -2 * cm[Int32.Parse(otherValues1[2 * i]), Int32.Parse(otherValues1[2 * j])] +
                            -2 * cm[Int32.Parse(otherValues1[2 * i]), interactionIndexes[multiple * j + k]] +
                            -2 * cm[interactionIndexes[multiple * i + k], Int32.Parse(otherValues1[2 * j])] +
                            -2 * cm[interactionIndexes[multiple * i + k], interactionIndexes[multiple * j + k]] +
                            2 * cm[Int32.Parse(otherValues1[2 * j]), interactionIndexes[multiple * j + k]];
                        double lcl = est - Z * Math.Sqrt(variance);
                        double ucl = est + Z * Math.Sqrt(variance);
                        iorOut = iorOut + "<tr><td class=\"stats\"><strong>" + lastVar1 + " " + otherValues1[2 * i + 1] + " vs " +
                            otherValues1[2 * j + 1] + " at " + lastVar2 + "=" + otherValues2[2 * k + 1] + "</strong></td>";
                        if (Math.Exp(est) < 1000) iorOut = iorOut + "<td  class=\"stats\" align=\"center\">" + Math.Exp(est).ToString("F6") + "</td>";
                        else iorOut = iorOut + "<td  class=\"stats\" align=\"center\">" + Math.Exp(est).ToString("0.##E+0") + "</td>";
                        if (Math.Exp(lcl) < 1000) iorOut = iorOut + "<td class=\"stats\" align=\"center\">" + Math.Exp(lcl).ToString("F6") + "</td>";
                        else iorOut = iorOut + "<td class=\"stats\" align=\"center\">" + Math.Exp(lcl).ToString("0.##E+0") + "</td>";
                        if (Math.Exp(ucl) < 1000) iorOut = iorOut + "<td class=\"stats\" align=\"center\">" + Math.Exp(ucl).ToString("F6") + "</td></tr>";
                        else iorOut = iorOut + "<td class=\"stats\" align=\"center\">" + Math.Exp(ucl).ToString("0.##E+0") + "</td></tr>";
                    }
            }
            return iorOut + "</table>";
        }

        public static String NoDummyVariables(double[,] cm, String[] bLabels, double[] B, String lastVar1, String lastVar2, int interactions, int iaTerms, double[,] DataArray)
        {
            String iorOut = "";
            double Z = SharedResources.ZFromP(0.025);
            int column2 = 1;
            for (int i = 0; i < bLabels.Length; i++)
                if (bLabels[i] != null && bLabels[i].Equals(lastVar2))
                    column2 += i;
            double ref2 = getColumnMean(column2, DataArray);
            int singleIndex = getContinuousIndex(lastVar1, bLabels);
            int[] interactionIndexes = getInteractionIndexes(interactions, iaTerms, bLabels);
            iorOut = iorOut + "<table><tr><td class=\"stats\" colspan=4 align=\"left\"><strong>Odds Ratios for " + lastVar1 + " * " + lastVar2 + " Interaction</strong></td></tr><tr><td class=\"stats\" align=\"left\"><strong>Label</strong></td><td class=\"stats\" align=\"center\"><strong>Estimate</strong></td><td class=\"stats\" colspan=2 align=\"center\"><strong>95% Confidence Limits</strong></td></tr>";

            double est = B[singleIndex] + ref2 * B[interactionIndexes[0]];
            double variance = cm[singleIndex, singleIndex] + Math.Pow(ref2, 2.0) * cm[interactionIndexes[0], interactionIndexes[0]] +
                2 * ref2 * cm[singleIndex, interactionIndexes[0]];
            double lcl = est - Z * Math.Sqrt(variance);
            double ucl = est + Z * Math.Sqrt(variance);
            iorOut = iorOut + "<tr><td class=\"stats\"><strong>" + lastVar1 + " at " + lastVar2 + "=" + ref2.ToString("F3") + "</strong></td>";
            if (Math.Exp(est) < 1000) iorOut = iorOut + "<td  class=\"stats\" align=\"center\">" + Math.Exp(est).ToString("F6") + "</td>";
            else iorOut = iorOut + "<td  class=\"stats\" align=\"center\">" + Math.Exp(est).ToString("0.##E+0") + "</td>";
            if (Math.Exp(lcl) < 1000) iorOut = iorOut + "<td class=\"stats\" align=\"center\">" + Math.Exp(lcl).ToString("F6") + "</td>";
            else iorOut = iorOut + "<td class=\"stats\" align=\"center\">" + Math.Exp(lcl).ToString("0.##E+0") + "</td>";
            if (Math.Exp(ucl) < 1000) iorOut = iorOut + "<td class=\"stats\" align=\"center\">" + Math.Exp(ucl).ToString("F6") + "</td></tr>";
            else iorOut = iorOut + "<td class=\"stats\" align=\"center\">" + Math.Exp(ucl).ToString("0.##E+0") + "</td></tr>";

            return iorOut + "</table>";
        }

        public static String DummyFirst(double[,] cm, String[] bLabels, double[] B, String lastVar1, String lastVar2, int interactions, int iaTerms, double[,] DataArray)
        {
            String iorOut = "";
            double Z = SharedResources.ZFromP(0.025);
            String ref1 = getRef(lastVar1, bLabels);
            int column2 = 1;
            for (int i = 0; i < bLabels.Length; i++)
                if (bLabels[i] != null && bLabels[i].Equals(lastVar2))
                    column2 += i;
            double ref2 = getColumnMean(column2, DataArray);
            String[] otherValues1 = getNonRef(lastVar1, bLabels);
            int[] interactionIndexes = getInteractionIndexes(interactions, iaTerms, bLabels);
            iorOut = iorOut + "<table><tr><td class=\"stats\" colspan=4 align=\"left\"><strong>Odds Ratios for " + lastVar1 + " * " + lastVar2 + " Interaction</strong></td></tr><tr><td class=\"stats\" align=\"left\"><strong>Label</strong></td><td class=\"stats\" align=\"center\"><strong>Estimate</strong></td><td class=\"stats\" colspan=2 align=\"center\"><strong>95% Confidence Limits</strong></td></tr>";
            for (int i = 0; i < otherValues1.Length / 2; i++)
            {
                double est = -B[Int32.Parse(otherValues1[2 * i])] - ref2 * B[interactionIndexes[i]];
                double variance = cm[Int32.Parse(otherValues1[2 * i]), Int32.Parse(otherValues1[2 * i])] + Math.Pow(ref2, 2.0) * cm[interactionIndexes[i], interactionIndexes[i]] +
                    2 * ref2 * cm[Int32.Parse(otherValues1[2 * i]), interactionIndexes[i]];
                double lcl = est - Z * Math.Sqrt(variance);
                double ucl = est + Z * Math.Sqrt(variance);
                iorOut = iorOut + "<tr><td class=\"stats\"><strong>" + lastVar1 + " " + ref1 + " vs " + otherValues1[2 * i + 1] +
                    " at " + lastVar2 + "=" + ref2.ToString("F3") + "</strong></td>";
                if (Math.Exp(est) < 1000) iorOut = iorOut + "<td  class=\"stats\" align=\"center\">" + Math.Exp(est).ToString("F6") + "</td>";
                else iorOut = iorOut + "<td  class=\"stats\" align=\"center\">" + Math.Exp(est).ToString("0.##E+0") + "</td>";
                if (Math.Exp(lcl) < 1000) iorOut = iorOut + "<td class=\"stats\" align=\"center\">" + Math.Exp(lcl).ToString("F6") + "</td>";
                else iorOut = iorOut + "<td class=\"stats\" align=\"center\">" + Math.Exp(lcl).ToString("0.##E+0") + "</td>";
                if (Math.Exp(ucl) < 1000) iorOut = iorOut + "<td class=\"stats\" align=\"center\">" + Math.Exp(ucl).ToString("F6") + "</td></tr>";
                else iorOut = iorOut + "<td class=\"stats\" align=\"center\">" + Math.Exp(ucl).ToString("0.##E+0") + "</td></tr>";
            }
            for (int i = 0; i < otherValues1.Length / 2; i++)
                for (int j = i + 1; j < otherValues1.Length / 2; j++)
                {
                    double est = B[Int32.Parse(otherValues1[2 * i])] + ref2 * B[interactionIndexes[i]] - B[Int32.Parse(otherValues1[2 * j])] - ref2 * B[interactionIndexes[j]];
                    double variance = cm[Int32.Parse(otherValues1[2 * i]), Int32.Parse(otherValues1[2 * i])] +
                        Math.Pow(ref2, 2.0) * cm[interactionIndexes[i], interactionIndexes[i]] + 
                        cm[Int32.Parse(otherValues1[2 * j]), Int32.Parse(otherValues1[2 * j])] +
                        Math.Pow(ref2, 2.0) * cm[interactionIndexes[j], interactionIndexes[j]] +
                        2 * ref2 * cm[Int32.Parse(otherValues1[2 * i]), interactionIndexes[i]] -
                        2 * cm[Int32.Parse(otherValues1[2 * i]), Int32.Parse(otherValues1[2 * j])] -
                        2 * ref2 * cm[Int32.Parse(otherValues1[2 * i]), interactionIndexes[j]] -
                        2 * ref2 * cm[Int32.Parse(otherValues1[2 * j]), interactionIndexes[i]] -
                        2 * ref2 * ref2 * cm[interactionIndexes[j], interactionIndexes[i]] +
                        2 * ref2 * cm[Int32.Parse(otherValues1[2 * j]), interactionIndexes[j]];
                    double lcl = est - Z * Math.Sqrt(variance);
                    double ucl = est + Z * Math.Sqrt(variance);
                    iorOut = iorOut + "<tr><td class=\"stats\"><strong>" + lastVar1 + " " + otherValues1[2 * i + 1] + " vs " +
                        otherValues1[2 * j + 1] + " at " + lastVar2 + "=" + ref2.ToString("F3") + "</strong></td>";
                    if (Math.Exp(est) < 1000) iorOut = iorOut + "<td  class=\"stats\" align=\"center\">" + Math.Exp(est).ToString("F6") + "</td>";
                    else iorOut = iorOut + "<td  class=\"stats\" align=\"center\">" + Math.Exp(est).ToString("0.##E+0") + "</td>";
                    if (Math.Exp(lcl) < 1000) iorOut = iorOut + "<td class=\"stats\" align=\"center\">" + Math.Exp(lcl).ToString("F6") + "</td>";
                    else iorOut = iorOut + "<td class=\"stats\" align=\"center\">" + Math.Exp(lcl).ToString("0.##E+0") + "</td>";
                    if (Math.Exp(ucl) < 1000) iorOut = iorOut + "<td class=\"stats\" align=\"center\">" + Math.Exp(ucl).ToString("F6") + "</td></tr>";
                    else iorOut = iorOut + "<td class=\"stats\" align=\"center\">" + Math.Exp(ucl).ToString("0.##E+0") + "</td></tr>";
                }
            return iorOut + "</table>";
        }

        public static String DummyLast(double[,] cm, String[] bLabels, double[] B, String lastVar1, String lastVar2, int interactions, int iaTerms, double[,] DataArray)
        {
            String iorOut = "";
            double Z = SharedResources.ZFromP(0.025);
            String ref1 = getRef(lastVar1, bLabels);
            int singleIndex = getContinuousIndex(lastVar2, bLabels);
            int[] interactionIndexes = getInteractionIndexes(interactions, iaTerms, bLabels);
            String[] otherValues1 = getNonRef(lastVar1, bLabels);
            iorOut = iorOut + "<table><tr><td class=\"stats\" colspan=4 align=\"left\"><strong>Odds Ratios for " + lastVar2 + " * " + lastVar1 + " Interaction</strong></td></tr><tr><td class=\"stats\" align=\"left\"><strong>Label</strong></td><td class=\"stats\" align=\"center\"><strong>Estimate</strong></td><td class=\"stats\" colspan=2 align=\"center\"><strong>95% Confidence Limits</strong></td></tr>";

            double est0 = B[singleIndex];
            double variance0 = cm[singleIndex, singleIndex];
            double lcl0 = est0 - Z * Math.Sqrt(variance0);
            double ucl0 = est0 + Z * Math.Sqrt(variance0);
            iorOut = iorOut + "<tr><td class=\"stats\"><strong>" + lastVar2 + " at " + lastVar1 + "=" + ref1 + "</strong></td>";
            if (Math.Exp(0) < 1000) iorOut = iorOut + "<td  class=\"stats\" align=\"center\">" + Math.Exp(est0).ToString("F6") + "</td>";
            else iorOut = iorOut + "<td  class=\"stats\" align=\"center\">" + Math.Exp(est0).ToString("0.##E+0") + "</td>";
            if (Math.Exp(lcl0) < 1000) iorOut = iorOut + "<td class=\"stats\" align=\"center\">" + Math.Exp(lcl0).ToString("F6") + "</td>";
            else iorOut = iorOut + "<td class=\"stats\" align=\"center\">" + Math.Exp(lcl0).ToString("0.##E+0") + "</td>";
            if (Math.Exp(ucl0) < 1000) iorOut = iorOut + "<td class=\"stats\" align=\"center\">" + Math.Exp(ucl0).ToString("F6") + "</td></tr>";
            else iorOut = iorOut + "<td class=\"stats\" align=\"center\">" + Math.Exp(ucl0).ToString("0.##E+0") + "</td></tr>";

            for (int i = 0; i < otherValues1.Length / 2; i++)
            {
                double est = B[singleIndex] + B[interactionIndexes[i]];
                double variance = cm[singleIndex, singleIndex] +
                    cm[interactionIndexes[i], interactionIndexes[i]] +
                    2 * cm[singleIndex, interactionIndexes[i]];
                double lcl = est - Z * Math.Sqrt(variance);
                double ucl = est + Z * Math.Sqrt(variance);
                iorOut = iorOut + "<tr><td class=\"stats\"><strong>" + lastVar2 + " at " + lastVar1 + "=" + otherValues1[2 * i + 1] + "</strong></td>";
                if (Math.Exp(est) < 1000) iorOut = iorOut + "<td  class=\"stats\" align=\"center\">" + Math.Exp(est).ToString("F6") + "</td>";
                else iorOut = iorOut + "<td  class=\"stats\" align=\"center\">" + Math.Exp(est).ToString("0.##E+0") + "</td>";
                if (Math.Exp(lcl) < 1000) iorOut = iorOut + "<td class=\"stats\" align=\"center\">" + Math.Exp(lcl).ToString("F6") + "</td>";
                else iorOut = iorOut + "<td class=\"stats\" align=\"center\">" + Math.Exp(lcl).ToString("0.##E+0") + "</td>";
                if (Math.Exp(ucl) < 1000) iorOut = iorOut + "<td class=\"stats\" align=\"center\">" + Math.Exp(ucl).ToString("F6") + "</td></tr>";
                else iorOut = iorOut + "<td class=\"stats\" align=\"center\">" + Math.Exp(ucl).ToString("0.##E+0") + "</td></tr>";
            }
            return iorOut + "</table>";
        }

        public static int[] getInteractionIndexes(int interactions, int iaTerms, String[] bLabels)
        {
            Stack<int> interactionIndexes = new Stack<int>();
            if (iaTerms == 2 && interactions == 1)
                for (int i = bLabels.Length - 1; i >= 0; i--)
                    if (bLabels[i] != null && bLabels[i].IndexOf(" * ") > 0)
                        interactionIndexes.Push(i);
            return interactionIndexes.ToArray();
        }

        public static int getContinuousIndex(String var, String[] bLabels)
        {
            int contIdx = 0;
            for (int i = bLabels.Length - 1; i >= 0; i--)
                if (bLabels[i] != null && bLabels[i].Equals(var))
                    contIdx = i;
            return contIdx;
        }

        public static String[] getNonRef(String var, String[] bLabels)
        {
            Stack<String> nonRefValues = new Stack<String>();
            for (int i = bLabels.Length - 1; i >= 0; i--)
                if (bLabels[i] != null && bLabels[i].IndexOf(" * ") < 0 && bLabels[i].IndexOf(' ') > 0 && bLabels[i].Substring(0, bLabels[i].IndexOf(' ')).Equals(var))
                {
                    nonRefValues.Push(bLabels[i].Substring(var.Length + 2, bLabels[i].Substring(var.Length + 2).IndexOf('/')));
                    nonRefValues.Push("" + i);
                }
            return nonRefValues.ToArray();
        }

        public static String getRef(String var, String[] bLabels)
        {
            String refValue = null;
            for (int i = 0; i < bLabels.Length; i++)
                if (bLabels[i] != null && bLabels[i].IndexOf(" * ") < 0 && bLabels[i].IndexOf(var) >= 0)
                    return bLabels[i].Substring(var.Length + bLabels[i].Substring(var.Length).IndexOf('/') + 1, bLabels[i].Substring(var.Length + bLabels[i].Substring(var.Length).IndexOf('/') + 1).IndexOf(')'));
            return refValue;
        }

        public static String CovMatrix(double[,] cm, String[] bLabels, double[] B)
        {
            String[] matrixLabels = new String[B.Length];
            for (int i = 0; i < matrixLabels.Length; i++)
            {
                int space1 = bLabels[i].IndexOf(' ');
                if (space1 > 0)
                    matrixLabels[i] = bLabels[i].Substring(0, space1);
                else
                    matrixLabels[i] = bLabels[i];
                int paren1 = bLabels[i].IndexOf('(');
                int slash1 = bLabels[i].IndexOf('/');
                int paren2 = bLabels[i].IndexOf(')');
                if (paren1 > 0 && slash1 > paren1)
                    matrixLabels[i] = matrixLabels[i] + bLabels[i].Substring(paren1 + 1, slash1 - paren1 - 1);
                int stars = 0;
                for (int j = 0; j < bLabels[i].Length; j++)
                    if (bLabels[i].Substring(j, 1).Equals("*"))
                        stars++;
                int star = 0;
                for (int j = 0; j < stars; j++)
                {
                    star += bLabels[i].Substring(star + 1).IndexOf('*') + 1;
                    if (star > 0)
                    {
                        space1 = bLabels[i].Substring(star + 2).IndexOf(' ');
                        if (space1 > 0)
                            matrixLabels[i] = matrixLabels[i] + bLabels[i].Substring(star + 2, space1);
                        paren1 = bLabels[i].Substring(star + 2).IndexOf('(');
                        slash1 = bLabels[i].Substring(star + 2).IndexOf('/');
                        paren2 = bLabels[i].Substring(star + 2).IndexOf(')');
                        if (paren1 > 0 && slash1 > 0)
                            matrixLabels[i] = matrixLabels[i] + bLabels[i].Substring(star + 2 + paren1 + 1, slash1 - paren1 - 1);
                    }
                }
            }

            String tableOut = "<table><tr><td>Parameter</td><td>" + matrixLabels[matrixLabels.Length - 1] + "</td>";
            for (int i = 0; i < matrixLabels.Length - 1; i++)
                tableOut = tableOut + "<td>" + matrixLabels[i] + "</td>";
            tableOut = tableOut + "</tr><tr><td>" + matrixLabels[matrixLabels.Length - 1] + "</td><td>" +
                Math.Round(1000000 * cm[matrixLabels.Length - 1, matrixLabels.Length - 1]) / 1000000 + "</td>";
            for (int j = 0; j < matrixLabels.Length - 1; j++)
                tableOut = tableOut + "<td>" + Math.Round(1000000 * cm[matrixLabels.Length - 1, j]) / 1000000 + "</td>";
            tableOut = tableOut + "</tr>";
            for (int i = 0; i < Math.Round(Math.Sqrt((double)cm.Length)) - 1; i++)
            {
                tableOut = tableOut + "<tr><td>" + matrixLabels[i] + "</td><td>" + Math.Round(1000000 * cm[i, matrixLabels.Length - 1]) / 1000000 + "</td>"; ;
                for (int j = 0; j < Math.Round(Math.Sqrt((double)cm.Length)) - 1; j++)
                    tableOut = tableOut + "<td>" + Math.Round(1000000 * cm[i, j]) / 1000000 + "</td>";
                tableOut = tableOut + "</tr>";
            }
            return tableOut + "</table>";
        }

        public static double getColumnMean(int columnNumber, double[,] DataArray)
        {
            double columnSum = 0.0;
            for (int i = 0; i <= DataArray.GetUpperBound(0); i++)
                columnSum += DataArray[i, columnNumber];
            return columnSum / (DataArray.GetUpperBound(0) + 1);
        }

        public static int countStars(String bLabel)
        {
            int numTerms = 0;
            int position = 0;
            int rc = 1;
            while (rc > 0)
            {
                numTerms++;
                rc = bLabel.Substring(position).IndexOf(" * ");
                position += (rc + 3);
            }
            return numTerms;
        }
    }
}
