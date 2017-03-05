# Reusable v2.13.0

`Reusable` is a collection of libraries that I use in my other projects.

## Changelog

### v2.13.0

- Bug fixes and improvements.

### v2.0.0

- There were too many projects. Some of them that didn't have any external dependencies were merged into `Reusable.Utils`

---

Why? I didn't want to write the same things over and over agian so I put them in separate projects ready for reuse.

It contains the following parts:

- `Reusable.Core` - General purpose utilities.
  - `Reusable.Clocks` - Wrappers for `DateTime`
  - `Reusable.Clocks.IClock` - The base interace for clocks.
  - `Reusable.Clocks.SystemClock` - System date and time.
  - `Reusable.Clocks.TestClock` - Date and time for testing.
  - `Resuable.Collection` - Custom collections.
  - `Resuable.Collections.AutoEqualityComparer` - 
  - `Resuable.Collections.AutoEqualityComparerFactory`
  - `Resuable.Collections.AutoKeyDictionary`
  - `Resuable.Collections.Enumerable`
  - `Resuable.Commands.LinkedCommand`
  - `Resuable.Commands.CommandComposition`
  - `Resuable.Data.Annotations`
  - `Resuable.Data.AppConfigRepository`
  - `Resuable.Drawing.Color32`
  - `Resuable.Drawing.ColorParser`
  - `Resuable.Formatters.Formatter`
  - `Resuable.Formatters.FormatterComposition`
  - `Resuable.Formatters.Formatters.BracketFormatter`
  - `Resuable.Formatters.Formatters.CaseFormatter`
  - `Resuable.Formatters.Formatters.DecimalColorFormatter`
  - `Resuable.Formatters.Formatters.HexadecimalColorFormatter`
  - `Resuable.Formatters.Formatters.QuoteFormatter`
  - `Resuable.Sequences.GeneratedSequence`
  - `Resuable.Sequences.FibonacciSequence`
  - `Resuable.Sequences.GeometricSequence`
  - `Resuable.Sequences.HarmonicSequence`
  - `Resuable.Sequences.LinearSequence`
  - `Resuable.Sequences.RegularSequence`
  - `Resuable.Sequences.FibonacciSequenceFactory`
  - `Resuable.Conditional`
  - `Resuable.Enumerator`
  - `Resuable.ExceptionPrettifier`
  - `Resuable.Node`
  - `Resuable.Reflection`
  - `Resuable.StringInterpolation`
- `Reusable.ConsoleColorizer` for easier console styling via xml.
- `Reusable.TypeConversion` for easier and extendable type conversion.
- `Reusable.Fuse` for consistent and fluent inline validation and verification.
- `Reusable.Logging.NLog.Tools` for easier `NLog` configuration.
- `Reusable.Markup.MakrupBuilder` for easier dynamic html/xml creation.
- `Reusable.SemanticVersion` for working with semantic versions.

---

Icon made by [Roundicons](http://www.flaticon.com/authors/roundicons) from www.flaticon.com is licensed by <a href="http://creativecommons.org/licenses/by/3.0/" title="Creative Commons BY 3.0" target="_blank">CC 3.0 BY</a></div>

Icon made by [Vectors Market](http://www.flaticon.com/authors/vectors-market) from www.flaticon.com is licensed by <a href="http://creativecommons.org/licenses/by/3.0/" title="Creative Commons BY 3.0" target="_blank">CC 3.0 BY</a></div>

