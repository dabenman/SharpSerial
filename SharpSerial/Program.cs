﻿using System;
using System.Text;

namespace SharpSerial
{
    partial class Program
    {
        static void Handler(Exception ex)
        {
            Try(() => Dump(ex));
            Try(() => Exception(ex));
            Environment.Exit(1);
        }

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) => Handler(e.ExceptionObject as Exception);

            var wrapper = new SerialWrapper();
            foreach (var arg in args) wrapper.SetProperty(arg);
            var line = Console.ReadLine();
            while (line != null)
            {
                if (string.IsNullOrWhiteSpace(line)) break;
                else if (line.StartsWith("$"))
                {
                    if (line.Contains("=")) wrapper.SetProperty(line.Substring(1));
                    else
                    {
                        var parts = line.Split(new char[] { ',' });
                        switch (parts[0])
                        {
                            case "$r":
                                if (parts.Length < 4) throw Make("Expected 4 parts for {0}", Readable(line));
                                var rSize = ParseInt(line, parts[1], 1);
                                var rEop = ParseInt(line, parts[2], 2);
                                var rToms = ParseInt(line, parts[3], 3);
                                var rData = wrapper.Read(rSize, rEop, rToms);
                                AnswerHex(rData);
                                break;
                            default:
                                throw Make("Unknown command {0}", Readable(line));
                        }
                    }
                }
                else if (line.StartsWith(">"))
                {
                    wrapper.Write(ParseHex(line));
                }
                else
                {
                    throw Make("Unknown command {0}", Readable(line));
                }
                line = Console.ReadLine();
            }
            Environment.Exit(0);
        }

        static int ParseInt(string line, string part, int index)
        {
            if (int.TryParse(part, out var value)) return value;
            throw Make("Invalid int at param {0} of {1}", index, Readable(line));
        }

        static void AnswerHex(byte[] data)
        {
            var sb = new StringBuilder();
            sb.Append("<");
            foreach (var b in data) sb.Append(b.ToString("X2"));
            Console.WriteLine(sb.ToString());
        }

        static byte[] ParseHex(string text)
        {
            Assert(text.Length % 2 == 1, "Odd length expected for {0}:{1}", text.Length, text);
            var bytes = new byte[text.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                var b2 = text.Substring(1 + i * 2, 2);
                bytes[i] = Convert.ToByte(b2, 16);
            }
            return bytes;
        }
    }
}
