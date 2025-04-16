namespace Progalgo.SPDX

open Parser

type License (id: string) =
    class
        member val Id = id with get
    end

type Exception (id: string) =
    class
        member val Id = id with get
    end

type ILegalContext<'V> =
    abstract member Evaluate : License -> Exception seq -> 'V
    abstract member EvaluateOption : 'V seq -> 'V
    abstract member EvaluateAnd : 'V seq -> 'V

type LicenseExpression (e: string) =

    let expr = lazy (parseLicenseExpression e)

    member this.Evaluate<'V> (ctx: ILegalContext<'V>) : 'V =
        let rec eval (e: Parser.LicenseExpression) : 'V =
            match e with
            | With (LicenseId lId, Some (Exception eId)) -> ctx.Evaluate (License lId) [Exception eId]
            | With (LicenseId lId, None) -> ctx.Evaluate (License lId) []
            | With (LicenseRef rId, _) -> ctx.Evaluate (License rId) []
            | And es -> ctx.EvaluateAnd [for e in es -> eval e]
            | Or es -> ctx.EvaluateOption [for e in es -> eval e]
        eval (expr.Value)
