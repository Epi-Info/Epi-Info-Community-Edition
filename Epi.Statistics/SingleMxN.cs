using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RDotNet;

namespace Epi.Statistics
{
    public class Node
    {
        private string ID;
        private int stage;
        private int[] values;
        private List<string> leadsTo;

        public Node()
        {
            this.ID = System.Guid.NewGuid().ToString();
            this.leadsTo = new List<string>();
        }

        public string getID()
        {
            return this.ID;
        }

        public void setStage(int stage)
        {
            this.stage = stage;
        }
        public int getStage()
        {
            return this.stage;
        }

        public void setValues(int[] values)
        {
            this.values = new int[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                this.values[i] = values[i];
            }
        }
        public int[] getValues()
        {
            return this.values;
        }

        public void pushLeadsTo(string s)
        {
            bool add = true;
            for (int i = 0; i < this.leadsTo.Count; i++)
            {
                if (s.Equals(this.leadsTo.ElementAt(i)))
                {
                    add = false;
                }
            }
            if (add)
            {
                this.leadsTo.Add(s);
            }
        }
        public int getLeadsToSize()
        {
            return this.leadsTo.Count;
        }
        public string getLeadsToElementAt(int i)
        {
            return this.leadsTo.ElementAt(i);
        }
        public List<string> getLeadsTo()
        {
            return this.leadsTo;
        }

        public bool equals(Node node)
        {
            if (this.stage != node.getStage() || this.values.Length != node.getValues().Length)
            {
                return false;
            }

            for (int i = 0; i < this.values.Length; i++)
            {
                if (this.values[i] != node.getValues()[i])
                {
                    return false;
                }
            }

            return true;
        }

        public string toString()
        {
            string retStr = "{" + this.stage + ") " + this.values[0];
            for (int i = 1; i < this.values.Length; i++)
            {
                retStr = retStr + ", " + this.values[i];
            }
            retStr = retStr + "}";
            return retStr;
        }

        public string verboseToString()
        {
            string retStr = this.toString();

            for (int i = 0; i < this.leadsTo.Count; i++)
            {
                retStr = retStr + "\n\t" + this.leadsTo.ElementAt(i);
            }
            return retStr;
        }
    }

    public static class SingleMxN
    {
        // Coumpute Fisher Exact Statistic
        public static double FEXACT(System.Data.DataRow[] SortedRows)
        {
            int[][] table = new int[SortedRows.Count() + 1][];
            for (int i = 0; i <= SortedRows.Count(); i++)
            {
                table[i] = new int[SortedRows[0].ItemArray.Length];
                if (i == 0)
                    continue;
                System.Data.DataRow dr = SortedRows.ElementAt(i - 1);
                for (int j = 1; j < dr.ItemArray.Count(); j++)
                {
                    table[i][j] = int.Parse(SortedRows[i - 1][j].ToString());
                }
            }

            if (table.Length > table[1].Length)
                table = transposeMatrix(table);

            int ncol = table[1].Length - 1;
            int nrow = table.Length - 1;
            int ldtabl = nrow;
            double expect = 0.0;
            double percent = 90.0;
            double emin = 1.0;
            double prt = 0.0;
            double pre = 0.0;

            int iwkmax = 200000;
            int mult = 30;
            int ireal = 4;
            double amiss = -12345.0;
            int iwkpt = 1;
            int ntot = 0;

            for (int r = 1; r <= nrow; r++)
            {
                for (int c = 1; c <= ncol; c++)
                {
                    ntot += table[r][c];
                }
            }

            int nco = ncol;
            int nro = nrow;
            int k = nrow + ncol + 1;
            int kk = k * ncol;
            int ldkey, ldstp, numb;

            int i1 = iwork(iwkmax, ref iwkpt, ntot + 1, ireal) - 1;
            int i2 = iwork(iwkmax, ref iwkpt, nco, 2) - 1;
            int i3 = iwork(iwkmax, ref iwkpt, nco, 2) - 1;
            int i3a = iwork(iwkmax, ref iwkpt, nco, 2) - 1;
            int i3b = iwork(iwkmax, ref iwkpt, nro, 2) - 1;
            int i3c = iwork(iwkmax, ref iwkpt, nro, 2) - 1;
            int iiwk = iwork(iwkmax, ref iwkpt, Math.Max(5 * k + 2 * kk, 800 + 7 * ncol), 2) - 1;
            int irwk = iwork(iwkmax, ref iwkpt, Math.Max(400 + ncol + 1, k), ireal) - 1;

            if (ireal == 4)
            {
                numb = 18 + 10 * mult;
                ldkey = (iwkmax - iwkpt + 1) / numb;
            }
            else
            {
                numb = 12 * 8 * mult;
                ldkey = (iwkmax - iwkpt + 1) / numb;
            }

            // double[] rwrk = new double[200000];

            double[][] dwrk = new double[(int)(100000 / (2 * ldkey))][];
            int[][] iwrk = new int[(int)(100000 / (2 *ldkey))][];
            for (int i = 0; i < dwrk.Length; i++)
            {
                dwrk[i] = new double[2];// *ldkey];
                iwrk[i] = new int[2];// *ldkey];
            }

            ldstp = mult * ldkey;
            int i4 = iwork(iwkmax, ref iwkpt, 2 * ldkey, 2) - 1;
            int i5 = iwork(iwkmax, ref iwkpt, 2 * ldkey, 2) - 1;
            int i6 = iwork(iwkmax, ref iwkpt, 2 * ldstp, ireal) - 1;
            int i7 = iwork(iwkmax, ref iwkpt, 6 * ldstp, 2) - 1;
            int i8 = iwork(iwkmax, ref iwkpt, 2 * ldkey, ireal) - 1;
            int i9 = iwork(iwkmax, ref iwkpt, 2 * ldkey, ireal) - 1;
            int i9a = iwork(iwkmax, ref iwkpt, 2 * ldkey, ireal) - 1;
            int i10 = iwork(iwkmax, ref iwkpt, 2 * ldkey, 2) - 1;

            double[] i1array = new double[irwk];
            int[] i2array = new int[i3 - i2 + 1];
            int[] i3array = new int[i3a - i3 + 1];
            int[] i3aarray = new int[i3b - i3a + 1];
            int[] i3barray = new int[i3c - i3b + 1];
            int[] i3carray = new int[iiwk - i3c + 1];
            int[] i4array = new int[i5 - i4 + 1];
            int[] i5array = new int[i7 - i5 + 1];
            double[] i6array = new double[i8 - i6 + 1];
            int[] i7array = new int[i10 - i7 + 1];
            double[] i8array = new double[i9 - i8 + 1];
            double[] i9array = new double[i9a - i9 + 1];
            double[] i9aarray = new double[iwkmax - i9a + 1];
            int[] i10array = new int[2 * ldkey + 1];
            int[] iiwkarray = new int[i4 - iiwk + 1];
            double[] irwkarray = new double[i6 - irwk + 1];

            f2xact(nrow, ncol, table, ldtabl, expect,
                   percent, emin, ref prt, ref pre, i1array,
                   i2array, i3array, i3aarray, i3barray, i3carray,
                   i4array, ldkey, i5array, i6array, ldstp,
                   i7array, i8array, i9array, i9aarray, i10array,
                   iiwkarray, irwkarray);

            return pre;
        }

