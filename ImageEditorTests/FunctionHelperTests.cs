using ImageEditor.Helpers;
using NUnit.Framework;
using OxyPlot;

namespace ImageEditorTests
{
    [TestFixture]
    class FunctionHelperTests
    {
        [Test]
        public void GetThirdPointYValue_Returns_3_21_Given_1_11_And_2_16()
        {
            var p1 = new DataPoint(1, 11);
            var p2=new DataPoint(2,16);
            var result = FunctionHelper.GetThirdPointYValue(p1, p2, 3);
            Assert.AreEqual(21,result);
        }

        [Test]
        public void GetThirdPointYValue_Returns_3_3_Given_0_0_And_255_255()
        {
            var p1 = new DataPoint(0,0);
            var p2 = new DataPoint(255, 255);
            var result = FunctionHelper.GetThirdPointYValue(p1, p2, 3);
            Assert.AreEqual(3, result);
        }
    }
}
