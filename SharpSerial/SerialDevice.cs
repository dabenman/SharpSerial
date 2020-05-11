using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Threading;

namespace SharpSerial
{
    // com0com BytesToRead is UNRELIABLE
    // Solution based on BaseStream and influenced by
    // https://www.sparxeng.com/blog/software/must-use-net-system-io-ports-serialport
    // https://www.vgies.com/a-reliable-serial-port-in-c/
    public class SerialDevice : ISerialStream, IDisposable
    {
        private readonly byte[] buffer;
        private readonly List<byte> list;
        private readonly Queue<byte> queue;
        private readonly SerialPort serial;

        public SerialDevice(object settings)
        {
            buffer = new byte[256];
            list = new List<byte>(256);
            queue = new Queue<byte>(256);
            serial = new SerialPort();

            //init serial port and launch async reader
            SerialSettings.CopyProperties(settings, serial);
            serial.Open();
            //DiscardInBuffer not needed by FTDI and ignored by com0com
            var stream = serial.BaseStream;
            //unavailable after closed so pass it
            stream.BeginRead(buffer, 0, buffer.Length, ReadCallback, stream);
        }

        public void Dispose()
        {
            Tools.Try(serial.Close);
            Tools.Try(serial.Dispose);
        }

        public void Write(byte[] data)
        {
            var stream = serial.BaseStream;
            stream.Write(data, 0, data.Length);
            //always flush to allow sync by following read available
            stream.Flush();
        }

        public byte[] Read(int size, int eop, int toms)
        {
            list.Clear();
            var dl = DateTime.Now.AddMilliseconds(toms);
            while (true)
            {
                var b = ReadByte();
                if (b == -1)
                {
                    //toms=0 should return immediately with available
                    if (DateTime.Now >= dl) break;
                    Thread.Sleep(1);
                    continue;
                }

                list.Add((byte) b);
                if (eop >= 0 && b == eop) break;
                if (size >= 0 && list.Count >= size) break;
                dl = DateTime.Now.AddMilliseconds(toms);
            }

            return list.ToArray();
        }

        private int ReadByte()
        {
            lock (queue)
            {
                if (queue.Count == 0) return -1;
                return queue.Dequeue();
            }
        }

        private void ReadCallback(IAsyncResult ar)
        {
            Tools.Try(() =>
            {
                //try needed to avoid triggering the domain unhandled 
                //exception handler when used as standalone stream
                var stream = ar.AsyncState as Stream;
                var count = stream.EndRead(ar);
                if (count > 0) //0 for closed stream
                {
                    lock (queue)
                    {
                        for (var i = 0; i < count; i++) queue.Enqueue(buffer[i]);
                    }

                    stream.BeginRead(buffer, 0, buffer.Length, ReadCallback, stream);
                }
            });
        }
    }
}