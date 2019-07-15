using System;
using System.Collections.Generic;
using System.Reflection;

namespace myXunit
{
    class Program
    {
        static void Main(string[] args)
        {
            var suite = new TestSuite();
            suite.Add(new TestCaseTest("TestTemplateMethod"));
            suite.Add(new TestCaseTest("TestResult"));
            suite.Add(new TestCaseTest("TestFailedResult"));
            suite.Add(new TestCaseTest("TestFailedResultFormatting"));
            suite.Add(new TestCaseTest("TestSuite"));
            var result = new TestResult();
            suite.Run(result);
            Console.WriteLine(result.Summary());
        }
    }

    class TestCaseTest : TestCase
    {
        private WasRun test;
        private readonly TestResult result = new TestResult();

        public void TestTemplateMethod()
        {
            test = new WasRun("TestMethod");
            test.Run(result);
            if ("SetUp TestMethod TearDown" == test.Log) { return; }
            throw new AssertException();
        }
        public void TestResult()
        {
            test = new WasRun("TestMethod");
            test.Run(result);
            if ("1 run, 0 failed" == result.Summary()) { return; }
            throw new AssertException();
        }
        public void TestFailedResult()
        {
            test = new WasRun("TestBrokenMethod");
            test.Run(result);
            if ("1 run, 1 failed" == result.Summary()) { return; }
            throw new AssertException();
        }
        public void TestFailedResultFormatting()
        {
            result.TestStarted();
            result.TestFailed();
            if ("1 run, 1 failed" == result.Summary()) { return; }
            throw new AssertException();
        }
        public void TestFailedSetUpFormatting()
        {
            var error = new Exception();
            result.SetUpFailed(error);
            if ($"SetUp is Failed: {error.Message}" == result.Summary()) { return; }
            throw new AssertException();
        }
        public void TestFailedSetUp()
        {
            test = new BrokenSetUp("");
            test.Run(result);
            if (result.Summary().StartsWith("SetUp is Failed:")) { return; }
            throw new AssertException();
        }
        public void TestTearDownWhenTestFailed()
        {
            test = new WasRun("TestBrokenMethod");
            test.Run(result);
            if (test.Log.Contains("TearDown")) { return; }
            throw new AssertException();
        }
        public void TestSuite()
        {
            var suite = new TestSuite();
            suite.Add(new WasRun("TestMethod"));
            suite.Add(new WasRun("TestBrokenMethod"));
            suite.Run(result);
            if ("2 run, 1 failed" == result.Summary()) { return; }
            throw new AssertException();
        }

        public TestCaseTest(string methodName) : base(methodName) { }
    }

    internal class TestSuite
    {
        List<TestCase> Tests { get; } = new List<TestCase>();

        internal void Add(TestCase test)
        {
            Tests.Add(test);
        }

        internal void Run(TestResult result)
        {
            foreach (var test in Tests)
            {
                test.Run(result);
            }
        }
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
    public class BrokenSetUp : WasRun
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
        public void Run(TestResult result)
        {
            result.TestStarted();
            try
            {
                SetUp();
            }
            catch (Exception ex)
            {
                result.SetUpFailed(ex);
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
        }
    }


    public class AssertException : Exception
    {
        public AssertException() { }
        public AssertException(string message) : base(message) { }
        public AssertException(string message, Exception inner) : base(message, inner) { }
        protected AssertException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
