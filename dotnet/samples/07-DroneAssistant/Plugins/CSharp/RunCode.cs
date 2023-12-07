// Copyright (c) Microsoft. All rights reserved.

using System.ComponentModel;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.CSharp.Scripting;


public sealed class RunCode
{
    [SKFunction, Description("Build and run C# code")]
    public async Task<string> BuildAndRun(
        [Description("The base C# code to build and run"), SKName("code")] string codeToRun)
    {
        var options = ScriptOptions.Default
            .AddReferences(typeof(object).Assembly)
            .AddReferences(typeof(TelloSharp.Tello).Assembly)
            .AddImports("System", "System.IO", "System.Text", "System.Text.RegularExpressions");

        try
        {
            Console.WriteLine("===============================");
            Console.WriteLine("Start running code ...");
            Console.WriteLine("===============================");
            var t = await CSharpScript.RunAsync(codeToRun, options);
            
            Console.WriteLine("Script Return Value: " + t.ReturnValue.ToString());
            Console.WriteLine("===============================");
            Console.WriteLine("Running code done");
            Console.WriteLine("===============================");
        }
        catch (CompilationErrorException ex)
        {
            Console.WriteLine("===============================");
            Console.WriteLine("Compilation Error");
            Console.WriteLine("===============================");
            Console.WriteLine(codeToRun);

            var sb = new StringBuilder();
            foreach (var err in ex.Diagnostics)
                sb.AppendLine(err.ToString());

            Console.WriteLine(sb.ToString());
            Console.WriteLine("===============================");
        }
        // Runtime Errors
        catch (Exception ex)
        {
            Console.WriteLine("===============================");
            Console.WriteLine("General Exception");
            Console.WriteLine("===============================");
            Console.WriteLine(codeToRun);
            Console.WriteLine(ex.ToString());
            Console.WriteLine("===============================");
        }       

        return "Done";
    }
}