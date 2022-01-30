using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Mond;
using Mond.Libraries;

namespace MondKeyboard
{
    static class Program
    {
        private static int _sendKeysCount;

        [STAThread]
        public static void Main()
        {
            Keyboard.Start();

            var replThread = new Thread(MondRepl);
            replThread.Start();
            replThread.Join();
        }

        private static void MondRepl()
        {
            var state = new MondState()
            {
                Options =
                {
                    DebugInfo = MondDebugInfoLevel.Full,
                    MakeRootDeclarationsGlobal = true,
                    UseImplicitGlobals = true
                }
            };

            state.Libraries.Configure(libs =>
            {
                var consoleOutput = libs.Get<ConsoleOutputLibrary>();
                consoleOutput.Out = new SendKeysTextWriter();

                var consoleInput = libs.Get<ConsoleInputLibrary>();
                consoleInput.In = new KeyboardInputTextReader();
            });

            state["quit"] = new MondFunction((_, args) =>
            {
                Environment.Exit(1);
                return MondValue.Undefined;
            });

            state["keys"] = new MondFunction((_, args) =>
            {
                SendKeys(args[0]);
                return MondValue.Undefined;
            });

            const string initFile = "init.mnd";
            if (File.Exists(initFile))
            {
                var initCode = File.ReadAllText(initFile);
                state.Run(initCode, initFile);
            }

            state.EnsureLibrariesLoaded();
            
            while (true)
            {
                try
                {
                    var program = MondProgram.CompileStatements(Keyboard.TriggerInput("!m "), "stdin", state.Options).First();

                    while (Keyboard.InputCount > 0)
                    {
                        SendKeys("{bs}", true);
                        Keyboard.InputCount--;
                    }

                    _sendKeysCount = 0;

                    var result = state.Load(program);
                    if (result != MondValue.Undefined)
                    {
                        SendKeys(Escape(result.Serialize()), true);
                    }
                }
                catch (Exception e)
                {
                    if (e is MondException)
                        SendKeys(Escape("ERROR: " + e.Message), true);

                    Keyboard.Reset();
                }
            }
        }

        private static void SendKeys(string keys, bool noCheck = false)
        {
            if (!noCheck && _sendKeysCount >= 100)
                throw new Exception();

            System.Windows.Forms.SendKeys.SendWait(keys);

            _sendKeysCount++;
        }

        private static string Escape(string input)
        {
            var sb = new StringBuilder(input.Length);

            foreach (var c in input)
            {
                switch (c)
                {
                    case '\n':
                        break;

                    case '+':
                    case '^':
                    case '%':
                    case '~':
                    case '(':
                    case ')':
                    case '[':
                    case ']':
                        sb.Append('{');
                        sb.Append(c);
                        sb.Append('}');
                        break;

                    case '{':
                        sb.Append("{{}");
                        break;

                    case '}':
                        sb.Append("{}}");
                        break;

                    default:
                        sb.Append(c);
                        break;
                }
            }

            return sb.ToString();
        }

        private class SendKeysTextWriter : TextWriter
        {
            public override Encoding Encoding
            {
                get { throw new NotSupportedException(); }
            }

            public override void Write(char value)
            {
                SendKeys(Escape("" + value));
            }

            public override void Write(string value)
            {
                SendKeys(Escape(value));
            }

            public override void WriteLine(string value)
            {
                SendKeys(Escape(value));
            }

            public override void WriteLine()
            {
                SendKeys("~");
            }
        }

        private class KeyboardInputTextReader : TextReader
        {
            public override string ReadLine()
            {
                return Keyboard.Input()
                    .TakeWhile(c => c != '\r')
                    .Aggregate("", (a, e) => a + e);
            }
        }
    }
}