// See https://aka.ms/new-console-template for more information
using System.CommandLine;
using CqrsScaffold.Tool.Commands;

var rootCommand = ScaffoldCommand.Create();
return await rootCommand.Parse(args).InvokeAsync();