
using Microsoft.SemanticKernel.AI.ChatCompletion;
using Microsoft.SemanticKernel.Handlebars;
using System.Runtime.CompilerServices;

string OpenAIApiKey = Env.Var("OpenAI:ApiKey")!;
string BingApiKey = Env.Var("Bing:ApiKey")!;
string currentDirectory = Directory.GetCurrentDirectory();

// Initialize the required functions and services for the kernel
IChatCompletion gpt35Turbo = new OpenAIChatCompletion("gpt-3.5-turbo-1106", OpenAIApiKey);
OllamaGeneration ollamaGeneration = new("wizard-math");

// Create plugins
IPlugin mathPlugin = new Plugin(
    name: "Math",
    functions: NativeFunction.GetFunctionsFromObject(new Math())
);

Plugin ollamaGenerationPlugin = new Plugin(
    name: "Math",
    functions: new() {
        SemanticFunction.GetFunctionFromYaml(currentDirectory + "/Plugins/Ollama/Math.prompt.yaml")
    }
);


//IPlugin searchPlugin = new Plugin(
//    name: "Search",
//    functions: NativeFunction.GetFunctionsFromObject(new Search(BingApiKey))
//);

//// Create a researcher
//IPlugin researcher = AssistantKernel.FromConfiguration(
//    currentDirectory + "/Assistants/Researcher.agent.yaml",
//    aiServices: new () { gpt35Turbo, },
//    plugins: new () { searchPlugin }
//);

// Create a mathmatician
IPlugin mathmatician = AssistantKernel.FromConfiguration(
    currentDirectory + "/Assistants/Mathmatician.agent.yaml",
    aiServices: new() { gpt35Turbo, ollamaGeneration },
    plugins: new() { mathPlugin }
);

Plugin ollamaGenerationPluginDrone = new Plugin(
    name: "Drone",
    functions: new() {
        SemanticFunction.GetFunctionFromYaml(currentDirectory + "/Plugins/TelloDrone/TelloDrone.prompt.yaml")
    }
);

// Create a mathmatician
IPlugin droneCodeGen = AssistantKernel.FromConfiguration(
    currentDirectory + "/Assistants/DroneCodeGenerator.agent.yaml",
    aiServices: new() { gpt35Turbo, ollamaGeneration },
    plugins: new() { ollamaGenerationPluginDrone }
);


// Create a Project Manager
AssistantKernel projectManager = AssistantKernel.FromConfiguration(
    currentDirectory + "/Assistants/ProjectManager.agent.yaml",
    aiServices: new() { gpt35Turbo },
    plugins: new() { mathmatician, droneCodeGen }
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
        if (userInput.ToLower() == "exit")
        {
            keepRunning = false;
            continue;
        }

        // validate if uyser ipunt is "d"
        if (userInput.ToLower() == "d")
            userInput = "generate a flight plan for a drone with the following actions: takeoff the drone, move forward 25 centimeters, flip right, move down 30 centimeters and land";


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