        private static void f2xact(int nrow, int ncol, int[][] table, int ldtabl, double expect,
                            double percnt, double emin, ref double prt, ref double pre, double[] fact,
                            int[] ico, int[] iro, int[] kyy, int[] idif, int[] irn,
                            int[] key, int ldkey, int[] ipoin, double[] stp, int ldstp,
                            int[] ifrq, double[] dlp, double[] dsp, double[] tm, int[] key2,
                            int[] iwk, double[] rwk)
        {
            int f5itp = 0;

            for (int i = 1; i <= 2 * ldkey; i++)
            {
                key[i] = -9999;
                key2[i] = -9999;
            }
            int preops = 0;

            int ncell;
            int ifault = 1;
            int iflag = 1;
            double tmp = 0.0;
            double pv;
            double df;
            double obs2;
            double obs3;
            bool chisq = false;

            pre = 0.0;
            int itop = 0;
            double emn = 0.0;
            double emx = 1.0e30;
            double tol = 3.45254e-7;
            double amiss = -12345.0;
            double imax = 2147483647;

            if (expect > 0.0)
            {
                emn = emin;
            }
            else
            {
                emn = emx;
            }

            int k = ncol;

            // Variables for f3xact
            int i31 = 1;
            int i32 = i31 + k;
            int i33 = i32 + k;
            int i34 = i33 + k;
            int i35 = i34 + k;
            int i36 = i35 + k;
            int i37 = i36 + k;
            int i38 = i37 + k;
            int i39 = i38 + 400;
            int i310 = 1;
            int i311 = 401;

            // Variables for f4xact
            k = nrow + ncol + 1;
            int i41 = 1;
            int i42 = i41 + k;
            int i43 = i42 + k;
            int i44 = i43 + k;
            int i45 = i44 + k;
            int i46 = i45 + k;
            int i47 = i46 + k * ncol;
            int i48 = 1;

            if (nrow > ldtabl)
            {
                return;
            }
            if (ncol <= 1)
            {
                return;
            }

            int ntot = 0;
            int[] nint = new int[maxTableCell(nrow, ncol, table) + 1];
            for (int r = 1; r <= nrow; r++)
            {
                iro[r] = 0;
                for (int c = 1; c <= ncol; c++)
                {
                    if (table[r][c] < -0.0001)
                    {
                        return;
                    }
                    iro[r] += table[r][c];
                    ntot += table[r][c];
                }
            }
            // Resize iro and to the proper row size
            int[] riro = new int[nrow + 1];
            for (int r = 1; r <= nrow; r++)
            {
                riro[r] = iro[r];
            }
            iro = riro;

            if (ntot == 0)
            {
                prt = amiss;
                pre = amiss;
                return;
            }

            for (int c = 1; c <= ncol; c++)
            {
                ico[c] = 0;
                for (int r = 1; r <= nrow; r++)
                {
                    ico[c] += table[r][c];
                }
            }
            // Resize ico to the proper column size
            int[] rico = new int[ncol + 1];
            for (int c = 1; c <= ncol; c++)
            {
                rico[c] = ico[c];
            }
            ico = rico;

            Array.Sort(iro);
            Array.Sort(ico);

            int nro = nrow;
            int nco = ncol;

            int[] rkyy = new int[nro + 1];
            rkyy[1] = 1;
            int mj = ncol;
            for (int r = 2; r <= nro; r++)
            {
                if (iro[r - 1] + 1 <= imax / rkyy[r - 1])
                {
                    rkyy[r] = rkyy[r - 1] * (iro[r - 1] + 1);
                    mj = mj / rkyy[r - 1];
                }
                else
                {
                    rkyy[r] = rkyy[r - 1] * (iro[r - 1] + 1);
                    mj = mj / rkyy[r - 1];
                }
            }
            kyy = rkyy;

            int kmax;
            if (iro[nro - 1] + 1 <= imax / kyy[nro - 1])
            {
                kmax = (iro[nro] + 1) * kyy[nro - 1];
            }
            else
            {
                kmax = (iro[nro] + 1) * kyy[nro - 1];
            }

            fact[0] = 0.0;
            fact[1] = 0.0;
            fact[2] = Math.Log(2.0);
            for (int i = 3; i <= ntot; i += 2)
            {
                fact[i] = fact[i - 1] + Math.Log((double)i);
                mj = i + 1;
                if (mj <= ntot)
                {
                    fact[mj] = fact[i] + fact[2] + fact[mj / 2] - fact[mj / 2 - 1];
                }
            }

            double obs = tol;
            ntot = 0;
            double dd = 0.0;
            for (mj = 1; mj <= nco; mj++)
            {
                dd = 0.0;
                for (int r = 1; r <= nro; r++)
                {
                    dd = dd + fact[table[r][mj]];
                    ntot = ntot + table[r][mj];
                }
                obs = obs + fact[ico[mj]] - dd;
            }
            double dro = f9xact(nro, ntot, iro, 1, fact);
            prt = Math.Exp(obs - dro);

            // Initializa pointers
            k = nco;
            int last = ldkey + 1;
            int jkey = ldkey + 1;
            int jstp = ldstp + 1;
            int jstp2 = 3 * ldstp + 1;
            int jstp3 = 4 * ldstp + 1;
            int jstp4 = 5 * ldstp + 1;
            int ikkey = 0;
            int ikstp = 0;
            int ikstp2 = 2 * ldstp;
            int ipo = 1;
            ipoin[1] = 1;
            stp[1] = 0.0;
            ifrq[1] = 1;
            ifrq[ikstp2 + 1] = -1;

            //           bool goto110 = true;
            goto110:
            int kb = nco - k + 1;
            int ks = 0;
            int n = ico[kb]; //Ends up being the lowest column total
            int kd = nro + 1;
            kmax = nro;

            for (int i = 1; i <= nro; i++)
            {
                idif[i] = 0;
            }

            goto130:
            kd = kd - 1; // So kd is now highest index of row totals vector
            ntot = Math.Min(n, iro[kd]); // The lowest column total or the highest row total??
            idif[kd] = ntot;
            if (idif[kmax] == 0)
            {
                kmax = kmax - 1;
            }
            n = n - ntot;
            if (n > 0 && kd != 1)
            {
                goto goto130;
            }

            int k1;
            if (n != 0)
            {
                goto goto310;
            }

            k1 = k - 1;
            n = ico[kb];
            ntot = 0;
            // kb began as 1 less than the FORTRAN value so this is the same as in FORTRAN
            for (int i = kb + 1; i <= nco; i++)
            {
                ntot = ntot + ico[i];
            }

            //            bool goto150 = true;
            goto150:
            for (int i = 1; i <= nro; i++)
            { // Line 150
                irn[i] = iro[i] - idif[i];
            } // Line 160

            // Sort irn 
//            Array.Sort(irn);
//            irn = irnhatt;

            int nrb = 0;
            int nro2 = 0;
            int ii = 0;
            if (k1 > 1)
            {
                int i = nro;
                if (nro == 2)
                {
                    if (irn[1] > irn[2])
                    {
                        ii = irn[1];
                        irn[1] = irn[2];
                        irn[2] = ii;
                    }
                }
                else if (nro == 3)
                {
                    ii = irn[1];
                    if (ii > irn[3])
                    {
                        if (ii > irn[2])
                        {
                            if (irn[2] > irn[3])
                            {
                                irn[1] = irn[3];
                                irn[3] = ii;
                            }
                            else
                            {
                                irn[1] = irn[1];
                                irn[2] = irn[2];
                                irn[3] = ii;
                            }
                        }
                        else
                        {
                            irn[1] = irn[3];
                            irn[3] = irn[2];
                            irn[2] = ii;
                        }
                    }
                    else if (ii > irn[2])
                    {
                        irn[1] = irn[2];
                        irn[2] = ii;
                    }
                    else if (irn[2] > irn[3])
                    {
                        ii = irn[2];
                        irn[2] = irn[3];
                        irn[3] = ii;
                    }
                }
                else
                {
                    for (int j = 2; j <= nro; j++)
                    {
                        i = j - 1;
                        ii = irn[j];
                        goto170:
                        if (ii < irn[i])
                        {
                            irn[i + 1] = irn[i];
                            i--;
                            if (i > 0)
                            {
                                goto goto170;
                            }
                        }
                        irn[i + 1] = ii;
                    }
                }
                for (i = 1; i <= nro; i++)
                {
                    if (irn[i] != 0)
                    {
                        goto goto200;
                    }
                }
                goto200:
                nrb = i;
                nro2 = nro - i + 1;
            }
            else
            {
                nrb = 1;
                nro2 = nro;
            }

            // The "Sort irn" section of the FORTRAN program
            // I think it does important things besides sorting an array

            // Some table values
            double ddf = f9xact(nro, n, idif, 1, fact);
            double drn = f9xact(nro2, ntot, irn, nrb, fact) - dro + ddf;

            // Get hash values
            int itp = 0;
            int kval = 1;
            if (k1 > 1)
            { // REVISIT: FORTRAN has if k1 gt 1
              // All of these indices are reduced by 1 from FORTRAN
                kval = irn[1] + irn[2] * kyy[2];
                int i = 2;
                for (i = 3; i <= nro; i++)
                {
                    kval = kval + irn[i] * kyy[i];
                }

                // Get hash table entry
                i = kval % (2 * ldkey) + 1;

                // Search for unused location
                for (itp = i; itp <= 2 * ldkey; itp++)
                {
                    ii = key2[itp];
                    if (ii == kval)
                    {
                        goto goto240;
                    }
                    else if (ii < 0)
                    {
                        key2[itp] = kval;
                        dlp[itp] = 1.0;
                        dsp[itp] = 1.0;
                        goto goto240;
                    }
                }

                for (itp = 1; itp <= i - 1; itp++)
                {
                    ii = key2[itp];
                    if (ii == kval)
                    {
                        goto goto240;
                    }
                    else if (ii < 0)
                    {
                        key2[itp] = kval;
                        dlp[itp] = 1.0;
                        goto goto240;
                    }
                }
            }

            goto240:
            bool ipsh = true;

            // Recover pastp
            int ipn = ipoin[ipo + ikkey];
            double pastp = stp[ipn + ikstp];
            int ifreq = ifrq[ipn + ikstp];
            // REVISIT: Had to use the min function bc ipn+ikstp was out of bounds

            // Compute shortest and longest path
            if (k1 > 1)
            { // REVISIT: FORTRAN says .gt. 1
                obs2 = obs - fact[ico[kb + 1]] - fact[ico[kb + 2]] - ddf;
                for (int i = 3; i <= k1; i++)
                {
                    obs2 = obs2 - fact[ico[kb + i]];
                }

                if (dlp[itp] > 0.0)
                {
                    double dspt = obs - obs2 - ddf;
                    // Compute longest path
                    dlp[itp] = 0.0;
                    // First need separate work arrays
                    int[] iwk0 = new int[iwk.Length];
                    int[] iwk1 = new int[iwk.Length];
                    int[] iwk2 = new int[iwk.Length];
                    int[] iwk3 = new int[iwk.Length];
                    int[] iwk4 = new int[iwk.Length];
                    int[] iwk5 = new int[iwk.Length];
                    int[] iwk6 = new int[iwk.Length];
                    int[] iwk7 = new int[iwk.Length];
                    int[] iwk8 = new int[iwk.Length];
                    Array.Copy(iwk, iwk0, iwk.Length);
                    Array.Copy(iwk, iwk1, iwk.Length);
                    Array.Copy(iwk, iwk2, iwk.Length);
                    Array.Copy(iwk, iwk3, iwk.Length);
                    Array.Copy(iwk, iwk4, iwk.Length);
                    Array.Copy(iwk, iwk5, iwk.Length);
                    Array.Copy(iwk, iwk6, iwk.Length);
                    Array.Copy(iwk, iwk7, iwk.Length);
                    Array.Copy(iwk, iwk8, iwk.Length);
                    double[] rwk0 = new double[rwk.Length];
                    double[] rwk1 = new double[rwk.Length];
                    Array.Copy(rwk, rwk0, rwk.Length);
                    Array.Copy(rwk, rwk1, rwk.Length);
                    //
                    f3xact(nro2, irn, nrb, k1, ico, kb + 1, ref dlp[itp],
                           ref ntot, fact, iwk0, iwk1, iwk2,
                           iwk3, iwk4, iwk5, iwk6,
                           iwk7, iwk8, rwk0, rwk1, tol);
                    dlp[itp] = Math.Min(0.0, dlp[itp]);

                    // Compute shortest path
                    dsp[itp] = dspt;
                    f4xact(nro2, irn, nrb, k1, ico, kb + 1, ref dsp[itp],
                           fact, iwk0, iwk1, iwk2,
                           iwk3, iwk4, iwk5, iwk6,
                           rwk0, tol);
                    dsp[itp] = Math.Min(0.0, dsp[itp] - dspt);

                    // Use chi-squared approximation
                    if ((double)((irn[nrb] * ico[kb + 1]) / (double)ntot) > emn)
                    {
                        ncell = 0;
                        for (int j = 1; j <= nro2; j++)
                        {
                            for (int l = 1; l <= k1; l++)
                            {
                                if (irn[nrb + j - 1] * ico[kb + l] >= ntot * expect)
                                {
                                    ncell = ncell + 1;
                                }
                            }
                        }
                        if (ncell * 100 >= k1 * nro2 * percnt)
                        {
                            tmp = 0.0;
                            for (int j = 1; j <= nro2; j++)
                            {
                                tmp = tmp + fact[irn[nrb + j - 1]] - fact[irn[nrb + j - 1] - 1];
                            }
                            tmp = tmp * (k1 - 1);
                            for (int j = 1; j <= k1; j++)
                            {
                                tmp = tmp + (nro2 - 1) * (fact[ico[kb + j]] - fact[ico[kb + j] - 1]);
                            }
                            df = (nro2 - 1) * (k1 - 1);
                            tmp = tmp + df * 1.83787706640934548356065947281;
                            tmp = tmp - (nro2 * k1 - 1) * (fact[ntot] - fact[ntot - 1]);
                            tm[itp] = -2.0 * (obs - dro) - tmp;
                        }
                        else
                        {
                            tm[itp] = -9876.0;
                        }
                    }
                    else
                    {
                        tm[itp] = -9876.0;
                    }
                }
                obs3 = obs2 - dlp[itp];
                obs2 = obs2 - dsp[itp];
                if (tm[itp] == -9876.0)
                {
                    chisq = false;
                }
                else
                {
                    chisq = true;
                    tmp = tm[itp];
                }
            }
            else
            {
                obs2 = obs - drn - dro;
                obs3 = obs2;
            }
            // Process node with new PASTP
            // bool goto300 = true;
            goto300:
            if (pastp <= obs3)
            {
                pre = pre + ifreq * Math.Exp(pastp + drn);
                preops++;
                if (preops == 106 || preops == 13)
                {
                    bool checkpoint = true;
                }
            }
            else if (pastp < obs2)
            {
                if (chisq)
                {
                    df = (nro2 - 1) * (k1 - 1);
                    pv = 1.0 - gammds(Math.Max(0.0, tmp + 2.0 * (pastp + drn)) / 2.0, df / 2.0, ifault);
                    pre = pre + ifreq * Math.Exp(pastp + drn) * pv;
                }
                else
                {
                    // Put daughter on queue
                    f5xact(pastp + ddf, tol, kval, ref key, jkey, ldkey,
                           ref ipoin, jkey, ref stp, jstp, ldstp, ref ifrq, jstp,
                           ref ifrq, jstp2, ref ifrq, jstp3, ref ifrq, jstp4, ifreq,
                           ref itop, ipsh, ref f5itp);
                    ipsh = false;
                }
            }
            // Get next PASTP on chain
            ipn = ifrq[ipn + ikstp2];
            if (ipn > 0)
            {
                pastp = stp[ipn + ikstp];
                ifreq = ifrq[ipn + ikstp];
                goto goto300;
            }
            // Generate new daughter node
            f7xact(kmax, iro, ref idif, ref kd, ref ks, ref iflag);
            if (iflag != 1)
            {
                goto goto150;
            }
            // Go get a new mother from stage K
            // bool goto310 = true;
            goto310:
            iflag = 1;
            f6xact(nro, ref iro, ref iflag, kyy, ref key, ikkey + 1, ldkey, ref last, ref ipo);
            if (iflag == 3)
            {
                k = k - 1;
                itop = 0;
                ikkey = jkey - 1;
                ikstp = jstp - 1;
                ikstp2 = jstp2 - 1;
                jkey = ldkey - jkey + 2;
                jstp = ldstp - jstp + 2;
                jstp2 = 2 * ldstp + jstp;
                for (int f = 1; f <= 2 * ldkey; f++)
                {
                    key2[f] = -9999;
                }
                if (k >= 2)
                {
                    goto goto310;
                }
            }
            else
            {
                // goto310 = false;
                // goto110 = true;
                goto goto110;
            }
        }

