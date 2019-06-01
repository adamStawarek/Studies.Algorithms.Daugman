using System;
using System.Collections.Generic;
using System.Text;
using ImageEditor.Helpers;
using NUnit.Framework;

namespace IrisExtractor.Tests.Unit
{
    [TestFixture]
    public class StringExtensionsTests
    {
        [Test]
        public void TrimRightFromChar_Returns_String_Without_Word_After_Last_Backslash()
        {
            var path = @"C:\Users\adams\Desktop\biometrics2\UBIRIS_200_150_R\Sessao_1\1\Img_1_1_5.jpg";
            var expected = @"C:\Users\adams\Desktop\biometrics2\UBIRIS_200_150_R\Sessao_1\1";
            var result = path.TrimRightFromChar();
            Assert.AreEqual(expected,result);
        }
    }
}
