using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StreamDeckEmulator.Services
{
    public interface IConsole
    {
        void Write(string message);
    }

    public class ConsoleWrapper : IConsole
    {
        public void Write(string message)
        {
            Console.Out.Write(message);
        }
    }
}
