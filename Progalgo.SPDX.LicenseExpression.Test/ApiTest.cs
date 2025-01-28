namespace Progalgo.SPDX.Test;

[TestClass]
public sealed class ApiTest
{
    [TestMethod]
    public void TestMethod1()
    {
        LicenseExpression expression = new("MIT OR Apache-2.0");
    }

    class TestLegalScope : LegalScope<bool>
    {
        public override bool Evaluate(License license, IEnumerable<Exception> exceptions)
        {
            return true;
        }

        public override bool EvaluateOption(IEnumerable<bool> vals)
        {
            return vals.Any(x => x);
        }

        public override bool EvaluateAnd(IEnumerable<bool> vals)
        {
            return vals.All(x => x);
        }
    }
}
