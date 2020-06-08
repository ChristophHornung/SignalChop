# SignalChop

![CI Build](https://github.com/ChristophHornung/SignalChop/workflows/CI%20Build/badge.svg)

SignalChop is a simple command line sender/receiver for [SignalR](https://github.com/SignalR/SignalR) messages. It can be used to call and receive from SignalR endpoints.

## Example
Checkout the `SignalChop.Example` folder for a full send/receive example.

## Installation

Grab the latest version [here](https://github.com/ChristophHornung/SignalChop/releases).

## Usage

  `SignalChop [options]`

### Options

 | Option | Description |
 |:--- |:--- |
 | --command-file command-file             | An optional command file that will be executed line by line. |
 | --quite                                 | Whether to output status information or restrict the output to received json only. |
 | --exit-after-count count                | An integer to indicate how many messages to retrieve before quitting. 0 indicates that no auto-exit will occur. |
 | --version                               | Show version information |
 | -?, -h, --help                          | Show help and usage information |
  
## Commands
  
### Connect
Usage: *Connect server*

Connects to the given signalR server.

Example: `Connect https://localhost:50001/chatHub`
  
### Listen
Usage: *Listen method [parameter1] [parameter2]...*

Listens for SignalR invocations for the given `method`. Received messages will be output on the console in JSON format.

*[parameterX]* will be used to name the given parameter in the json output. The number of parameters has to match the `method`-definition on the server.

Example: `Listen broadcastMessage username chatmessage"`
  
### StopListen
Usage: *StopListen method*

Stops listening for SingalR invocation messages for `method`.

Example: `StopListen broadcastMessage"`

### Send
Usage: *Send method [parameter1] [parameter2] ...*

Sends a SignalR invocation message for `method` to the server.

*[parameterX]* defines the parameters of the invocation. The number of parameters has to match the `method`s definition on the server.

Use `'`-marks to denote strings or json. Json parameters have to start with a curly bracket (`{`).

Example: `Send Order 'Nike' 10 '{\"ProductName\":\"Shoe\", \"Id\":2, \"Comment\":\"Pink laces\"}'`

### Quit
Usage: *Quit [waitCount]*

Quits all execution and disconnects from the server.
The optional *[waitCount]* indicates to not quit immediately but instead wait for *[waitCount]* invocations from the server first and then quit.

Example: `Quit 2`
