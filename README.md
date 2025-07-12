# LlamaServerClientSharp

A C# client interface for [`llama-server`](https://github.com/ggml-org/llama.cpp/blob/master/tools/server/README.md), llama.cpp's HTTP server.

# Disclaimer

This project is incomplete, and is missing some parts of the APIs.

I find the state of this project sufficient enough for my personal use. If you need something else, go ahead and make changes as you please. (A PR would be highly appreciated!)

# How to use

No package is provided for now because I am lazy, sorry.

You can either:
- Clone this repository somewhere, and add a reference to it.
- Add this repo to your repo as a git submodule, and add a reference to it.
- Copy `LlamaClient.cs` and `LlamaClientStructs.cs` files directly into your project.

### Clone this repository and add reference

```sh
# on somewhere
git clone https://github.com/sinusinu/LlamaServerClientSharp
# on your project
dotnet add reference ../path/to/cloned/repo/LlamaServerClientSharp.csproj
```

### Add as git submodule and add reference

```sh
# on your project (should be a git repository)
git submodule add https://github.com/sinusinu/LlamaServerClientSharp
dotnet add reference LlamaServerClientSharp/LlamaServerClientSharp.csproj
```

# Quickstart

```csharp
using static LlamaServerClientSharp.LlamaClient;

using var client = new LlamaClient();
// or: using var client = new LlamaClient("http://localhost:8080");

var messages = new Message.ListBuilder()
    .System("Write an answer to the user's message.")
    .User("Nice to meet you!")
    .Build();

var request = new OAIChatCompletionRequest.Builder()
    .SetMessages(messages)
    .Build();

var response = await client.OAIChatCompletionAsync(request);
Console.WriteLine(response.FirstChoice.Message.Content);
```

More examples can be found at `src/Program.cs`.

# License

MIT
