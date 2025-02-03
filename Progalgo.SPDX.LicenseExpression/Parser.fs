namespace Progalgo.SPDX

open System.Runtime.CompilerServices

module Parser =

    [<assembly: InternalsVisibleTo("Progalgo.SPDX.LicenseExpression.ParserTest")>]
    do ()

    open FParsec
    
// SPDX License Expression grammar in ABNF
//
// Source: https://spdx.github.io/spdx-spec/v2-draft/SPDX-license-expressions/
//
// idstring = 1*(ALPHA / DIGIT / "-" / "." )
// license-id = <short form license identifier in Annex A.1>
// license-exception-id = <short form license exception identifier in Annex A.2>
// license-ref = ["DocumentRef-"(idstring)":"]"LicenseRef-"(idstring)
// simple-expression = license-id / license-id"+" / license-ref
// compound-expression = (simple-expression /
//
// simple-expression "WITH" license-exception-id /
//   compound-expression "AND" compound-expression /
//   compound-expression "OR" compound-expression /
//   "(" compound-expression ")" )
// license-expression = (simple-expression / compound-expression)

    let idstring = many1Satisfy (fun c -> isLetter c || isDigit c || c = '-' || c = '.')
    
    type License =
        | LicenseId of string
        | LicenseRef of string

    type Exception = Exception of string

    type LicenseExpression =
        | With of License * Exception Option
        | And of LicenseExpression * LicenseExpression
        | Or of LicenseExpression * LicenseExpression

    let licenseRef =
        opt (pstring "DocumentRef-" >>. idstring .>> pstring ":") .>>. (pstring "LicenseRef-" >>. idstring) |>> fun (d, l) -> LicenseRef (sprintf "%sLicenseRef-%s" (defaultArg d "") l)
    
    let license = choice [
        licenseRef
        idstring |>> LicenseId
    ]
    
    let licenseWithException =
        license .>>. opt (pstring " WITH " >>. idstring |>> Exception) |>> With
    
    let opp = new OperatorPrecedenceParser<LicenseExpression, unit, unit>()

    opp.TermParser <- licenseWithException <|> between (pstring "(") (pstring ")") opp.ExpressionParser

    opp.AddOperator(InfixOperator(" AND", spaces1, 1, Associativity.Left, fun x y -> And(x, y)))
    opp.AddOperator(InfixOperator(" OR", spaces1, 2, Associativity.Left, fun x y -> Or(x, y)))

    let licenseExpr = opp.ExpressionParser
