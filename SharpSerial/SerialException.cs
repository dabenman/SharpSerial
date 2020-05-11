using System;

namespace SharpSerial
{
    public class SerialException : Exception
    {
        public SerialException(string message, string trace) : base(message)
        {
            Trace = trace;
        }

        public string Trace { get; }
    }
}