        private static void f3xact(int nrow, int[] irow, int irowoffset, int ncol, int[] icol, int icoloffset, ref double dlp, ref int mm,
                            double[] fact, int[] ico, int[] iro, int[] it, int[] lb, int[] nr,
                            int[] nt, int[] nu, int[] itc, int[] ist, double[] stv, double[] alen,
                            double tol)
        {
            int n11 = 0;
            int n12 = 0;
            int nro;
            int nco;
            double val;
            int nn;
            bool xmin;
            int nitc = 0;
            int nst = 0;
            int nn1 = 0;
            int nc1 = 0;
            int ic1 = 0;
            int ic2 = 0;
            int ii = 0;
            int key = 0;
            int ipn = 0;
            int itp = 0;

            for (int i = 0; i <= ncol; i++)
            {
                alen[i] = 0.0;
            }
            for (int i = 1; i <= 400; i++)
            {
                ist[i] = -1;
            }

            // nrow is 1
            if (nrow <= 1)
            {
                if (nrow > 0)
                {
                    dlp = dlp - fact[icol[1]];
                    for (int i = 2; i <= ncol; i++)
                    {
                        dlp = dlp - fact[icol[i + icoloffset - 1]];
                    }
                }
                return;
            }

            // ncol is 1
            if (ncol <= 1)
            {
                if (ncol > 0)
                {
                    dlp = dlp - fact[irow[1]] - fact[irow[2]];
                    for (int i = 3; i <= nrow; i++)
                    {
                        dlp = dlp - fact[irow[i + irowoffset - 1]];
                    }
                }
                return;
            }

            // 2 by 2 table
            if (nrow * ncol == 4)
            {
                n11 = (irow[1 + irowoffset - 1] + 1) * (icol[1 + icoloffset - 1] + 1) / (mm + 2);
                n12 = irow[1 + irowoffset - 1] - n11;
                dlp = dlp - fact[n11] - fact[n12] - fact[icol[1 + icoloffset - 1] - n11] - fact[icol[2 + icoloffset - 1] - n12];
                return;
            }

            // Test for optimal table
            val = 0.0;
            xmin = false;
            if (irow[nrow + irowoffset - 1] <= irow[1 + irowoffset - 1] + ncol)
            {
                f10act(nrow, irow, irowoffset, ncol, icol, icoloffset, ref val, ref xmin, fact, lb, nu, nr);
            }
            if (!xmin)
            {
                if (icol[ncol + icoloffset - 1] <= icol[1 + icoloffset - 1] + nrow)
                {
                    f10act(ncol, icol, icoloffset, nrow, irow, irowoffset, ref val, ref xmin, fact, lb, nu, nr);
                }
            }

            if (xmin)
            {
                dlp = dlp - val;
                return;
            }

            // Setup for dynamic programming
            nn = mm;

            // Minimize ncol
            if (nrow >= ncol)
            {
                nro = nrow;
                nco = ncol;

                for (int i = 1; i <= nrow; i++)
                {
                    iro[i] = irow[i + irowoffset - 1];
                }

                ico[1] = icol[1 + icoloffset - 1];
                nt[1] = nn - ico[1];
                for (int i = 2; i <= ncol; i++)
                {
                    ico[i] = icol[i + icoloffset - 1];
                    nt[i] = nt[i - 1] - ico[i];
                }
            }
            else
            {
                nro = ncol;
                nco = nrow;

                ico[1] = irow[1 + irowoffset - 1];
                nt[1] = nn - ico[1];
                for (int i = 2; i <= nrow; i++)
                {
                    ico[i] = irow[i + irowoffset - 1];
                    nt[i] = nt[i - 1] - ico[i];
                }
                for (int i = 1; i <= ncol; i++)
                {
                    iro[i] = icol[i + icoloffset - 1];
                }
            }

            // Initialize pointers
            double vmn = 1.0e10;
            int nc1s = nco - 1;
            int irl = 1;
            int ks = 0;
            int ldst = 200;
            int k = ldst;
            int kyy = ico[nco] + 1;
            goto goto100;

            goto90:
            xmin = false;
            if (iro[nro - 1] <= iro[irl - 1] + nco)
            {
                f10act(nro, iro, irl, nco, ico, 1, ref val, ref xmin, fact, lb, nu, nr);
            }
            if (!xmin)
            {
                if (ico[nco - 1] <= ico[0] + nro)
                {
                    f10act(nco, ico, 1, nro, iro, irl, ref val, ref xmin, fact, lb, nu, nr);
                }
            }
            if (xmin)
            {
                if (val < vmn)
                {
                    vmn = val;
                }
                goto goto200;
            }

            goto100:
            int lev = 1;
            int nr1 = nro - 1;
            int nrt = iro[irl];
            int nct = ico[1];
            lb[1] = (int)((double)((nrt + 1) * (nct + 1)) / (double)(nn + nr1 * nc1s + 1) - tol) - 1;
            nu[1] = (int)((double)((nrt + nc1s) * (nct + nr1)) / (double)(nn + nr1 * nc1s)) - lb[1] + 1;
            nr[1] = nrt - lb[1];

            goto110:
            nu[lev]--;
            if (nu[lev] == 0)
            {
                if (lev == 1)
                {
                    goto goto200;
                }
                lev--;
                goto goto110;
            }
            lb[lev]++;
            nr[lev]--;

            goto120:
            alen[lev] = alen[lev - 1] + fact[lb[lev]];
            if (lev < nc1s)
            {
                nn1 = nt[lev];
                nrt = nr[lev];
                lev = lev + 1;
                nc1 = nco - lev;
                nct = ico[lev];
                lb[lev] = (int)((double)((nrt + 1) * (nct + 1)) / (double)(nn1 + nr1 * nc1 + 1) - tol);
                nu[lev] = (int)((double)((nrt + nc1) * (nct + nr1)) / (double)(nn1 + nr1 + nc1) - lb[lev] + 1);
                nr[lev] = nrt - lb[lev];
                goto goto120;
            }
            alen[nco] = alen[lev] + fact[nr[lev]];
            lb[nco] = nr[lev];

            double v = val + alen[nco];

            if (nro == 2)
            {
                v = v + fact[ico[1] - lb[1]] + fact[ico[2] - lb[2]];
                for (int i = 3; i <= nco; i++)
                {
                    v = v + fact[ico[i] - lb[i]];
                }
                if (v < vmn)
                {
                    vmn = v;
                }
            }
            else if (nro == 3 && nco == 2)
            {
                nn1 = nn - iro[irl] + 2;
                ic1 = ico[1] - lb[1];
                ic2 = ico[2] - lb[2];
                n11 = (iro[irl + 1] + 1) * (ic1 + 1) / nn1;
                n12 = iro[irl + 1] - n11;
                v = v + fact[n11] + fact[n12] + fact[ic1 - n11] + fact[ic2 - n12];
                if (v < vmn)
                {
                    vmn = v;
                }
            }
            else
            {
                for (int i = 1; i <= nco; i++)
                {
                    it[i] = ico[i] - lb[i];
                }
                if (nco == 2)
                {
                    if (it[1] > it[2])
                    {
                        ii = it[1];
                        it[1] = it[2];
                        it[2] = ii;
                    }
                }
                else if (nco == 3)
                {
                    ii = it[1];
                    if (ii > it[3])
                    {
                        if (ii > it[2])
                        {
                            if (it[2] > it[3])
                            {
                                it[1] = it[3];
                                it[3] = ii;
                            }
                            else
                            {
                                it[1] = it[2];
                                it[2] = it[3];
                                it[3] = ii;
                            }
                        }
                        else
                        {
                            it[1] = it[3];
                            it[3] = it[2];
                            it[2] = ii;
                        }
                    }
                    else if (ii > it[2])
                    {
                        it[1] = it[2];
                        it[2] = ii;
                    }
                    else if (it[2] > it[3])
                    {
                        ii = it[2];
                        it[2] = it[3];
                        it[3] = ii;
                    }
                }
                else
                {
                    Array.Sort(it);
                }

                key = it[1] * kyy + it[2];
                for (int i = 3; i <= nco; i++)
                {
                    key = it[i] + key * kyy;
                }

                ipn = key % ldst + 1;

                ii = ks + ipn;
                for (itp = ipn; itp <= ldst; ipn++)
                {
                    if (ist[ii] < 0)
                    {
                        goto goto180;
                    }
                    else if (ist[ii] == key)
                    {
                        goto goto190;
                    }
                    ii++;
                }

                ii = ks + 1;
                for (itp = 1; itp <= ipn - 1; itp++)
                {
                    if (ist[ii] < 0)
                    {
                        goto goto180;
                    }
                    else if (ist[ii] == key)
                    {
                        goto goto190;
                    }
                    ii++;
                }

                goto180:
                ist[ii] = key;
                stv[ii] = v;
                nst++;
                ii = nst + ks;
                itc[ii] = itp;
                goto goto110;

                goto190:
                stv[ii] = Math.Min(v, stv[ii]);
            }
            goto goto110;

            goto200:
            if (nitc > 0)
            {
                itp = itc[nitc + k] + k;
                nitc--;
                val = stv[itp];
                key = ist[itp];
                ist[itp] = -1;
                for (int i = nco; i >= 2; i--)
                {
                    ico[i] = key % kyy;
                    key = key / kyy;
                }
                ico[1] = key;

                nt[1] = nn - ico[1];
                for (int i = 2; i <= nco; i++)
                {
                    nt[i] = nt[i - 1] - ico[i];
                }
                goto goto90;
            }
            else if (nro > 2 && nst > 0)
            {
                nitc = nst;
                nst = 0;
                k = ks;
                ks = ldst - ks;
                nn = nn - iro[irl];
                irl++;
                nro--;
                goto goto200;
            }

            dlp = dlp - vmn;
        }

