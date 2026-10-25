# Native AOT planning probes (2026-10-25)

Throwaway checks run while planning the native-AOT work, kept so they can be rerun. Their outputs are recorded in `docs/aot/native-aot-and-net10.md`, sections E1–E8.

| probe | what it checks | evidence | run |
|---|---|---|---|
| `scl-behavior/scl-behavior.cs` | System.CommandLine 2.0.0's own handling of enum options and `bool` flags: arity, help, completions, parsing | E2 | `dotnet run scl-behavior/scl-behavior.cs` |
| `annotations/annotations8.cs`, `annotations10.cs` | the trim/AOT attributes the runtime declares on candidate APIs, .NET 8 against .NET 10 | E3 | `dotnet run annotations/annotations10.cs` |
| `guard-lib/` | the analyzer's view of each planned pattern, built for net8.0 and net10.0 | E3 | `dotnet build guard-lib/GuardProbe.csproj --artifacts-path <a folder outside lab>` |
| `aot-run/` | every planned route in a real native build | E4 | `dotnet publish aot-run/AotProbe.csproj -c Release -r win-x64 --artifacts-path <a folder outside lab>`, then run the exe |
| `size/` | native size per table entry; `-p:DefineConstants=WIDE` or `WIDE%3BARRAYS` widens the table | E5 | as `aot-run/` |
| `runonce/runonce.cs` | `BasicCLI.RunOnce`'s join-and-resplit against `Parse(args)` | E6 | `dotnet run runonce/runonce.cs` |
| `scl-types/scl-types.cs` | which property types System.CommandLine 2.0.0 parses on its own | E7 | `dotnet run scl-types/scl-types.cs` |
| `static-check/static-check.cs` | the static `GetXOption()` return-type check, the old check against the new one | E8 | `dotnet run static-check/static-check.cs -p:PublishAot=false`; native: `dotnet publish static-check/static-check.cs -o <a folder outside lab>` |

Notes:

- Native builds ran with the Visual Studio Installer folder (`C:\Program Files (x86)\Microsoft Visual Studio\Installer`, where `vswhere.exe` lives) first on `PATH`.
- Keep every `--artifacts-path` and `-o` outside these folders: an output folder inside a project folder gets its generated `.cs` files compiled a second time.
- A file-based probe without `#:property PublishAot=false` runs with dynamic code off even under `dotnet run` (E8).
