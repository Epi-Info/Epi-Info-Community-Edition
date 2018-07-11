using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class CreateMapImage
    {
        [TestMethod]
        public void Create()
        {
            EpiDashboard.Common common = new EpiDashboard.Common();
            common.CreateMapImage(@"C:\_CODE\Epi-Info-Community-Edition\Build\Debug\Projects\Sample\chi.map7");
        }
    }
}
