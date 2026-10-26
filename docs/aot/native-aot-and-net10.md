# Native AOT and .NET 10

CommandLine.EasyBuilder 2.0 bound command classes through reflection that a native AOT build can't run. This note records how the library moved to .NET 10 and became native-AOT-safe in October 2026: what broke before, what replaced it and why, what else changed in the same pass, and how the result is checked. How to use it is in the README (its "Native AOT", "Flags that take no value" and "`RunOnce` and parser settings" sections).

A few terms. A *native AOT build* is an app compiled ahead of time to machine code (`dotnet publish` with `PublishAot`): nothing is compiled while it runs, so every closed generic type it uses (`Option<int>`, say) has to exist in code the compiler saw, and reflection sees only what the trimmer kept. A *JIT run* is the ordinary `dotnet run`, where .NET compiles as it goes. The *analyzer* is the build-time checker that flags code a trimmed or native build may not run; its warnings are the IL2xxx and IL3xxx codes. *Dynamic code* is what `RuntimeFeature.IsDynamicCodeSupported` reports: off in a native build, on in a JIT run.

## Summary (short)

EasyBuilder now targets net10.0 only, with `IsAotCompatible` on, and builds with no trim or AOT warning and nothing suppressed. A small typed object per property type, a *binder* (`PropTypeBinder<T>`), replaced the reflection that made `Option<T>` and `Argument<T>`, set their defaults and called `GetValue<T>`; the command class's type is annotated, so the trimmer keeps what EasyBuilder reflects on. Every type System.CommandLine parses on its own is built in. In a native build each of an app's own types, enums included, takes one `AutoBuilder.RegisterTypes()` step, while an app with dynamic code on still binds any type, through a reflection fallback the native compiler removes. The same pass added a flag that takes no value (`MaxArity = 0`) and made `RunOnce` parse its arguments as given, with parser settings. Nothing public was removed; the old code stays one compile symbol away (`EASYBUILDER_LEGACY`); and a self-checking app, `tools/aot-check.cs`, holds the JIT run, the native build and the old code to the same 169 checks.

## What broke before

An app on EasyBuilder 2.0.2, published native AOT, got 14 trim and AOT warnings from inside EasyBuilder (E1), and its first command then failed with `MissingMethodException: No parameterless constructor defined for type '<the command class>'`. EasyBuilder made the command class, its `Option<T>` and `Argument<T>` objects, their defaults and their `GetValue<T>` calls by reflection (`Activator.CreateInstance`, `MakeGenericType`, `MakeGenericMethod`, a compiled expression), which the native build trims or can't generate at all. A default written as a property initializer (`Kind { get; set; } = "heavy"`) crashed at start with a `NullReferenceException`, because the default went through `DefaultValueFactory`, found and set by reflection. An experimental port of a small tool onto EasyBuilder could only be made to work by rooting EasyBuilder's reflection targets by hand and silencing warnings.

