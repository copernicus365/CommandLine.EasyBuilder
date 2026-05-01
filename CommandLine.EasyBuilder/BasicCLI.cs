using System.CommandLine;
using System.Text;

using static System.Console;

namespace CommandLine.EasyBuilder;

/// <summary>
/// Simple runner for System.CommandLine apps. Three modes:
/// <list type="bullet">
///   <item><see cref="RunOnce"/> — pure one-shot: parse args, run, exit. Standard CLI semantics.</item>
///   <item><see cref="RunRepl"/> — pure REPL: prompt → parse → execute, loop until exit/EOF.</item>
///   <item><see cref="Run"/> — orchestrator: combines the two; useful when you want either mode selectable at runtime.</item>
/// </list>
/// For full-fledged apps you'd write your own loop. This covers the common cases
/// (single-file utilities, dev REPLs) without 30 lines of boilerplate.
/// </summary>
public static class BasicCLI
{
	/// <summary>
	/// One-shot. Parses <paramref name="args"/> as a single command, runs it, returns
	/// the System.CommandLine exit code. Empty <paramref name="args"/> falls back to
	/// <paramref name="noArgsDefaultCmd"/> (default: <c>-h</c>, i.e. show help).
	/// </summary>
	public static async Task<int> RunOnce(
		RootCommand rootCmd,
		string[] args,
		string noArgsDefaultCmd = "-h")
	{
		string cmdln = (args == null || args.Length == 0)
			? noArgsDefaultCmd
			: JoinArgs(args);
		return await ExecuteAsync(rootCmd, cmdln);
	}

	/// <summary>
	/// Pure REPL. Prompts repeatedly, executing each entered command, until the user
	/// types <c>exit</c> / <c>quit</c> / <c>:q</c> or sends EOF (Ctrl+Z on Windows,
	/// Ctrl+D on Unix). Empty input just re-prompts.
	/// Optional <paramref name="initialCmd"/> runs once before the first prompt
	/// (handy for showing help on entry: pass <c>"-h"</c>).
	/// </summary>
	public static async Task RunRepl(
		RootCommand rootCmd,
		string initialCmd = null,
		string prompt = ">> ")
	{
		string cmdln = initialCmd?.Trim();
		while(true) {
			if(string.IsNullOrWhiteSpace(cmdln)) {
				Write(prompt);
				cmdln = ReadLine()?.Trim();
				if(cmdln is null) break;   // EOF (Ctrl+Z / Ctrl+D)
			}

			if(IsExit(cmdln)) break;

			if(!string.IsNullOrEmpty(cmdln)) {
				await ExecuteAsync(rootCmd, cmdln);
				WriteLine();
			}
			cmdln = null;
		}
	}

	/// <summary>
	/// Smart orchestrator. By default <em>auto-detects</em> REPL mode from the args:
	/// no args → REPL, presence of any flag in <paramref name="replFlags"/> (default
	/// <c>--repl</c>) → REPL, otherwise one-shot. Pass <paramref name="interactive"/>
	/// non-null to force a specific mode.
	///
	/// Recognized REPL flags are stripped from the args before they're handed to
	/// System.CommandLine, so they never collide with command parsing.
	/// </summary>
	/// <param name="rootCmd">The configured root command to dispatch against.</param>
	/// <param name="args">Process arguments (typically passed straight through from <c>Main</c>).</param>
	/// <param name="interactive">null = auto-detect; true = force REPL; false = force one-shot.</param>
	/// <param name="noArgsDefaultCmd">Command used when no args are present (defaults to <c>-h</c>, i.e. show help).</param>
	/// <param name="replFlags">
	/// Tokens that, if present in <paramref name="args"/>, switch on REPL mode and are
	/// stripped from the args. Defaults to <c>["--repl"]</c>. Pass <c>["--repl", "-i"]</c>
	/// to add the conventional short form, or <c>[]</c> to disable flag-based detection
	/// (auto-detect then only fires on no-args).
	/// </param>
	public static async Task Run(
		RootCommand rootCmd,
		string[] args,
		bool? interactive = null,
		string noArgsDefaultCmd = "-h",
		IEnumerable<string> replFlags = null)
	{
		(string[] cmdArgs, bool autoInteractive) = ParseReplFlags(args, replFlags);
		bool effective = interactive ?? autoInteractive;

		if(!effective) {
			await RunOnce(rootCmd, cmdArgs, noArgsDefaultCmd);
			return;
		}
		string initialCmd = cmdArgs.Length == 0
			? noArgsDefaultCmd
			: JoinArgs(cmdArgs);
		await RunRepl(rootCmd, initialCmd);
	}

	/// <summary>
	/// Inspects <paramref name="args"/> for REPL trigger flags and returns
	/// (a) the args with those flags removed, and (b) whether REPL mode should
	/// activate (true if any flag was present, OR if the resulting args are empty).
	///
	/// Useful when you want the standard REPL detection convention without going
	/// through <see cref="Run"/> — e.g. when you're dispatching to a custom flow.
	/// </summary>
	/// <param name="args">Process arguments to scan and strip.</param>
	/// <param name="replFlags">
	/// Tokens that switch on REPL mode. Defaults to <c>["--repl"]</c>.
	/// Pass <c>[]</c> to skip flag detection (only no-args triggers REPL).
	/// </param>
	public static (string[] CmdArgs, bool Interactive) ParseReplFlags(
		string[] args,
		IEnumerable<string> replFlags = null)
	{
		HashSet<string> flags = replFlags is null
			? ["--repl"]
			: [.. replFlags];

		args ??= [];
		bool flagPresent = flags.Count > 0 && args.Any(flags.Contains);
		string[] cmdArgs = flags.Count == 0
			? args
			: [.. args.Where(a => !flags.Contains(a))];

		return (cmdArgs, flagPresent || cmdArgs.Length == 0);
	}

	static async Task<int> ExecuteAsync(RootCommand rootCmd, string cmdln)
	{
		if(string.IsNullOrWhiteSpace(cmdln)) return 0;

		ParseResult res = rootCmd.Parse(cmdln);
		if(res.Errors.Any()) {
			foreach(var err in res.Errors)
				WriteLine($"Error: {err.Message}");
			WriteLine();
			return 1;
		}
		return await res.InvokeAsync();
	}

	static bool IsExit(string s)
		=> s == "exit" || s == "quit" || s == ":q" || s == ":exit";

	/// <summary>
	/// Joins a string[] back into a single command-line string, quoting any element
	/// that contains whitespace or quotes so System.CommandLine's tokenizer recovers
	/// the original boundaries.
	/// </summary>
	static string JoinArgs(string[] args)
	{
		StringBuilder sb = new();
		for(int i = 0; i < args.Length; i++) {
			if(i > 0) sb.Append(' ');
			string a = args[i] ?? "";
			bool needsQuotes = a.Length == 0 || a.AsSpan().IndexOfAny(' ', '\t', '"') >= 0;
			if(needsQuotes) {
				sb.Append('"');
				sb.Append(a.Replace("\"", "\\\""));
				sb.Append('"');
			}
			else {
				sb.Append(a);
			}
		}
		return sb.ToString();
	}
}