        static class gotos
        {
            public static void goto90(ref int nrow, ref int[] irow, ref int ncol, ref int[] icol, ref double dlp, ref int mm,
                            ref double[] fact, ref int[] ico, ref int[] iro, ref int[] it, ref int[] lb, ref int[] nr,
                            ref int[] nt, ref int[] nu, ref int[] itc, ref int[] ist, ref double[] stv, ref double[] alen,
                            ref double tol, ref bool xmin, ref int nro, ref int nco, ref int irl, ref double val, ref double vmn)
            {
                xmin = false;
                if (iro[nro-1] <= iro[irl-1]+nco)
                {
                    f10act(nro, iro, 1, nco, ico, 1, ref val, ref xmin, fact, lb, nu, nr);
                }
                if (!xmin)
                {
                    if (ico[nco-1] <= ico[0]+nro)
                    {
                        f10act(nco, ico, 1, nro, iro, 1, ref val, ref xmin, fact, lb, nu, nr);
                    }
                }
                if (xmin)
                {
                    if (val < vmn)
                    {
                        vmn = val;
                    }
                    goto200(ref nrow, ref irow, ref ncol, ref icol, ref dlp, ref mm,
                            ref fact, ref ico, ref iro, ref it, ref lb, ref nr,
                            ref nt, ref nu, ref itc, ref ist, ref stv, ref alen,
                            ref tol, ref xmin, ref nro, ref nco, ref irl, ref val, ref vmn);
                }
                goto100(ref nrow, ref irow, ref ncol, ref icol, ref dlp, ref mm,
                        ref fact, ref ico, ref iro, ref it, ref lb, ref nr,
                        ref nt, ref nu, ref itc, ref ist, ref stv, ref alen,
                        ref tol, ref xmin, ref nro, ref nco, ref irl, ref val, ref vmn);
            }
            public static void goto100(ref int nrow, ref int[] irow, ref int ncol, ref int[] icol, ref double dlp, ref int mm,
                            ref double[] fact, ref int[] ico, ref int[] iro, ref int[] it, ref int[] lb, ref int[] nr,
                            ref int[] nt, ref int[] nu, ref int[] itc, ref int[] ist, ref double[] stv, ref double[] alen,
                            ref double tol, ref bool xmin, ref int nro, ref int nco, ref int irl, ref double val, ref double vmn)
            {
            }
            public static void goto200(ref int nrow, ref int[] irow, ref int ncol, ref int[] icol, ref double dlp, ref int mm,
                            ref double[] fact, ref int[] ico, ref int[] iro, ref int[] it, ref int[] lb, ref int[] nr,
                            ref int[] nt, ref int[] nu, ref int[] itc, ref int[] ist, ref double[] stv, ref double[] alen,
                            ref double tol, ref bool xmin, ref int nro, ref int nco, ref int irl, ref double val, ref double vmn)
            {
            }
        }

