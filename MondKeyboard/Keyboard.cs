using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace MondKeyboard
{
    static class Keyboard
    {
        private static GlobalKeyboardHook _keyHook;
        private static byte[] _keyStates = new byte[256];
        private static Queue<char> _input = new Queue<char>();

        public static int InputCount { get; set; }

        public static IEnumerable<char> TriggerInput(string trigger)
        {
            var triggerIndex = 0;

            foreach (var c in Input())
            {
                if (triggerIndex >= trigger.Length)
                {
                    if (InputCount == trigger.Length + 1 && c == '=')
                    {
                        foreach (var ch in "return ")
                        {
                            yield return ch;
                        }
                    }
                    else
                    {
                        yield return c;
                    }

                    continue;
                }

                if (c == trigger[triggerIndex])
                {
                    triggerIndex++;
                    InputCount = triggerIndex;
                    continue;
                }

                triggerIndex = 0;
            }
        }

        public static IEnumerable<char> Input()
        {
            InputCount = 0;

            var prevCh = '\0';
            while (true)
            {
                char? ch;

                lock (_input)
                {
                    if (_input.Count == 0)
                        ch = null;
                    else
                        ch = _input.Dequeue();
                }

                if (ch.HasValue)
                {
                    if (ch.Value == '\uFFFF')
                        throw new Exception();

                    if (prevCh != '\r' || ch.Value != '\n')
                        InputCount++;

                    prevCh = ch.Value;
                    yield return ch.Value;

                    if (ch.Value == ';')
                        yield return ' ';
                }
                else
                {
                    Thread.Sleep(10);
                }
            }
        }

        public static void Start()
        {
            _keyHook = new GlobalKeyboardHook();
            _keyHook.KeyDown += HookKeyDown;
            _keyHook.KeyUp += HookKeyUp;
        }

        public static void Reset()
        {
            lock (_input)
            {
                _input.Clear();
            }

            InputCount = 0;
        }

        private static void HookKeyUp(object sender, KeyEventArgs e)
        {
            _keyStates[(int)e.KeyCode] = 0x00;

            if (e.KeyCode == Keys.LShiftKey || e.KeyCode == Keys.RShiftKey)
                _keyStates[(int)Keys.ShiftKey] = 0x00;
        }

        private static void HookKeyDown(object sender, KeyEventArgs e)
        {
            _keyStates[(int)e.KeyCode] = 0xFF;

            if (e.KeyCode == Keys.LShiftKey || e.KeyCode == Keys.RShiftKey)
                _keyStates[(int)Keys.ShiftKey] = 0xFF;

#if DEBUG
            Console.WriteLine("Key: {0}", e.KeyCode);
#endif

            if (e.KeyCode == Keys.Escape)
            {
                lock (_input)
                {
                    _input.Enqueue('\uFFFF');
                }

                return;
            }

            var buf = new StringBuilder(32);
            ToUnicode((uint)e.KeyCode, 0, _keyStates, buf, 32, 0);

            var str = buf.ToString();

            if (str.Length == 0 || str[0] == '\b')
                return;

            if (str[0] == '\r')
                str = "\r\n";

            lock (_input)
            {
                foreach (var ch in str)
                {
                    _input.Enqueue(ch);
                }
            }
        }

        [DllImport("user32.dll")]
        private static extern int ToUnicode(
            uint virtualKeyCode,
            uint scanCode,
            byte[] keyboardState,
            [Out, MarshalAs(UnmanagedType.LPWStr, SizeConst = 32)] StringBuilder receivingBuffer,
            int bufferSize,
            uint flags);
    }
}
