using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using mathProject;

namespace TestMathProject
{
    [TestClass]
    public class MathTest
    {
        [TestMethod]
        public void TestTriangleArea()
        {
            double a = 1;
            double b = 1;
            Assert.AreEqual(MyMath.triangleArea(a,b), 0.5, "Square is not correct");
        }
    }
}