        private static void f4xact(int nrow, int[] irow, int irowoffset, int ncol, int[] icol, int icoloffset, ref double dsp,
                            double[] fact, int[] icstkk, int[] ncstk, int[] lstk, int[] mstk,
                            int[] nstk, int[] nrstk, int[] irstkk, double[] ystk,
                            double tol)
        {
            int i = 1;
            int j = 1;
            int[][] irstk = new int[nrow + ncol + 1][];
            for (i = 1; i <= nrow; i++)
            {
                irstk[i] = new int[nrow + ncol + 1];
            }
            int[][] icstk = new int[nrow + ncol + 1][];
            for (i = 1; i <= ncol; i++)
            {
                icstk[i] = new int[nrow + ncol + 1];
            }

            if (nrow == 1)
            {
                for (i = 1; i <= ncol; i++)
                {
                    dsp = dsp - fact[icol[i + icoloffset - 1]];
                }
                return;
            }

            if (ncol == 1)
            {
                for (i = 1; i <= nrow; i++)
                {
                    dsp = dsp - fact[irow[i + irowoffset - 1]];
                }
                goto goto9000;
            }

            if (nrow * ncol == 4)
            {
                if (irow[2 + irowoffset - 1] <= icol[2 + icoloffset - 1])
                {
                    dsp = dsp - fact[irow[2 + irowoffset - 1]] - fact[icol[1 + icoloffset - 1]] - fact[icol[2 + icoloffset - 1] - irow[2 + irowoffset - 1]];
                }
                else
                {
                    dsp = dsp - fact[icol[2 + icoloffset - 1]] - fact[irow[1 + irowoffset - 1]] - fact[irow[2 + irowoffset - 1] - icol[2 + icoloffset - 1]];
                }
                goto goto9000;
            }

            // initialization before loop
            for (i = 1; i <= nrow; i++)
            {
                irstk[i][1] = irow[nrow - i + 1 + irowoffset - 1];
            }
            for (j = 1; j <= ncol; j++)
            {
                icstk[j][1] = icol[ncol - j + 1 + icoloffset - 1];
            }

            int nro = nrow;
            int nco = ncol;
            nrstk[1] = nro;
            ncstk[1] = nco;
            ystk[1] = 0.0;
            double y = 0.0;
            int istk = 1;
            int l = 1;
            double amx = 0.0;

            goto50:
            int ir1 = irstk[1][istk];
            int ic1 = icstk[1][istk];
            int m = 0;
            int n = 0;
            int k = 0;
            int mn = 0;
            if (ir1 > ic1)
            {
                if (nro >= nco)
                {
                    m = nco - 1;
                    n = 2;
                }
                else
                {
                    m = nro;
                    n = 1;
                }
            }
            else if (ir1 < ic1)
            {
                if (nro <= nco)
                {
                    m = nro - 1;
                    n = 1;
                }
                else
                {
                    m = nco;
                    n = 2;
                }
            }
            else
            {
                if (nro <= nco)
                {
                    m = nro - 1;
                    n = 1;
                }
                else
                {
                    m = nco - 1;
                    n = 2;
                }
            }

            goto60:
            if (n == 1)
            {
                i = l;
                j = 1;
            }
            else
            {
                i = 1;
                j = l;
            }

            int irt = irstk[i][istk];
            int ict = icstk[j][istk];
            mn = irt;
            if (mn > ict)
            {
                mn = ict;
            }
            y = y + fact[mn];
            if (irt == ict)
            {
                nro = nro - 1;
                nco = nco - 1;
                f11act(irstk[1], istk, i, nro, irstk[1], istk + 1);
                f11act(icstk[1], istk, j, nco, icstk[1], istk + 1);
            }
            else if (irt >= ict)
            {
                nco = nco - 1;
                f11act(icstk[1], istk, j, nco, icstk[1], istk + 1);
                f8xact(irstk[1], istk, irt - ict, i, nro, ref irstk[1], istk+1);
            }
            else
            {
                nro = nro - 1;
                f11act(irstk[1], istk, i, nro, irstk[1], istk + 1);
                f8xact(icstk[1], istk, ict - irt, j, nco, ref icstk[1], istk + 1);
            }

            if (nro == 1)
            {
                for (k = 1; k <= nco; k++)
                {
                    y = y + fact[icstk[k][istk + 1]];
                }
                goto goto90;
            }

            if (nco == 1)
            {
                for (k = 1; k <= nro; k++)
                {
                    y = y + fact[irstk[k][istk + 1]];
                }
                goto goto90;
            }

            lstk[istk] = l;
            mstk[istk] = m;
            nstk[istk] = n;
            istk++;
            nrstk[istk] = nro;
            ncstk[istk] = nco;
            ystk[istk] = y;
            l = 1;
            goto goto50;

            goto90:
            if (y > amx)
            {
                amx = y;
                if (dsp - amx <= tol)
                {
                    dsp = 0.0;
                    goto goto9000;
                }
            }

            goto100:
            istk--;
            if (istk == 0)
            {
                dsp = dsp - amx;
                if (dsp - amx <= tol)
                {
                    dsp = 0.0;
                }
                goto goto9000;
            }
            l = lstk[istk] + 1;

            goto110:
            if (l > mstk[istk])
            {
                goto goto100;
            }
            n = nstk[istk];
            nro = nrstk[istk];
            nco = ncstk[istk];
            y = ystk[istk];
            if (n == 1)
            {
                if (irstk[l][istk] < irstk[l - 1][istk])
                {
                    goto goto60;
                }
            }
            else if (n == 2)
            {
                if (icstk[l][istk] < icstk[l - 1][istk])
                {
                    goto goto60;
                }
            }

            l++;
            goto goto110;

            goto9000:
            return;
        }

