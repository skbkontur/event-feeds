# Changelog

## v1.1.3 - 2021.12.01
- Update dependencies.
- Run tests against net6.0 tfm.

## v1.1.2 - 2021.03.14
- Update dependencies.
- Run tests against net5.0 tfm.

## v1.1.1 - 2021.03.05
- Add cancellation token to the `EventFeedsRunner` and blade executing.

## v1.0.9 - 2020.04.16
- Allow clients to implement event feed lag metrics reporting in such a way 
  that only the leader thread which runs given feed will send metrics to e.g. Graphite.

## v1.0.5 - 2020.04.12
- Extract this package from [internal git repository](https://git.skbkontur.ru/edi/edi/tree/c600d6072e243a4302a73bacc5629f560fd25779/Core/EventFeeds).
- Target .NET Standard 2.0.
- Use [SourceLink](https://github.com/dotnet/sourcelink) to help ReSharper decompiler show actual code.
- Use [Nerdbank.GitVersioning](https://github.com/AArnott/Nerdbank.GitVersioning) to automate generation of assembly and nuget package versions.
