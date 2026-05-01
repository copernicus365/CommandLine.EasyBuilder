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
	/// Orchestrator. If <paramref name="interactive"/> is false, runs once and exits
	/// (same as <see cref="RunOnce"/>). If true, runs the initial command (from
	/// <paramref name="args"/>, or <paramref name="noArgsDefaultCmd"/> if none),
	/// then drops into a REPL.
	/// </summary>
	public static async Task Run(
		RootCommand rootCmd,
		string[] args,
		bool interactive = false,
		string noArgsDefaultCmd = "-h")
	{
		if(!interactive) {
			await RunOnce(rootCmd, args, noArgsDefaultCmd);
			return;
		}
		string initialCmd = (args == null || args.Length == 0)
			? noArgsDefaultCmd
			: JoinArgs(args);
		await RunRepl(rootCmd, initialCmd);
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
