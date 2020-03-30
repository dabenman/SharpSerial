﻿using System;

namespace SharpSerial
{
    public interface ISerialStream
    {
        void Write(byte[] data);
        byte[] Read(int size, int eop, int toms);
    }
}
