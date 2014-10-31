using Toaster;

namespace BuildContentTests
{
    [Order(100)]
    [TestClass]
    public class AuthTests
    {
        private static TestContext testContext;

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            AuthTests.testContext = testContext;
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
        }

        [Order(100)]
        [TestMethod]
        public void TestContent1()
        {

        }

        [Order(110)]
        [TestMethod]
        public void TestContent2()
        {
        }

        [Order(120)]
        [TestMethod]
        public void TestCircularDependency()
        {
        }
    }
}
