MondKeyboard
============
Pipe all your keypresses into Mond for on-demand macros. Like a REPL with no graphical or even textual UI of its own.

![demonstration](https://files.facepunch.com/Rohan/2019/January/19_09-59-58.gif)

Works in any application - no textbox required!

## How does it work?
[Mond](https://github.com/Rohansi/Mond) supports compiling script fragments from a stream of characters (`IEnumerable<char>`). This means:
1. We can feed all keypresses into Mond by wrapping them into an `IEnumerable<char>`
2. Mond will return a compiled program as soon as the stream of characters forms a complete statement

Note that this means backspacing is impossible because the characters you type are immediately fed into the compiler. There's no taking it back so don't make any mistakes!

The trigger text `!m` was added to prevent unintentional code execution while you are working. This is not strictly necessary but good to have for obvious reasons.

Additionally, pressing Escape will cancel the compilation of the current statement in case you have made a mistake. You will need to re-enter the trigger text after pressing Escape.

## Builtins
`quit()` will exit MondKeyboard in case you ran it without opening a window.

`keys(str)` will call [`SendKeys`](https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.sendkeys.sendwait?view=windowsdesktop-6.0) with the given string so that you can simulate input into the currently active window.

Mond's standard output functions (`print`, `printLn`) are remapped to `SendKeys` to type out text and the standard input function (`readLn`) is remapped to read your keypresses until you press enter.

The `init.mnd` file will be ran at startup to set up any functions and/or initial state you may want.

## Dependencies
* Windows (for a global keyboard hook)
* .NET Core 3.1 or 6.0
* [Mond](https://github.com/Rohansi/Mond)