        private static void f5xact(double pastp, double tol, int kval, ref int[] key, int keyoffset, int ldkey, ref int[] ipoin, int ipoinoffset,
                                   ref double[] stp, int stpoffset, int ldstp, ref int[] ifrq, int ifrqoffset, ref int[] npoin, int npoinoffset, ref int[] nr, int nroffset,
                                   ref int[] nl, int nloffset, int ifreq, ref int itop, bool ipsh, ref int itp)
        {
            int ird = 1;
            int ipn = 1;

            if (ipsh)
            {
                ird = (kval % ldkey) +1;

                for (itp = ird; itp <= ldkey; itp++)
                {
                    if (key[itp + keyoffset - 1] == kval)
                    {
                        goto goto40;
                    }
                    if (key[itp + keyoffset - 1] < 0)
                    {
                        goto goto30;
                    }
                }
                for (itp = 1; itp <= ird - 1; itp++)
                {
                    if (key[itp + keyoffset - 1] == kval)
                    {
                        goto goto40;
                    }
                    if (key[itp + keyoffset - 1] < 0)
                    {
                        goto goto30;
                    }
                }

                goto30:
                key[itp + keyoffset - 1] = kval;
                itop++;
                ipoin[itp + ipoinoffset - 1] = itop;
                if (itop > ldstp)
                    return;
                npoin[itop + npoinoffset - 1] = -1;
                nr[itop + nroffset - 1] = -1;
                nl[itop + nloffset - 1] = -1;
                stp[itop + stpoffset - 1] = pastp;
                ifrq[itop + ifrqoffset - 1] = ifreq;
                return;
            }
            goto40:
            ipn = ipoin[itp + ipoinoffset - 1];
            double test1 = pastp - tol;
            double test2 = pastp + tol;

            goto50:
            if (stp[ipn + stpoffset - 1] < test1)
            {
                ipn = nl[ipn + nloffset - 1];
                if (ipn > 0)
                {
                    goto goto50;
                }
            }
            else if (stp[ipn + stpoffset - 1] > test2)
            {
                ipn = nr[ipn + nroffset - 1];
                if (ipn > 0)
                {
                    goto goto50;
                }
            }
            else
            {
                ifrq[ipn + ifrqoffset - 1] = ifrq[ipn + ifrqoffset - 1] + ifreq;
                return;
            }

            itop++;
            if (itop > ldstp)
                return;

            ipn = ipoin[itp + ipoinoffset - 1];
            int itmp = ipn;
            goto60:
            if (stp[ipn + stpoffset - 1] < test1)
            {
                itmp = ipn;
                ipn = nl[ipn + nloffset - 1];
                if (ipn > 0)
                {
                    goto goto60;
                }
                else
                {
                    nl[itmp + nloffset - 1] = itop;
                }
            }
            else if (stp[ipn + stpoffset - 1] > test2)
            {
                itmp = ipn;
                ipn = nr[ipn + nroffset - 1];
                if (ipn > 0)
                {
                    goto goto60;
                }
                else
                {
                    nr[itmp + nroffset - 1] = itop;
                }
            }

            npoin[itop + npoinoffset - 1] = npoin[itmp + npoinoffset - 1];
            npoin[itmp + npoinoffset - 1] = itop;
            stp[itop + stpoffset - 1] = pastp;
            ifrq[itop + ifrqoffset - 1] = ifreq;
            nl[itop + nloffset - 1] = -1;
            nr[itop + nroffset - 1] = -1;
        }

        private static void f6xact(int nrow, ref int[] irow, ref int iflag, int[] kyy, ref int[] key, int keyoffset, int ldkey, ref int last, ref int ipn)
        {
            int kval;
            goto10:
            last++;
            if (last <= ldkey)
            {
                if (key[last + keyoffset - 1] < 0)
                {
                    goto goto10;
                }
                kval = key[last + keyoffset - 1];
                key[last + keyoffset - 1] = -9999;
                for (int j = nrow; j >= 2; j--)
                {
                    irow[j] = kval / kyy[j];
                    kval = kval - irow[j] * kyy[j];
                }
                irow[1] = kval;
                ipn = last;
            }
            else
            {
                last = 0;
                iflag = 3;
            }
        }

        private static void f7xact(int nrow, int[] imax, ref int[] idif, ref int k, ref int ks, ref int iflag)
        {
            int m = 0;
            int k1 = 0;
            int mm = 0;
            iflag = 0;

            if (ks == 0)
            {
                goto10:
                ks++;
                if (idif[ks] == imax[ks])
                {
                    goto goto10;
                }
            }

            goto20:
            if (idif[k] > 0 && k > ks)
            {
                idif[k] = idif[k] - 1;
                goto30:
                k--;
                if (imax[k] == 0)
                {
                    goto goto30;
                }
                m = k;
                goto40:
                if (idif[m] >= imax[m])
                {
                    m--;
                    goto goto40;
                }
                idif[m] = idif[m] + 1;

                if (m == ks)
                {
                    if (idif[m] == imax[m])
                    {
                        ks = k;
                    }
                }
            }
            else
            {
                goto50:
                for (k1 = k + 1; k1 <= nrow; k1++)
                {
                    if (idif[k1] > 0)
                    {
                        goto goto70;
                    }
                }
                iflag = 1;
                return;

                goto70:
                mm = 1;
                for (int i = 1; i <= k; i++)
                {
                    mm = mm + idif[i];
                    idif[i] = 0;
                }
                k = k1;
                goto90:
                k--;
                m = Math.Min(mm, imax[k]);
                idif[k] = m;
                mm = mm - m;
                if (mm > 0 && k != 1)
                {
                    goto goto90;
                }

                if (mm > 0)
                {
                    if (k1 != nrow)
                    {
                        k = k1;
                        goto goto50;
                    }
                    iflag = 1;
                    return;
                }

                idif[k1] = idif[k1] - 1;
                ks = 0;
                goto100:
                ks++;
                if (ks > k)
                {
                    return;
                }
                if (idif[ks] >= imax[ks])
                {
                    goto goto100;
                }
            }
        }

        private static void f8xact(int[] irow, int irowoffset, int iz, int i1, int izero, ref int[] noo, int noooffset)
        {
            int i = 0;
            for (i = 1; i <= i1 - 1; i++)
            {
                noo[i + noooffset - 1] = irow[i + irowoffset - 1];
            }
            for (i = i1; i <= izero - 1; i++)
            {
                if (iz >= irow[i + irowoffset - 1 + 1])
                {
                    goto goto30;
                }
                noo[i + noooffset - 1] = irow[i + irowoffset - 1 + 1];
            }

            i = izero;

            goto30:
            noo[i + noooffset - 1] = iz;
            goto40:
            i++;
            if (i > izero)
            {
                return;
            }
            noo[i + noooffset - 1] = irow[i + irowoffset - 1];
            goto goto40;
        }

        private static void f10act(int nrow, int[] irow, int irowoffset, int ncol, int[] icol, int icoloffset, ref double val, ref bool xmin, double[] fact, int[] nd, int[] ne, int[] m)
        {
            for (int i = 1; i <= nrow-1; i++)
            {
                nd[i] = 0;
            }
            int iz = icol[1 + icoloffset - 1] / nrow;
            ne[1] = iz;
            int ix = icol[1 + icoloffset - 1] - nrow * iz;
            m[1] = ix;
            if (ix != 0)
            {
                nd[ix] = nd[ix] + 1;
            }

            for (int i = 2; i <= ncol; i++)
            {
                ix = icol[i + icoloffset - 1] / nrow;
                ne[i] = ix;
                iz = iz + ix;
                ix = icol[i + icoloffset - 1] - nrow * ix;
                m[i] = ix;
                if (ix != 0)
                {
                    nd[ix] = nd[ix] + 1;
                }
            }

            for (int i = nrow - 2; i >= 1; i--)
            {
                nd[i] = nd[i] + nd[i + 1];
            }

            ix = 0;
            int nrw1 = nrow + 1;
            for (int i = nrow; i >= 2; i--)
            {
                ix = ix + iz + nd[nrw1 - i] - irow[i + irowoffset - 1];
                if (ix < 0)
                {
                    return;
                }
            }

            for (int i = 1; i <= ncol; i++)
            {
                ix = ne[i];
                iz = m[i];
                val = val + iz * fact[ix + 1] + (nrow - iz) * fact[ix];
            }
            xmin = true;
        }

