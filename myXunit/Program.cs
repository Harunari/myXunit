using System;
using System.Reflection;

namespace myXunit
{
    class Program
    {
        static void Main(string[] args)
        {
            new TestCaseTest("TestTemplateMethod").Run();
        }
    }

    class TestCaseTest : TestCase
    {
        private WasRun test;

        public void TestTemplateMethod()
        {
            test = new WasRun("TestMethod");
            test.Run();
            Console.WriteLine($"setup log test: {"SetUp TestMethod TearDown" == test.Log}");
        }

        public TestCaseTest(string methodName) : base(methodName) {}
    }

    public class WasRun : TestCase
    {
        public WasRun(string methodName) : base(methodName) { }

        public string Log { get; set; } = "";

        public override void SetUp()
        {
            Log = "SetUp ";
        }
        public void TestMethod()
        {
            Log += "TestMethod ";
        }
        public override void TearDown()
        {
            Log += "TearDown";
        }
    }

    public abstract class TestCase
    {
        public string Name { get; set; } = "";
        public TestCase(string methodName) { Name = methodName; }

        public virtual void SetUp() { }
        public virtual void TearDown() { }
        public void Run()
        {
            SetUp();
            Type.GetType(GetType().FullName).GetMethod(Name).Invoke(this, new object[] { });
            TearDown();
        }
    }
}
