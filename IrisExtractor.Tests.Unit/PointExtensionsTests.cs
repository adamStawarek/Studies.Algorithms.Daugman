using System.Drawing;
using ImageEditor.Helpers;
using NUnit.Framework;

namespace IrisExtractor.Tests.Unit
{
    public class PointExtensionsTests
    {
        [Test]
        public void GetAngle_Returns_Approximately_0_When_First_Point_Is_00_And_Second_10()
        {
            var point1 = new Point(0, 0);
            var point2 = new Point(1, 0);

            var angle = point1.GetAngle(point2);

            Assert.AreEqual(0, angle, 1);
        }

        [Test]
        public void GetAngle_Returns_Approximately_45_When_First_Point_Is_00_And_Second_11()
        {
            var point1 = new Point(0, 0);
            var point2 = new Point(1, 1);

            var angle = point1.GetAngle(point2);

            Assert.AreEqual(45,angle,1);
        }

        [Test]
        public void GetAngle_Returns_Approximately_90_When_First_Point_Is_00_And_Second_01()
        {
            var point1 = new Point(0, 0);
            var point2 = new Point(0,1);

            var angle = point1.GetAngle(point2);

            Assert.AreEqual(90, angle, 1);
        }

        [Test]
        public void GetAngle_Returns_Approximately_180_When_First_Point_Is_00_And_Second_minus10()
        {
            var point1 = new Point(0, 0);
            var point2 = new Point(-1, 0);

            var angle = point1.GetAngle(point2);

            Assert.AreEqual(180, angle, 1);
        }

        [Test]
        public void GetAngle_Returns_Approximately_Minus90_When_First_Point_Is_00_And_Second_0minus1()
        {
            var point1 = new Point(0, 0);
            var point2 = new Point(0, -1);

            var angle = point1.GetAngle(point2);

            Assert.AreEqual(-90, angle, 1);
        }

        [Test]
        public void PolarToCartesian_Returns()
        {
            var angle = 22.6;
            var radius = 13;
            var expectedPoint=new Point(12,5);
            Assert.AreEqual(expectedPoint,PointExtensions.PolarToCartesian(angle,radius));
        }
    }
}
