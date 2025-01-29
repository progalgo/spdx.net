namespace Progalgo.SPDX

module Parser =

    open FParsec
    
// SPDX License Expression grammar in ABNF
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

    type ExpressionTree =
        | License of string
        | LicenseWithException of string * string
        | And of ExpressionTree * ExpressionTree
        | Or of ExpressionTree * ExpressionTree

    let parseLicenseId = many1Satisfy (fun c -> isLetter c || isDigit c || c = '-' || c = '.')
    let parseLicenseRef =
        opt (pstring "DocumentRef-" >>. parseLicenseId .>> pstring ":") >>. pstring "LicenseRef-" >>. parseLicenseId
    let parseSimpleExpression = 
        (parseLicenseId .>> pstring "+" |>> License)
        <|> (parseLicenseId |>> License)
        <|> (parseLicenseRef |>> License)
    let parseLicenseWithException =
        parseLicenseId .>> pstring "WITH" .>>. parseLicenseId |>> LicenseWithException
    let parseCompoundExpressionRef = ref (fun _ -> failwith "uninitialized")
    let rec parseCompoundExpression = parseCompoundExpressionRef.Value
    do parseCompoundExpressionRef := 
        (parseSimpleExpression .>> pstring "AND" .>>. parseCompoundExpression |>> And)
        <|> (parseSimpleExpression .>> pstring "OR" .>>. parseCompoundExpression |>> Or)
        <|> (parseSimpleExpression .>>. parseCompoundExpression |>> And)
        <|> (parseSimpleExpression |>> id)
        <|> (between (pstring "(") (pstring ")") parseCompoundExpression)
    let parseLicenseExpression = parseCompoundExpression <|> parseSimpleExpression

    let runParser input =
        match run parseLicenseExpression input with
        | Success(result, _, _) -> Some(result)
        | Failure(_, _, _) -> None