        private static double gammds(double y, double p, int ifault)
        {
            double gammds = 0.0;
            int ifail = 1;
            double e = 1.0e-6;
            double zero = 0.0;
            double one = 1.0;

            ifault = 1;
            gammds = zero;

            if (y <= 0 || p <= 0)
                return gammds;

            ifault = 2;
            double f = Math.Exp(p * Math.Log(y) - alogam(p + one, ifail) - y);
            if (f == zero)
                return gammds;
            ifault = 0;
            double c = one;
            gammds = one;
            double a = p;
            goto10:
            a = a + one;
            c = c * y / a;
            gammds = gammds + c;
            if (c / gammds > e)
                goto goto10;
            gammds = gammds * f;
            return gammds;
        }

        private static double alogam(double x, int ifault)
        {
            double alogam = 0.0;

            double a1 = 0.918938533204673;
            double a2 = 0.00595238095238;
            double a3 = 0.00793650793651;
            double a4 = 0.002777777777778;
            double a5 = 0.08333333333333;

            double half = 0.5;
            double zero = 0.0;
            double one = 1.0;
            double seven = 7.0;

            alogam = zero;
            ifault = 1;
            if (x < zero)
                return alogam;
            ifault = 0;
            double y = x;
            double f = zero;
            if (y > seven)
                goto goto30;
            f = y;
            goto10:
            y = y + one;
            if (y >= seven)
                goto goto20;
            f = f * y;
            goto goto10;
            goto20:
            f = -1.0 * Math.Log(f);
            goto30:
            double z = one / (y * y);
            alogam = f + (y - half) * Math.Log(y) - y + a1 + (((-a2 * z + a3) * z - a4) * z + a5) / y;

            return alogam;
        }

        private static int maxTableCell(int nrow, int ncol, int[][] table)
        {
            int maxTableCell = 0;
            for (int r = 0; r < nrow; r++)
                for (int c = 0; c < ncol; c++)
                    if (table[r][c] > maxTableCell)
                        maxTableCell = table[r][c];
            return maxTableCell;
        }

        private static double f9xact(int n, int mm, int[] ir, int iroffset, double[] fact)
        {
            //System.out.println("f9xact: mm = " + mm);
            double f9xact = fact[mm];
            //System.out.println("f9xact: fact[mm] = " + fact[mm]);
            //System.out.println("f9xact: n = " + n);
            //System.out.println("f9xact: ir[n-1] = " + ir[n-1]);
            for (int k = 1; k <= n; k++)
            {
                f9xact = f9xact - fact[ir[k + iroffset - 1]];
            }
            return f9xact;
        }

        private static void f11act(int[] irow, int irowoffset, int i1, int i2, int[] noo, int noooffset)
        {
            for (int i = 1; i <= i1 - 1; i++)
            {
                noo[i + noooffset - 1] = irow[i + irowoffset - 1];
            }
            for (int i = i1; i <= i2; i++)
            {
                noo[i + noooffset - 1] = irow[i + irowoffset - 1 + 1];
            }
        }

        private static int iwork(int iwkmax, ref int iwkpt, int number, int itype)
        {
            int iwork = iwkpt;

            if (itype == 2 || itype == 3)
            {
                iwkpt = iwkpt + number;
            }
            else
            {
                if ((iwork % 2) != 0)
                {
                    iwork = iwork + 1;
                }
                iwkpt = iwkpt + 2 * number;
                iwork = iwork / 2;
            }

            if (iwkpt > iwkmax + 1)
            {
            }

            return iwork;
        }

        public static double FisherTest(System.Data.DataRow[] SortedRows, Boolean classic)
        {
            double P;
            double lnD;
            int[] Rs;
            int[] Cs;
            List<Node> nodes;
            List<Double> Ps;
            List<string> PathsAndPs;

            int[][] matrix = new int[SortedRows.Count()][];
            for (int i = 0; i < SortedRows.Count(); i++)
            {
                matrix[i] = new int[SortedRows[0].ItemArray.Length - 1];
                System.Data.DataRow dr = SortedRows.ElementAt(i);
                for (int j = 1; j < dr.ItemArray.Count(); j++)
                {
                    matrix[i][j-1] = int.Parse(SortedRows[i][j].ToString());
                }
            }
            if ((matrix.Length > 2 && matrix[0].Length > 2) || matrix.Length * matrix[0].Length > 14)
                return Double.NegativeInfinity;

            if (matrix.Length > matrix[0].Length)
                matrix = transposeMatrix(matrix);

            nodes = new List<Node>();
            Ps = new List<double>();
            PathsAndPs = new List<string>();

            int cells = matrix.Length * matrix[0].Length;
            int c = matrix[0].Length;
            int r = cells / c;

            double[] p = tableP(r, c, matrix);
            P = p[0];
            lnD = p[2];
            Rs = new int[r];
            Cs = new int[c];
            setRsAndCs(Rs, Cs, r, c, matrix);

            Node firstNode = new Node();
            firstNode.setStage(c);
            firstNode.setValues(Rs);
            nodes.Add(firstNode);

            nextStage(Rs, Cs, firstNode, nodes);
            int iterator = 1;
            while (nodes.Count() > iterator)
            {
                nextStage(Rs, Cs, nodes.ElementAt(iterator), nodes);
                iterator++;
            }

            buildPaths(Cs, Ps, PathsAndPs, P, lnD, nodes);

            return Ps.Sum();
        }

        static void buildPaths(int[] Cs, List<double> Ps, List<string> PathsAndPs, double P, double lnD, List<Node> nodes)
        {
            Node firstNode = nodes.ElementAt(0);
            String path = "";
            List<String> paths = new List<string>();
            double pathLength = 0.0;
            continuePath(Cs, Ps, PathsAndPs, P, lnD, nodes, firstNode, firstNode, path, 0, paths, pathLength);

//            System.out.println("Total Possible Paths: " + paths.size());
        }

        static void continuePath(int[] Cs, List<double> Ps, List<string> PathsAndPs, double P, double lnD, List<Node> nodes, Node previousNode, Node n, String path, int pathIteration, List<string> paths, double pathLength)
        {
            pathLength += segmentLength(Cs, previousNode, n);
            path = path + n.toString();
            if (n.getLeadsToSize() > 0)
            {
                for (int j = 0; j < n.getLeadsToSize(); j++)
                {
                    for (int i = 0; i < nodes.Count(); i++)
                    {
                        if (n.getLeadsToElementAt(j).Equals(nodes.ElementAt(i).getID()))
                        {
                            continuePath(Cs, Ps, PathsAndPs, P, lnD, nodes, n, nodes.ElementAt(i), path, pathIteration, paths, pathLength);
                            break;
                        }
                    }
                }
            }
            else
            {
                double pathP = Math.Exp(pathLength - lnD);
                if (pathP <= P)
                {
                    Ps.Add(pathP);
                    if (pathP == P)
                    {
                        PathsAndPs.Add(path + " -- " + pathP + "\t***");
                    }
                    else
                    {
                        PathsAndPs.Add(path + " -- " + pathP);
                    }
                }
                paths.Add(path + " " + pathP);
            }
        }

        static double segmentLength(int[] Cs, Node a, Node b)
        {
            if (a.equals(b))
                return 0.0;

            double numerator = lnFactorial(Cs[Cs.Length - a.getStage()]);
            double denominator = 0.0;
            int[] aValues = a.getValues();
            int[] bValues = b.getValues();
            for (int i = 0; i < aValues.Count(); i++)
            {
                denominator += lnFactorial(aValues[i] - bValues[i]);
            }
            return numerator - denominator;
        }

        static String matrixToString(int[][] matrix)
        {
            String matrixString = "";
            for (int r = 0; r < matrix.Length; r++)
            {
                if (r > 0)
                {
                    matrixString = matrixString + "\n";
                }
                for (int c = 0; c < matrix[0].Length; c++)
                {
                    matrixString = matrixString + "\t" + matrix[r][c];
                }
            }
            return matrixString;
        }

