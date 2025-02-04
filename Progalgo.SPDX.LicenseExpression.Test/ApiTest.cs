namespace Progalgo.SPDX.Test;

[TestClass]
public sealed class ApiTest
{
    [TestMethod]
    public void TestMethod1()
    {
        LicenseExpression expression = new("MIT OR Apache-2.0");
        Assert.IsTrue(expression.Evaluate(new AcceptedLicensesContext { "MIT" }));
        Assert.IsTrue(expression.Evaluate(new AcceptedLicensesContext { "Apache-2.0" }));
        Assert.IsTrue(expression.Evaluate(new AcceptedLicensesContext { "MIT", "Apache-2.0" }));
        Assert.IsFalse(expression.Evaluate(new AcceptedLicensesContext { "GPL-3.0" }));
    }

    class AcceptedLicensesContext : HashSet<string>, ILegalContext<bool>
    {
        public bool Evaluate(License license, IEnumerable<Exception> exceptions)
        {
            return this.Contains(license.Id);
        }

        public bool EvaluateOption(IEnumerable<bool> vals)
        {
            return vals.Any(x => x);
        }

        public bool EvaluateAnd(IEnumerable<bool> vals)
        {
            return vals.All(x => x);
        }
    }
}
