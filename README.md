# SPDX License Expression Parser and Evaluator

This library parses and evaluates [SPDX license expressions](//spdx.github.io/spdx-spec/v2-draft/SPDX-license-expressions).
This library provides a robust and efficient way to handle license expressions in your .NET applications,
ensuring compliance with SPDX standards.

## Features

- **SPDX License Expression Parsing**: Parse complex SPDX license expressions with ease.
- **License Evaluation**: Evaluate license expressions against a given context to determine compliance.
- **Customizable Context**: Implement your own context to evaluate licenses based on your specific requirements.

## Getting Started

### Prerequisites

- .NET 6.0 SDK or later

### Usage

Here's a simple example of how to use the library to parse and evaluate a license expression:

```csharp
using Progalgo.SPDX;

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

class Program
{
    static void Main()
    {
        LicenseExpression expression = new("MIT OR Apache-2.0");
        var context = new AcceptedLicensesContext { "MIT" };

        bool isCompliant = expression.Evaluate(context);
        Console.WriteLine($"Is compliant: {isCompliant}");
    }
}
```

### Running Tests

To run the tests, use the following command:

```sh
dotnet test
```

## Contributing

We welcome contributions!

## License

This project is licensed under the GPL License.

## Acknowledgements

- [FParsec](https://www.quanttec.com/fparsec/) for the powerful F# parser combinator library used in this project.

## Contact

For any questions or feedback, please open an issue on GitHub.
