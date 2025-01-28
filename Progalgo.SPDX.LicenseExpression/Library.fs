namespace Progalgo.SPDX

type LicenseExpression (e: string) = class end

type License (id: string) = class end

type Exception (name: string) = class end

[<AbstractClass>]
type LegalScope<'value> () =
    class
    abstract member Evaluate : License -> Exception seq -> 'value
    abstract member EvaluateOption : 'value seq -> 'value
    abstract member EvaluateAnd : 'value seq -> 'value
    end
