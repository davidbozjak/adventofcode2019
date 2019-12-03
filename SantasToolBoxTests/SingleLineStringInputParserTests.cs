using Microsoft.VisualStudio.TestTools.UnitTesting;
using SantasToolbox;

namespace SantasToolBoxTests
{
    [TestClass]
    public class SingleLineStringInputParserTests
    {
        [TestMethod]
        public void ReturnsFalseOnSingleNullInput()
        {
            // Arrange
            var parser = new SingleLineStringInputParser<int>(int.TryParse);
            int value;

            // Act, Assert
            Assert.IsFalse(parser.GetValue(null, out value));
        }

        [TestMethod]
        public void ReturnsFalseOnSingleEmptyStringInput()
        {
            // Arrange
            var parser = new SingleLineStringInputParser<int>(int.TryParse);
            int value;

            // Act, Assert
            Assert.IsFalse(parser.GetValue(string.Empty, out value));
        }

        [TestMethod]
        public void ReturnsFalseOnSingleNonIntInput()
        {
            // Arrange
            var parser = new SingleLineStringInputParser<int>(int.TryParse);
            int value;

            // Act, Assert
            Assert.IsFalse(parser.GetValue("abc", out value));
        }

        [TestMethod]
        public void ParsesSingleInput()
        {
            // Arrange
            var parser = new SingleLineStringInputParser<int>(int.TryParse);
            int value;

            // Act, Assert
            Assert.IsTrue(parser.GetValue("1", out value));
            Assert.AreEqual(value, 1);
        }

        [TestMethod]
        public void ParsesSingleInputWithWhitespace()
        {
            // Arrange
            var parser = new SingleLineStringInputParser<int>(int.TryParse);
            int value;

            // Act, Assert
            Assert.IsTrue(parser.GetValue(" 1 ", out value));
            Assert.AreEqual(value, 1);
        }

        [TestMethod]
        public void ParsesSingleInputWithDelimiter()
        {
            // Arrange
            var parser = new SingleLineStringInputParser<int>(int.TryParse);
            int value;

            // Act, Assert

            Assert.IsTrue(parser.GetValue("1,", out value));
            Assert.AreEqual(value, 1);
        }

        [TestMethod]
        public void ParsesSingleInputWithDelimiterAndWhitespace()
        {
            // Arrange
            var parser = new SingleLineStringInputParser<int>(int.TryParse);
            int value;

            // Act, Assert

            Assert.IsTrue(parser.GetValue(" 1, ", out value));
            Assert.AreEqual(value, 1);
        }

        [TestMethod]
        public void ReturnsFalseAfterDepleted()
        {
            // Arrange
            var parser = new SingleLineStringInputParser<int>(int.TryParse);
            int value;

            // Act, Assert
            Assert.IsTrue(parser.GetValue("1", out value));
            Assert.IsFalse(parser.GetValue(null, out value));
        }

        [TestMethod]
        public void InputLineWith3IntsReads3Ints()
        {
            // Arrange
            var parser = new SingleLineStringInputParser<int>(int.TryParse);
            int value;

            // Act, Assert
            parser.GetValue("1, 2, 3", out value);
            Assert.AreEqual(value, 1);

            parser.GetValue(null, out value);
            Assert.AreEqual(value, 2);

            parser.GetValue(null, out value);
            Assert.AreEqual(value, 3);

            Assert.IsFalse(parser.GetValue(null, out value));
        }

        [TestMethod]
        public void ReturnsSeveralConsecutiveSingleInputs()
        {
            // Arrange
            var parser = new SingleLineStringInputParser<int>(int.TryParse);
            int value;

            // Act, Assert
            parser.GetValue("1", out value);
            Assert.AreEqual(value, 1);

            parser.GetValue("2", out value);
            Assert.AreEqual(value, 2);

            Assert.IsFalse(parser.GetValue(null, out value));
        }

        [TestMethod]
        public void ReturnsSeveralConsecutiveSingleInputsWithDelimiters()
        {
            // Arrange
            var parser = new SingleLineStringInputParser<int>(int.TryParse);
            int value;

            // Act, Assert
            parser.GetValue("1,", out value);
            Assert.AreEqual(value, 1);

            parser.GetValue("2, ", out value);
            Assert.AreEqual(value, 2);

            Assert.IsFalse(parser.GetValue(null, out value));
        }

        [TestMethod]
        public void ReturnsSeveralConsecutiveSingleInputsWithWhitespace()
        {
            // Arrange
            var parser = new SingleLineStringInputParser<int>(int.TryParse);
            int value;

            // Act, Assert
            parser.GetValue("1,2", out value);
            Assert.AreEqual(value, 1);

            parser.GetValue("3, ", out value);
            Assert.AreEqual(value, 2);

            Assert.IsTrue(parser.GetValue(null, out value));
            Assert.AreEqual(value, 3);

            Assert.IsFalse(parser.GetValue(null, out value));
        }
    }
}
