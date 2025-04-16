module Progalgo.SPDX.Parser

open System.Runtime.CompilerServices

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


type License =
    | LicenseId of string
    | LicenseRef of string

    override this.ToString() =
        match this with
        | LicenseId id -> id
        | LicenseRef ref -> ref

type Exception =
    | Exception of string

    override this.ToString() =
        match this with
        | Exception ex -> ex

type LicenseExpression =
    | With of License * Exception Option
    | And of LicenseExpression list
    | Or of LicenseExpression list

    override this.ToString() =
        match this with
        | With(l, None) -> $"{l}"
        | With(l, Some e) -> $"{l} WITH {e}"
        | And ls -> "(" + System.String.Join(" AND ", Seq.map string ls) + ")"
        | Or ls -> "(" + System.String.Join(" OR ", Seq.map string ls) + ")"

    static member (&&&)(l, r) =
        match (l, r) with
        | (And ls, And rs) -> And(ls @ rs)
        | (And ls, r) -> And(ls @ [ r ])
        | (l, And rs) -> And(l :: rs)
        | (l, r) -> And [ l; r ]

    static member (|||)(l, r) =
        match (l, r) with
        | (Or ls, Or rs) -> Or(ls @ rs)
        | (Or ls, r) -> Or(ls @ [ r ])
        | (l, Or rs) -> Or(l :: rs)
        | (l, r) -> Or [ l; r ]


let justLicense l = With(LicenseId l, None)

let (^^) (l, e) = With(LicenseId l, Some(Exception e))




let idstring = many1Satisfy (fun c -> isLetter c || isDigit c || c = '-' || c = '.')

let licenseRef =
    opt (pstring "DocumentRef-" >>. idstring .>> pstring ":")
    .>>. (pstring "LicenseRef-" >>. idstring)
    |>> fun (d, l) -> LicenseRef(sprintf "%sLicenseRef-%s" (defaultArg d "") l)

let license = choice [ licenseRef; idstring |>> LicenseId ]

let licenseWithException =
    (license) .>>. opt ((pstring " WITH ") >>. idstring |>> Exception) |>> With

let opp = new OperatorPrecedenceParser<LicenseExpression, unit, unit>()

opp.TermParser <-
    (licenseWithException)
    <|> between (pstring "(") (pstring ")") opp.ExpressionParser

opp.AddOperator(InfixOperator(" AND", spaces, 1, Associativity.Left, fun x y -> x &&& y))
opp.AddOperator(InfixOperator(" OR", spaces, 2, Associativity.Left, fun x y -> x ||| y))

let licenseExpr = opp.ExpressionParser

let public parseLicenseExpression (s: string) =
    match run licenseExpr s with
    | Success(v, _, _) -> v
    | Failure(msg, _, _) -> failwith msg
