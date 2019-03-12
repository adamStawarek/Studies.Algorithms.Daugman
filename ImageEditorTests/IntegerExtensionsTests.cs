using System;
using System.Drawing;
using ImageEditor.Helpers;
using NUnit.Framework;

namespace ImageEditorTests
{
    [TestFixture]
    public class IntegerExtensionsTests
    {
        [Test]
        public void GetPointMatrixAroundTheCenterTest_When_Center_Is_11_Returns_00_10_20___01_11_2_1___02_12_22()
        {
            Point center=new Point(1,1);
            var matrix = center.GetPointMatrixAroundTheCenter(offset:1);
            var expectedMatrix=new Point[3,3]
            {
                {new Point(0,0),new Point(1,0),new Point(2,0)   },
                {new Point(0,1),new Point(1,1),new Point(2,1)   },
                {new Point(0,2),new Point(1,2),new Point(2,2)   }
            };
            Assert.AreEqual(expectedMatrix,matrix);
        }
    }
}
