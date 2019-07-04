using System;
using System.Reflection;

namespace myXunit
{
    class Program
    {
        static void Main(string[] args)
        {
            new TestCaseTest("TestTemplateMethod").Run();
            new TestCaseTest("TestResult").Run();
            new TestCaseTest("TestFailedResultFormatting").Run();
            new TestCaseTest("TestFailedResult").Run();
            new TestCaseTest("TestFailedSetUpFormatting").Run();
            new TestCaseTest("TestFailedSetUp").Run();
            new TestCaseTest("TestTearDownWhenTestFailed").Run();
        }
    }

    class TestCaseTest : TestCase
    {
        private WasRun test;

        public void TestTemplateMethod()
        {
            test = new WasRun("TestMethod");
            test.Run();
            Console.WriteLine($"TestTemplateMethod: {"SetUp TestMethod TearDown" == test.Log}");
        }
        public void TestResult()
        {
            test = new WasRun("TestMethod");
            var result = test.Run();
            Console.WriteLine($"TestResult: {"1 run, 0 failed" == result.Summary()}");
        }
        public void TestFailedResult()
        {
            test = new WasRun("TestBrokenMethod");
            var result = test.Run();
            Console.WriteLine($"TestFailedResult: {"1 run, 1 failed" == result.Summary()}");
        }
        public void TestFailedResultFormatting()
        {
            var result = new TestResult();
            result.TestStarted();
            result.TestFailed();
            Console.WriteLine($"TestFailedResultFormatting: {"1 run, 1 failed" == result.Summary()}");
        }
        public void TestFailedSetUpFormatting()
        {
            var result = new TestResult();
            var error = new Exception();
            result.SetUpFailed(error);
            Console.WriteLine($"TestFailedSetUpFormatting: {$"SetUp is Failed: {error.Message}" == result.Summary()}");
        }
        public void TestFailedSetUp()
        {
            test = new BrokenSetUp("");
            var result = test.Run();
            Console.WriteLine($"TestFailedSetUp: {result.Summary().StartsWith("SetUp is Failed:")}");
        }
        public void TestTearDownWhenTestFailed()
        {
            test = new WasRun("TestBrokenMethod");
            test.Run();
            Console.WriteLine($"TestTearDownWhenTestFailed: {test.Log.Contains("TearDown")}");
        }

        public TestCaseTest(string methodName) : base(methodName) {}
    }

    public class TestResult
    {
        public int RunCount { get; set; } = 0;
        public int ErrorCount { get; set; } = 0;
        public bool IsSetUpError { get; set; } = false;
        public string ErrorMessage { get; set; } = "";
        public void TestStarted()
        {
            RunCount++;
        }
        public string Summary()
        {
            if (!string.IsNullOrEmpty(ErrorMessage)) { return ErrorMessage; }
            
            return $"{RunCount} run, {ErrorCount} failed";
        }

        internal void TestFailed()
        {
            ErrorCount++;
        }

        public void SetUpFailed(Exception ex)
        {
            ErrorMessage = $"SetUp is Failed: {ex.Message}";
        }
    }
    public class BrokenSetUp: WasRun
    {
        public BrokenSetUp(string methodName) : base(methodName) { }
        public override void SetUp()
        {
            throw new Exception();
        }
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
        public void TestBrokenMethod()
        {
            throw new Exception();
        }
    }

    public abstract class TestCase
    {
        public string Name { get; set; } = "";
        public TestCase(string methodName) { Name = methodName; }

        public virtual void SetUp() { }
        public virtual void TearDown() { }
        public　TestResult Run()
        {
            var result = new TestResult();
            result.TestStarted();
            try
            {
                SetUp();
            }
            catch (Exception ex)
            {
                result.SetUpFailed(ex);
                return result;
            }
            try
            {
                Type.GetType(GetType().FullName).GetMethod(Name).Invoke(this, new object[] { });
            }
            catch
            {
                result.TestFailed();
            }
            TearDown();
            return result;
        }
    }
}
