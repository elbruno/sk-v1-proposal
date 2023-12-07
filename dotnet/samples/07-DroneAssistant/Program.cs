﻿
using Microsoft.SemanticKernel.AI.ChatCompletion;
using Microsoft.SemanticKernel.Handlebars;
using System.Runtime.CompilerServices;

string OpenAIApiKey = Env.Var("OpenAI:ApiKey")!;
string BingApiKey = Env.Var("Bing:ApiKey")!;
string currentDirectory = Directory.GetCurrentDirectory();

// Initialize the required functions and services for the kernel
IChatCompletion gpt35Turbo = new OpenAIChatCompletion("gpt-3.5-turbo-1106", OpenAIApiKey);

IPlugin searchPlugin = new Plugin(
    name: "Search",
    functions: NativeFunction.GetFunctionsFromObject(new Search(BingApiKey))
);

// Create a researcher
IPlugin researcher = AssistantKernel.FromConfiguration(
    currentDirectory + "/Assistants/Researcher.agent.yaml",
    aiServices: new() { gpt35Turbo, },
    plugins: new() { searchPlugin }
);

Plugin openAIChatCompletionDrone = new Plugin(
    name: "Drone",
    functions: new() {
        SemanticFunction.GetFunctionFromYaml(currentDirectory + "/Plugins/TelloDrone/TelloDrone.prompt.yaml")
    }
);

// Create a drone code generator agent
IPlugin droneCodeGen = AssistantKernel.FromConfiguration(
    currentDirectory + "/Assistants/DroneCodeGenerator.agent.yaml",
    aiServices: new() { gpt35Turbo },
    plugins: new() { openAIChatCompletionDrone }
);


// Create a Project Manager
AssistantKernel projectManager = AssistantKernel.FromConfiguration(
    currentDirectory + "/Assistants/ProjectManager.agent.yaml",
    aiServices: new() { gpt35Turbo },
    plugins: new() { researcher, droneCodeGen }
);

IThread thread = await projectManager.CreateThreadAsync();
bool keepRunning = true;
while (keepRunning)
{
    // Get user input
    Console.Write("User > ");
    string userInput = Console.ReadLine();
    if (userInput != null)
    {
        // validate if the user wants to exit
        if (userInput.ToLower() == "exit" || string.IsNullOrEmpty(userInput.ToLower()))
        {
            keepRunning = false;
            continue;
        }

        // validate if uyser ipunt is "d"
        if (userInput.ToLower() == "d")
        {
            userInput = "generate a flight plan for a drone with the following actions: takeoff the drone, move forward 25 centimeters, flip right, move down 30 centimeters and land";
            Console.WriteLine("def user input > " + userInput);
        }

        _ = thread.AddUserMessageAsync(userInput);

        // Run the thread using the project manager kernel
        var result = await projectManager.RunAsync(thread);

        // Print the results
        var messages = result.GetValue<List<ModelMessage>>();
        foreach (ModelMessage message in messages)
        {
            Console.WriteLine("Project Manager > " + message);
        }
    }
}