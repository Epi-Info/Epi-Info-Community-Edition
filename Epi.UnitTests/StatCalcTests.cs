using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Epi.Windows;
using EpiDashboard;
using Epi.Core;
using StatisticsRepository;

namespace Epi.UnitTests
{
    [TestClass]
    public class StatCalcTests : StatisticsTests
    {
        [TestMethod]
        public void BinomialTest()
        {
            double ltp = 0;
            double lep = 0;
            double eqp = 0;
            double gep = 0;
            double gtp = 0;
            double ptt = 0;
            int lcl = 0;
            int ucl = 0;

            var reader = new StreamReader(File.OpenRead(@"..\..\Data\BinomialTests.csv"));
            int linesinfile = 5000;
            int[] ms = new int[linesinfile];
            int[] ns = new int[linesinfile];
            double[] ps = new double[linesinfile];
            double[] ltps = new double[linesinfile];
            double[] leps = new double[linesinfile];
            double[] eqps = new double[linesinfile];
            double[] geps = new double[linesinfile];
            double[] gtps = new double[linesinfile];
            double[] ptts = new double[linesinfile];
            int[] lcls = new int[linesinfile];
            int[] ucls = new int[linesinfile];
            int i = 0;
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(',');
                bool rv = int.TryParse(values[0], out ms[i]);
                rv = int.TryParse(values[1], out ns[i]);
                rv = Double.TryParse(values[2], out ps[i]);
                rv = Double.TryParse(values[3], out ltps[i]);
                rv = Double.TryParse(values[4], out leps[i]);
                rv = Double.TryParse(values[5], out eqps[i]);
                rv = Double.TryParse(values[6], out geps[i]);
                rv = Double.TryParse(values[7], out gtps[i]);
                rv = Double.TryParse(values[8], out ptts[i]);
                rv = int.TryParse(values[9], out lcls[i]);
                rv = int.TryParse(values[10], out ucls[i]);
                i++;
                if (i == linesinfile)
                    break;
            }

            float divisor = 1000f;
            System.Diagnostics.Trace.Write("j\t m\t n\t   p\tl\tu\n");
            for (int j = 0; j < i; j++)
            {
                if (Math.Round((double)j / divisor) != (double)j / divisor)
                    continue;
                StatisticsRepository.Strat2x2 calculator = new StatisticsRepository.Strat2x2();
                calculator.binpdf(ns[j], ms[j], ps[j], ref ltp, ref lep, ref eqp, ref gep, ref gtp, ref ptt, ref lcl, ref ucl);

                System.Diagnostics.Trace.Write(j + "\t" + ms[j] + "\t" + ns[j] + "\t" + ps[j] + "\t" + (lcls[j] - lcl) + "\t" + (ucls[j] - ucl) + "\n");

                Assert.AreEqual(lep, leps[j], 0.000001);
                Assert.AreEqual(eqp, eqps[j], 0.000001);
                Assert.AreEqual(gep, geps[j], 0.000001);
                Assert.AreEqual(gtp, gtps[j], 0.000001);
                Assert.AreEqual(ptt, ptts[j], 0.000001);
                Assert.AreEqual(lcl, lcls[j], 0.000001);
                Assert.AreEqual(ucl, ucls[j], 1);
            }
        }
    }
}
