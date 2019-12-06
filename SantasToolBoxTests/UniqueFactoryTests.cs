using Microsoft.VisualStudio.TestTools.UnitTesting;
using SantasToolbox;
using System;
using System.Linq;

namespace SantasToolBoxTests
{
    [TestClass]
    public class UniqueFactoryTests
    {
        [TestMethod]
        public void CallsConstructingFuncToGetOrCreateInstance()
        {
            // Arrange
            bool hasCalledConstructingFunc = false;
            Func<string, int> constructingFunc = _ => { hasCalledConstructingFunc = true; return 1; };
            var factory = new UniqueFactory<string, int>(constructingFunc);

            // Act
            var instance = factory.GetOrCreateInstance("1");

            // Assert
            Assert.IsTrue(hasCalledConstructingFunc);
        }

        [TestMethod]
        public void ConstructingFuncIsCalledOnceForSameId()
        {
            // Arrange
            int constructingFuncCount = 0;
            Func<string, object> constructingFunc = _ => { constructingFuncCount++; return new object(); };
            var factory = new UniqueFactory<string, object>(constructingFunc);

            // Act
            var instance1 = factory.GetOrCreateInstance("1");
            var instance2 = factory.GetOrCreateInstance("1");

            // Assert
            Assert.IsNotNull(instance1);
            Assert.IsNotNull(instance2);
            Assert.AreEqual(constructingFuncCount, 1);
        }

        [TestMethod]
        public void SameObjectIsReturnedForSameId()
        {
            // Arrange
            Func<string, object> constructingFunc = _ =>  new object();
            var factory = new UniqueFactory<string, object>(constructingFunc);

            // Act
            var instance1 = factory.GetOrCreateInstance("1");
            var instance2 = factory.GetOrCreateInstance("1");

            // Assert
            Assert.AreEqual(instance1, instance2);
        }

        [TestMethod]
        public void DifferetObjectIsReturnedForDifferentId()
        {
            // Arrange
            Func<string, object> constructingFunc = _ => new object();
            var factory = new UniqueFactory<string, object>(constructingFunc);

            // Act
            var instance1 = factory.GetOrCreateInstance("1");
            var instance2 = factory.GetOrCreateInstance("2");

            // Assert
            Assert.AreNotEqual(instance1, instance2);
        }

        [TestMethod]
        public void CreatedInstanceExistsInAllCreatedInstances()
        {
            // Arrange
            Func<string, object> constructingFunc = _ => new object();
            var factory = new UniqueFactory<string, object>(constructingFunc);

            // Act
            var instance1 = factory.GetOrCreateInstance("1");
            var instance2 = factory.GetOrCreateInstance("3");

            // Assert
            Assert.IsTrue(factory.AllCreatedInstances.Contains(instance1));
            Assert.IsTrue(factory.AllCreatedInstances.Contains(instance2));
        }

        [TestMethod]
        public void AllCreatedInstancesDoesNotStoreDuplicateObjects()
        {
            // Arrange
            Func<string, object> constructingFunc = _ => new object();
            var factory = new UniqueFactory<string, object>(constructingFunc);

            // Act
            var instance1 = factory.GetOrCreateInstance("1");
            var instance2 = factory.GetOrCreateInstance("1");
            var instance3 = factory.GetOrCreateInstance("3");

            // Assert
            Assert.AreEqual(factory.AllCreatedInstances.Count, 2);
        }
    }
}
