This example does a simple test of sending and receiving through SignalChop.

Start the example via `Test.ps1`

Does

1. Starts an example echo SignalR service (See `ExampleEchoHub.cs`)
2. Starts SignalChop with the given `Example.ChopCommands` The commands will:
   1. Listens to `broadcastMessage` with two parameters
   2. Prepares to quit after receiving one message
   3. Sends a SignalR message via on `SendComplexObject` which will trigger the `broadcastMessage` reply and
      thus quit SignalChop
3. Pipes the output into `reply.json` in the `bin/Debug/netcoreapp3.1` folder
4. Compares the `reply.json` with the expected `reply.json` in the project folder