        public static void nextStage(int[] Rs, int[]Cs, Node node, List<Node> nodes)
        {
            int k = node.getStage();
            if (k == 0)
                return;
            int Ck = Cs[Cs.Length - k];
            int[] R = new int[node.getValues().Length];
            int S = 0;

            for (int i = Cs.Length - k + 1; i < Cs.Length; i++)
            {
                S += Cs[i];
            }

            bool keepLooping = true;
            bool[] subtracting = new bool[R.Length];
            int[] subtractvalues = new int[R.Length];
            subtracting[R.Length - 1] = true;

            while (keepLooping)
            {
                for (int i = 0; i < R.Length; i++)
                {
                    int Rik = node.getValues()[i];
                    int sigma = 0;
                    int sigmaForMax = 0;
                    for (int l = 0; l < i; l++)
                    {
                        sigma += (node.getValues()[l] - R[l]);
                        sigmaForMax += R[l];
                    }
                    int minValue = Math.Max(0, Rik - Ck + sigma);

                    int maxValue = Math.Min(node.getValues()[i], S - sigmaForMax);

                    R[i] = maxValue - subtractvalues[i];
                    if (subtracting[i])
                    {
                        subtractvalues[i]++;
                        if (maxValue - subtractvalues[i] < minValue)
                        {
                            if (i > 0)
                            {
                                subtractvalues[i] = 0;
                                subtracting[i] = false;
                                subtracting[i - 1] = true;
                            }
                            else
                            {
                                keepLooping = false;
                            }
                        }
                        else if (i < R.Length - 1)
                        {
                            subtracting[i] = false;
                            subtracting[i + 1] = true;
                        }
                    }
                }
                Node newNode = new Node();
                newNode.setStage(k - 1);
                newNode.setValues(R);

                bool addIt = false;
                for (int i = 0; i < nodes.Count(); i++)
                {
                    addIt = false;
                    if (newNode.equals(nodes.ElementAt(i)))
                    {
                        node.pushLeadsTo(nodes.ElementAt(i).getID());
                        break;
                    }
                    addIt = true;
                }
                if (addIt)
                {
                    node.pushLeadsTo(newNode.getID());
                    nodes.Add(newNode);
                }
            }
        }

        private static void setRsAndCs(int[] Rs, int[] Cs, int r, int c, int[][] mtx)
        {
            for (int j = 0; j < r; j++)
            {
                Rs[j] = 0;
            }
            for (int j = 0; j < c; j++)
            {
                Cs[j] = 0;
            }

            for (int j = 0; j < r; j++)
            {
                for (int i = 0; i < c; i++)
                {
                    Rs[j] += mtx[j][i];
                    Cs[i] += mtx[j][i];
                }
            }
        }

        private static int[][] transposeMatrix(int[][] matrix)
        {
            int cells = matrix.Length * matrix[0].Length;
            int c = matrix[0].Length;
            int r = cells / c;

            int[][] mtx = new int[c][];

            for (int j = 0; j < c; j++)
            {
                mtx[j] = new int[r];
                for (int i = 0; i < r; i++)
                {
                    mtx[j][i] = matrix[i][j];
                }
            }

            return mtx;
        }

        private static double[] tableP(int r, int c, int[][] mtx)
        {
            int[] R = new int[r];
            int[] C = new int[c];
            int T = 0;

            for (int j = 0; j < r; j++)
            {
                for (int i = 0; i < c; i++)
                {
                    R[j] += mtx[j][i];
                    C[i] += mtx[j][i];
                    T += mtx[j][i];
                }
            }

            double logD = lnD(r, T, R);
            double logN = lnN(r, c, mtx, C);

            double[] returnP = new double[3];
            returnP[0] = Math.Exp(logN - logD);
            returnP[1] = logN;
            returnP[2] = logD;

            return returnP;
        }
        private static double[] P(int r, int c, int[][] mtx)
        {
            return tableP(r, c, mtx);
        }

        private static double lnFactorial(int x)
        {
            double ln = 0.0;
            while (x > 1)
            {
                ln += Math.Log(x);
                x--;
            }
            return ln;
        }

        private static double lnD(int n, int[] rows)
        {
            double Dd = 0.0;

            for (int e = 0; e < n; e++)
            {
                Dd += lnFactorial(rows[e]);
            }

            return Dd;
        }
        private static double lnD(int n, int t, int[] rows)
        {
            double lnd = lnFactorial(t) - lnD(n, rows);
            return lnd;
        }

        private static double lnN(int r, int c, int[][] rxc, int[] cVector)
        {
            double lnN = 0.0;

            int[][] cxr = new int[c][];
            for (int j = 0; j < c; j++)
            {
                cxr[j] = new int[r];
                for (int i = 0; i < r; i++)
                {
                    cxr[j][i] = rxc[i][j];
                }
            }

            for (int i = 0; i < c; i++)
            {
                double numerator = lnFactorial(cVector[i]);
                double denominator = lnD(r, cxr[i]);
                lnN += numerator - denominator;
            }

            return lnN;
        }

        public static double CalcFisher(System.Data.DataRow[] SortedRows, Boolean classic)
        {
            double fortranPRE = FEXACT(SortedRows);
//            return fortranPRE;

            REngine engine = REngine.GetInstance();
            StringBuilder strb = new StringBuilder();
            strb.Append("c(");

            for (int i = 0; i < SortedRows.Length; i++)
            {
                for (int j = 1; j < SortedRows[0].ItemArray.Length; j++)
                {
                    strb.Append(SortedRows[i][j] + ",");
                }
            }
            int ncol = SortedRows[0].ItemArray.Length - 1;
            strb.Replace(',', ')', strb.Length - 1, 1);
            strb.Append(",byrow=TRUE,ncol=" + ncol);

            GenericVector fisherResults = engine.Evaluate("aggregates = matrix(" + strb.ToString() + ")\n" +
                "fisher.test(aggregates)").AsList();

            return fisherResults[0].AsNumeric().ToArray()[0];

        }

        public static double[] CalcChiSq(System.Data.DataRow[] SortedRows, Boolean classic)
        {
            double[] ChiSq = { 0.0, 0.0 };
            if (classic)
            {
                int SRLength = SortedRows.Length;
                int SRWidth = SortedRows[0].ItemArray.Length;
                double[] totals = new double[SRWidth];
                for (int i = 0; i < SRLength; i++)
                    for (int j = 1; j < SRWidth; j++)
                        totals[j] += (double)SortedRows[i][j];
                double[] ps = new double[SRWidth];
                for (int j = 2; j < SRWidth; j++)
                    ps[j] = totals[j] / totals[1];
                double[] observed = new double[SRLength * (SRWidth - 2)];
                double[] expected = new double[SRLength * (SRWidth - 2)];
                double[] OminusESqOverE = new double[SRLength * (SRWidth - 2)];
                int k = 0;
                for (int i = 0; i < SRLength; i++)
                    for (int j = 0; j < SRWidth - 2; j++)
                    {
                        observed[k] = (double)SortedRows[i][j + 2];
                        expected[k] = (double)SortedRows[i][1] * ps[j + 2];
                        OminusESqOverE[k] = Math.Pow(observed[k] - expected[k], 2.0) / expected[k];
                        ChiSq[0] += OminusESqOverE[k];
                        if (expected[k] < 1.0)
                            ChiSq[1] = 1.0;
                        if (expected[k] < 5.0 && ChiSq[1] == 0.0)
                            ChiSq[1] = 5.0;
                        k++;
                    }
            }
            else
            {
                int SRLength = SortedRows.Length;
                int SRWidth = SortedRows[0].ItemArray.Length;
                double[] totals = new double[SRWidth + 1];
                double[] rowtotals = new double[SRLength];
                for (int i = 0; i < SRLength; i++)
                {
                    for (int j = 2; j < SRWidth + 1; j++)
                    {
                        totals[j] += (double)SortedRows[i][j - 1];
                        rowtotals[i] += (double)SortedRows[i][j - 1];
                    }
                    totals[1] += rowtotals[i];
                }
                double[] ps = new double[SRWidth + 1];
                for (int j = 2; j < SRWidth + 1; j++)
                    ps[j] = totals[j] / totals[1];
                double[] observed = new double[SRLength * (SRWidth - 1)];
                double[] expected = new double[SRLength * (SRWidth - 1)];
                double[] OminusESqOverE = new double[SRLength * (SRWidth - 1)];
                int k = 0;
                for (int i = 0; i < SRLength; i++)
                    for (int j = 0; j < SRWidth - 1; j++)
                    {
                        observed[k] = (double)SortedRows[i][j + 1];
                        expected[k] = rowtotals[i] * ps[j + 2];
                        OminusESqOverE[k] = Math.Pow(observed[k] - expected[k], 2.0) / expected[k];
                        ChiSq[0] += OminusESqOverE[k];
                        if (expected[k] < 1.0)
                            ChiSq[1] = 1.0;
                        if (expected[k] < 5.0 && ChiSq[1] == 0.0)
                            ChiSq[1] = 5.0;
                        k++;
                    }
            }
            return ChiSq;
        }
    }
}
