namespace Progalgo.SPDX.LicenseExpression.ParserTests

open Microsoft.VisualStudio.TestTools.UnitTesting
open Progalgo.SPDX.Parser

[<TestClass>]
type Parser_Should () =

    let test input =
        match FParsec.CharParsers.run licenseExpr input with
        | FParsec.CharParsers.Success(result, _, _) -> Ok result
        | FParsec.CharParsers.Failure(errorMsg, _, _) -> Error errorMsg

    [<TestMethod>]
    member this.TestSimpleLicenseId () =
        let input = "MIT"
        let expected = (With(LicenseId "MIT", None))
        match test input with
            | Ok result -> Assert.AreEqual<LicenseExpression>(expected, result)
            | Error msg -> Assert.Fail(msg)

    [<TestMethod>]
    member this.TestLicenseIdWithException () =
        let input = "MIT WITH Classpath"
        let expected = With(LicenseId "MIT", Some (Exception "Classpath"))
        match test input with
            | Ok result -> Assert.AreEqual<LicenseExpression>(expected, result)
            | Error msg -> Assert.Fail(msg)

    // [<TestMethod>]
    // member this.TestLicenseIdWithPlus () =
    //     let input = "GPL-2.0+"
    //     let expected = License(LicenseId "GPL-2.0+")
    //     match test input with
    //         | Ok result -> Assert.AreEqual<ExpressionTree>(expected, result)
    //         | Error msg -> Assert.Fail(msg)

    [<TestMethod>]
    member this.TestLicenseReference () =
        let input = "LicenseRef-1234"
        let expected = With(LicenseRef "LicenseRef-1234", None)
        match test input with
            | Ok result -> Assert.AreEqual<LicenseExpression>(expected, result)
            | Error msg -> Assert.Fail(msg)

    [<TestMethod>]
    member this.TestCompoundExpressionWithAnd () =
        let input = "MIT AND GPL-2.0"
        let expected = justLicense "MIT" &&& justLicense "GPL-2.0"
        match test input with
            | Ok result -> Assert.AreEqual<LicenseExpression>(expected, result)
            | Error msg -> Assert.Fail(msg)

    [<TestMethod>]
    member this.TestCompoundExpressionWithOr () =
        let input = "MIT OR GPL-2.0"
        let expected = justLicense "MIT" ||| justLicense "GPL-2.0"
        match test input with
            | Ok result -> Assert.AreEqual<LicenseExpression>(expected, result)
            | Error msg -> Assert.Fail(msg)

    [<TestMethod>]
    member this.TestNestedCompoundExpression () =
        let input = "(MIT AND GPL-2.0) OR Apache-2.0"
        let expected = (justLicense "MIT" &&& justLicense "GPL-2.0") ||| justLicense "Apache-2.0"
        match test input with
            | Ok result -> Assert.AreEqual<LicenseExpression>(expected, result)
            | Error msg -> Assert.Fail(msg)

    [<TestMethod>]
    member this.TestComplexExpressionWithException () =
        let input = "MIT AND (GPL-2.0 WITH Classpath)"
        let expected = justLicense "MIT" &&& With (LicenseId "GPL-2.0", (Some (Exception "Classpath")))
        match test input with
            | Ok result -> Assert.AreEqual<LicenseExpression>(expected, result)
            | Error msg -> Assert.Fail(msg)
