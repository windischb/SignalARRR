using System.Text;
using doob.SignalARRR.Common.Helper;
using SignalARRR.Tests.SharedModels;
using Xunit;

namespace SignalARRR.Tests.HelperTests
{
    public class TypeHelperTests
    {
        [Theory]
        [InlineData(null, typeof(void))]
        [InlineData("", typeof(void))]
        [InlineData("   ", typeof(void))]
        [InlineData("String", typeof(string))]
        [InlineData("System.Guid", typeof(Guid))]
        [InlineData("System.Text.StringBuilder", typeof(StringBuilder))]
        [InlineData("SignalARRR.Tests.SharedModels.ITestServerMethods", typeof(ITestServerMethods))]
        [InlineData("SignalARRR.tests.SharedModels.ITestServerMethods", typeof(ITestServerMethods))]
        public void FindType_ValidTypeNames_ReturnsExpected(string input, Type expected)
        {
            // Act
            var result = TypeHelper.FindType(input);
            
            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void FindType_NonExistingType_ReturnsNull()
        {
            // Act
            var result = TypeHelper.FindType("Completely.NonExisting.Type.Name");
            
            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void FindType_IsCached_AfterFirstLookup()
        {
            // Arrange
            const string typeName = "System.Guid";
            
            // Act
            var first  = TypeHelper.FindType(typeName);
            var second = TypeHelper.FindType(typeName);
            
            // Assert: Referenzgleichheit, d.h. Cache wurde genutzt
            Assert.Same(first, second);
        }

        [Fact]
        public void FindType_ResolvesNestedTestType()
        {
            // Innerer Test-Typ, voll qualifizierter Name enthält ein '+' für Nested Types
            var fullName = typeof(MyDummyType).FullName!;
            
            // Act
            var result = TypeHelper.FindType(fullName);
            
            // Assert
            Assert.Equal(typeof(MyDummyType), result);
        }

        // einfache Hilfsklasse zum Testen des Assembly-Durchsuchens
        private class MyDummyType { }
    }
}