The same port turned up two things unrelated to AOT. A `bool` option takes a following `true` or `false` as its value (System.CommandLine's default arity for `bool` is zero or one), so `--apply false` meant false, and no attribute could say "takes no value", since 0 and 0 meant "not set". And `BasicCLI.RunOnce` joined its arguments into one string and parsed that, so a value containing a double quote came apart (E6).

## Why .NET 10

The library had targeted net8.0. System.CommandLine 2.0.0 ships only net8.0 and netstandard2.0 builds, and a net10.0 library uses the first as is. What decided it is the reflection fallback (below): it sits behind `if(RuntimeFeature.IsDynamicCodeSupported)`, and only the .NET 10 analyzer accepts that check as a guard (the property carries `[FeatureSwitchDefinition]` on .NET 10 and nothing on .NET 8). On net10.0 the fallback builds with no warning; on net8.0 the same code draws IL3050 and would need a suppression (E3). Targeting net10.0 alone keeps one code path; an app still on .NET 8 or 9 stays on EasyBuilder 2.0.x.

## How a property binds now

**The command class.** Its type carries `[DynamicallyAccessedMembers(PublicConstructors | PublicProperties | PublicMethods)]` from `AddAutoCommand<TModel>`, `AddAutoCommand(Type)` and `SetAutoCommand<TModel>` all the way down to `CmdModelInfo.ModelType`, so a trimmed or native build keeps its constructors, its properties, and its methods (`Handle`, `HandleAsync`, the static `Get{PropName}Option()` ones). That cleared 5 of the 14 warnings, and the static lookup below 2 more.

**The property's type.** Each property type gets a binder, `PropTypeBinder<T>` behind `IPropTypeBinder`, which does the four things EasyBuilder had done by reflection: makes the `Option<T>` or `Argument<T>`, sets its `DefaultValueFactory`, reads its value through the typed `GetValue`, and supplies `default(T)`. Because `T` is closed wherever a binder is named, the native compiler generates exactly the `Option<int>` code it needs. That cleared the other 7.

`PropTypeBinders.GetBinder` looks a property type up once, when the command tree is built, and caches the binder:

1. **The built-in table**: every type System.CommandLine 2.0.0 parses on its own. That is `bool`, `byte`, `sbyte`, `short`, `ushort`, `int`, `uint`, `long`, `ulong`, `float`, `double`, `decimal`, `DateTime`, `DateTimeOffset`, `DateOnly`, `TimeOnly`, `TimeSpan` and `Guid`, each with its nullable form and its array, plus `string`, `FileInfo`, `DirectoryInfo`, `FileSystemInfo` and their arrays: 58 entries. `int[]`, `long[]`, `double[]` and `decimal[]` keep EasyBuilder's own comma-list binding (an `Option<string>`, `--delays 1,2.5`), so they need no entry. The types System.CommandLine can't parse (`Uri`, `Version`, `IPAddress`, `char`, `Half`, `Int128`, `UInt128`, `nint`, `nuint`, `BigInteger`, E7) are left out.
2. **The types an app registers**: `AutoBuilder.RegisterTypes().Add<Point>().AddEnum<Color>()`. `Add<T>()` registers exactly `T`; `AddEnum<TEnum>()` registers `TEnum`, `TEnum?` and `TEnum[]`. Each step creates the typed binder on the spot: it's the generic call that makes the native compiler build `Option<Point>`, which a stored `typeof(Point)` could only get back to through `MakeGenericType`.
3. **With dynamic code on only**, a binder made by `MakeGenericMethod` on a known generic method, so a JIT app binds any type it always could, with no registration. A native build compiles this step away (E4).
4. **Otherwise none**, and the property is refused when the tree is built:

> Property 'Where' of type 'Point' can't be bound with dynamic code off (a native AOT build, or `dotnet run` of an app with PublishAot on, as a file-based app has by default). Add `AutoBuilder.RegisterTypes().Add<Point>();` before the command tree is built.

For any form of an enum the step named is `AddEnum<Color>()`.

**What the table costs.** A value-type entry compiles its own copy of System.CommandLine's generic code into every native app: about 2.5 KB each, about 1.7 KB for an array, and next to nothing for a reference type, which shares one copy. At those rates the whole table comes to about 120 KB (E5).

**Enums** have no special path: an enum is just another `T`. With dynamic code on, the fallback binds it as System.CommandLine's own `Option<TEnum>`, exactly as before; in a native build it has to be registered, and then it is the same `Option<TEnum>`, with names in any case, numbers, `[Flags]` combinations, arrays, nullable forms, help and completions all System.CommandLine's own (E2, E4). Binding enums as text in a native build was considered and set aside: it would have made the option's type `Option<string>`, so an app's `opt as Option<TEnum>` would come back null, and the two kinds of build would differ.

**When dynamic code is off.** In a native build, and also under `dotnet run` for an app that sets `PublishAot`: a `.csproj` with `<PublishAot>true</PublishAot>`, or a file-based app (`dotnet run app.cs`), which sets it by default. Such an app follows native rules from its first run, so a missing registration shows up at once rather than after publishing. A file-based app that never goes native can add `#:property PublishAot=false`.

## Defaults

A `DefVal`, or a property initializer, is cast to `T` once, when the tree is built. Before the cast EasyBuilder checks `propTyp.IsInstanceOfType(value)`, because the cast alone is looser than the old code was. The runtime unboxes an enum and its underlying integer as each other, so a bare cast takes an `int` as an enum's default, and an enum as an `int`'s. `IsInstanceOfType` is the test the old `Expression.Constant` made, and gives the same six answers:

| the default | the property | old: `Expression.Constant` | a bare `(T)value` | `IsInstanceOfType` |
|---|---|---|---|---|
| `1` (int) | `ConsoleColor` | ArgumentException | `DarkBlue` | false |
| `ConsoleColor.White` | `int` | ArgumentException | `15` | false |
| `1` (int) | `int?` | `1` | `1` | true |
| `1` (int) | `double` | ArgumentException | InvalidCastException | false |
| `1` (int) | `long` | ArgumentException | InvalidCastException | false |
| `1` (int) | `uint` | ArgumentException | InvalidCastException | false |

So a mistyped default still fails at startup, now as an `InvalidCastException` that names the property ("Property 'Delay' has the default 42 (Int32), which can't be the default of its option, of type Double."), where it was an `ArgumentException` ("Argument types do not match").

The default of the property's type, which tells an initializer's real value from an untouched one, is now `default(T)` where it was `Activator.CreateInstance(type)`. The two agree for every type but a struct with its own parameterless constructor, which the old call ran.

## Statics: `Get{PropName}Option()`

A static that supplies a property's `Option<T>` or `Argument<T>` is written where it always was, next to the property. EasyBuilder now looks for it from the command class, with `BindingFlags.FlattenHierarchy`, trying the property's declaring class first, so whatever that class offers is picked as before, and then the command class and on up its base classes. The return type is matched by its generic definition and type argument, where the old code built the expected `Option<T>` with `MakeGenericType`; over 80 combinations the two checks agree, in a JIT run and a native one (E8). One case is new: a static on the command class, for an inherited property whose declaring class has none, is now found instead of being an error. A static-supplied option's value is read through the binder for its type, so in a native build that type must be built in or registered.

## Also in this pass

**A flag that takes no value.** `MinArity` and `MaxArity` now default to -1, meaning not set. With neither set, System.CommandLine's own arity for the type applies; with one set, the other reads as 0, so `MaxArity = 3` alone is still 0 to 3. `MaxArity = 0` gives `ArgumentArity.Zero`: `--apply` sets the flag, and `--apply false` is refused, or the `false` falls to a catch-all argument (E2). A lone `MinArity = 0`, which used to mean "not set", now also makes a flag. Two settings are refused when the tree is built: zero arity on anything but a `bool` or `bool?` (a zero-arity `string` option reads null and an `int` 0, given or not), and a no-value flag whose default is `true`, which could never be false.

**`RunOnce`, `Run` and `RunRepl`.** `RunOnce` parses `args` with `rootCmd.Parse(args, parserConfig)`, so values arrive exactly as the shell split them; with no arguments it still parses `noArgsDefaultCmd` (`-h`) as a command line. `Run` in REPL mode runs its first command from the arguments the same way. All three take `ParserConfiguration parserConfig = null` as a new last parameter, for System.CommandLine's parser settings; `ResponseFileTokenReplacer = null`, for one, keeps a value that starts with `@` as text (E6). Every existing call still compiles; code compiled against 2.0.2 needs a recompile, since the parameter changes the methods' signatures.

## Public API

Nothing public was removed or renamed. The members that can't work in a trimmed or native build keep working as before and gain an attribute that makes the analyzer warn an app that calls them there; EasyBuilder itself calls the new overload beside each.

| member | what changed |
|---|---|
| `new CmdPropGetter(PropertyInfo)`, `CmdPropGetter.PropToCmpProp(PropertyInfo, object)` | `[RequiresUnreferencedCode]`; overloads that take the command class's type. The old constructor goes through the new lookup, so it also finds a static on a base class of the property's declaring class |
| `CmdPropGetter.TryGetExplicitDefaultValue(object, PropertyInfo, object, out object)` | `[RequiresUnreferencedCode]`; an overload that takes the type's default |
| `CmdModelReflectionHelper.TryGetOptionFromStaticMethod(PropertyInfo, bool, Type)` | `[RequiresUnreferencedCode]`; the same lookup as before, the declaring class only, with the new type check; an overload that takes the command class's type |
| `CmdPropGetter.GetArgResFuncReturnsObjectVal(Type, object)` | `[RequiresDynamicCode]`; no longer called |
| the command class's `Type` parameters, and `CmdModelInfo.ModelType` | `[DynamicallyAccessedMembers]` |
| `MinArity`, `MaxArity` | default -1, not 0 |

New: `AutoBuilder.RegisterTypes()`, `TypeRegistrar` (a stateless `readonly struct`, so the chain allocates nothing), and in `CommandLine.EasyBuilder.Internal`, public like their neighbours, `IPropTypeBinder`, `PropTypeBinder<T>`, `PropTypeBinders` (`Register<T>()`, `GetBinder(Type)`) and `CmdProp.Binder`.

## The old way, one switch away

Each of the six places this pass replaced keeps its old code, verbatim, beside the new under `#if EASYBUILDER_LEGACY … #else … #endif`: making the `Option<T>` or `Argument<T>`, setting its default, the default of the property's type, reading the value, finding a `Get{PropName}Option()` static, and `RunOnce`'s and `Run`'s join-and-resplit. Uncommenting one line in `CommandLine.EasyBuilder.csproj` brings the old way back for a JIT app:

```xml
<DefineConstants>$(DefineConstants);EASYBUILDER_LEGACY</DefineConstants>
```

That build shows the old code's analyzer warnings again and can't be built native. The additions (the flag, `parserConfig`, `RegisterTypes()`) are in both. The blocks go once the new path has run in the wild for a while.

## Verifying it

`tools/aot-check.cs` is a file-based app whose `#:project` line binds the library in the working tree. Its 169 checks cover:

- every built-in type, given a value and given none, the latter compared with what a plain System.CommandLine `Option<T>` of the type reads;
- enums, registered;
- defaults and their help;
- the flag and the arity rules;
- statics;
- the command class's own routes: an argument, `ParseResult` with a private setter, `Handle` and `HandleAsync`, `AddAutoCommand(Type)`, `SetAutoCommand<T>()`;
- the refusals, the unregistered enum among them;
- `RunOnce` and `Run`.

It prints PASS or FAIL per check and exits 1 on any failure.

```
dotnet run tools/aot-check.cs -p:PublishAot=false               # the JIT, dynamic code on: the fallback runs
dotnet publish tools/aot-check.cs -o artifacts/aot-check        # the native exe, into the git-ignored artifacts/
artifacts/aot-check/aot-check.exe
dotnet run tools/aot-check.cs -p:PublishAot=false -- --legacy   # with the csproj line above uncommented
```

`-- --dump-help` prints every command's help instead, for comparing the JIT run's with the native one's. The native toolchain finds the linker through `vswhere.exe`, so the Visual Studio Installer folder has to be on `PATH`.

The runs on 2026-10-25:

- **The library:** no warning of any kind.
- **The JIT run:** 169 of 169.
- **The native build:** no warning, and 169 of 169, its only difference from the JIT run being the unregistered enum, refused there by design.
- **Help:** the JIT and native dumps were identical, 230 lines each.
- **The old code:** 169 of 169, with `--legacy` expecting the old behavior in the six checks where the old code behaves differently by design: the two re-split values, the three default exceptions and the inherited static. As a control, the same `--legacy` expectations run against the new code fail exactly those six, so the pass shows the old code really ran.

## The evidence

The scripts behind E2 to E8 are in `lab/aot/2026-10-25-planning-probes/`, with a README on rerunning them.

**E1 · The warnings before.** Built with `-p:IsAotCompatible=true`, the 2.0.2 library gave 14, the same on net8.0 and net10.0:

| where | warning | the call |
|---|---|---|
| `CmdModelInfo.cs:75` | IL2067 | `Activator.CreateInstance(modelType)` |
| `CmdModelReflectionHelper.cs:26`, `:28`, `:45`, `:84` | IL2070 ×4 | `GetMethod("Handle")`, `GetMethod("HandleAsync")`, `GetProperties()`, `GetConstructors(…)` |
| `CmdModelReflectionHelper.cs:112` | IL3050 | `MakeGenericType`, the static's type check |
| `CmdModelReflectionHelper.cs:118` | IL2075 | `pi.DeclaringType.GetMethods(…)` |
| `CmdPropGetter.cs:110` | IL3050 | `typeof(Option<>).MakeGenericType(propTyp)` |
| `CmdPropGetter.cs:139`, `:143` | IL2077 ×2 | `Activator.CreateInstance(constructedGenType, name)` |
| `CmdPropGetter.cs:243` | IL2072 | `Activator.CreateInstance(prop.PropertyType)` |
| `CmdPropGetter.cs:264`, `:272` | IL2080, IL3050 | `DefaultValueFactory` found by `GetProperty`, its `Func<,>` closed by `MakeGenericType` |
| `SetPropValue.cs:124` | IL3050 | `GetValue<T>` reached by `MakeGenericMethod` |

**E2 · System.CommandLine 2.0.0 on its own.** For `Option<Color>`: `red` gives Red, `BLUE` Blue, `2` Blue, `99` the value 99, and `bogus` is refused ("Cannot parse argument 'bogus' for option '--color' as expected type 'Color'. Did you mean one of the following?"); for a `[Flags]` enum, `read,write` and `3` both give Read, Write; `--colors red --colors blue` gives [Red, Blue]. Help lists an enum's names (`--color <Blue|Green|Red>`) but not an enum array's (`--colors <colors>`). Default arity: `bool` 0..1, `string` and enums 1..1, arrays 1..100000. With zero arity, `--flag` is true, and `--flag false` is refused ("Unrecognized command or argument 'false'.") unless a catch-all argument takes the `false`; a zero-arity `string` reads null, an `int` 0.

**E3 · The analyzer, net8.0 against net10.0.** `MakeGenericMethod` on a known method behind the `IsDynamicCodeSupported` check: IL3050 on net8.0, clean on net10.0; unguarded: IL3050 on both. Statics looked up on `pi.DeclaringType`: IL2075 on both. An annotated command class (`GetProperties`, `GetMethod`, statics with base classes, `GetConstructors`, `Activator.CreateInstance`), a typed binder class and a generic registration method: clean on both.

**E4 · A native run.** A small native build bound through typed binders: a zero-arity flag (`--flag` true, `--flag false` refused), an `int` and a `double?` with defaults, a registered enum and its array; the guarded fallback returned null, compiled away.

**E5 · The size of a table entry.** Two native builds alike but for the table: 26 more value-type entries added 66,048 bytes (about 2,540 each), and 17 array entries 28,672 bytes (about 1,690 each).

**E6 · `RunOnce`.** With the old join-and-resplit, `[-p, say "hi" to them]` came through as `-p` = `say \` plus stray text; `Parse(args)` gives `say "hi" to them`. A trailing backslash, an empty value and a `--` tail came through both ways. `[-p, @team please review]` failed both ways ("Response file not found"), which only a parser setting changes.

**E7 · What System.CommandLine 2.0.0 parses.** Yes: the 18 value types of the table, `string`, `FileInfo`, `DirectoryInfo`, `FileSystemInfo`, and nullable forms. No: `char`, `nint` and `nuint` are refused when parsed; `Half`, `Int128`, `UInt128`, `BigInteger`, `Uri`, `Version` and `IPAddress` parse without error and then throw when the value is read.

**E8 · The static's type check.** Ten static methods against four property types, as `Option` and as `Argument`: 80 comparisons, 5 matches, and no case where the old check (`MakeGenericType` and compare) and the new one (generic definition and type argument) disagree, in a JIT run and in a native one. Both reject an `Option<T>` of the wrong type, `Option<int>` for an `int?` property, a subclass of `Option<FileInfo>`, a plain `Option`, and a generic `Option<T> GetX<T>()`.

## What's left

- Remove the `EASYBUILDER_LEGACY` blocks once the new path has run in the wild.
- A native app using DI (`BuilderDI.ModelInstanceGetter = type => ActivatorUtilities.CreateInstance(services, type)`) should draw one analyzer warning in its own lambda, because `Func<Type, object>` carries no annotation (inferred, not run); fixing that means changing the property's type.
- A source generator writing each command class's typed code at compile time, as System.Text.Json's does, would make the table and the registration step unnecessary.
- A later pass on names and shapes in `CommandLine.EasyBuilder.Internal`.
