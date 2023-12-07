// Copyright (c) Microsoft. All rights reserved.

using System.ComponentModel;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.CSharp.Scripting;


public sealed class RunCode
{
    private ILoggerFactory? _logger;

    public RunCode(ILoggerFactory? loggerFactory = null)
    {
        this._logger = loggerFactory;
    }

    [SKFunction, Description("Build and execute C# code")]
    public async Task<string> BuildAndRun(
        [Description("The base C# code class file"), SKName("code")] string classfile)
    {
        FileInfo sourceFile = new FileInfo(classfile);
        Console.WriteLine("Loading file: " + sourceFile.Exists);
        var fileContent = File.ReadAllText(sourceFile.FullName);

        var options = ScriptOptions.Default
            .AddReferences(typeof(object).Assembly)
            .AddReferences(typeof(TelloSharp.Tello).Assembly)
            .AddImports("System", "System.IO", "System.Text", "System.Text.RegularExpressions");

        try
        {
            await CSharpScript.RunAsync(fileContent, options);
        }
        catch (CompilationErrorException ex)
        {
            Console.WriteLine(fileContent);

            var sb = new StringBuilder();
            foreach (var err in ex.Diagnostics)
                sb.AppendLine(err.ToString());

            Console.WriteLine(sb.ToString());
        }
        // Runtime Errors
        catch (Exception ex)
        {
            Console.WriteLine(fileContent);
            Console.WriteLine(ex.ToString());
        }

        Console.WriteLine("Exit...");

        return "Done";
    }
}