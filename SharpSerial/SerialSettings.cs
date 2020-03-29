﻿using System;
using System.IO.Ports;
using System.Globalization;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace SharpSerial
{
    public class SerialSettings
    {
        public SerialSettings() => CopyProperties(this, new SerialPort());

        public SerialSettings(SerialPort sp) => CopyProperties(this, sp);

        public void CopyFrom(SerialPort sp) => CopyProperties(this, sp);

        public void CopyTo(SerialPort sp) => CopyProperties(sp, this);

        public void CopyFrom(SerialSettings ss) => CopyProperties(this, ss);

        public void CopyTo(SerialSettings ss) => CopyProperties(ss, this);

        [TypeConverter(typeof(BaudRateConverter))]
        public int BaudRate { get; set; }

        public int DataBits { get; set; }

        public Parity Parity { get; set; }

        public Handshake Handshake { get; set; }

        public StopBits StopBits { get; set; }

        static void CopyProperties(Object source, Object target)
        {
            CopyProperty(source, target, "BaudRate");
            CopyProperty(source, target, "DataBits");
            CopyProperty(source, target, "Parity");
            CopyProperty(source, target, "Handshake");
            CopyProperty(source, target, "StopBits");
        }

        static void CopyProperty(Object source, Object target, string name)
        {
            var propertyTarget = target.GetType().GetProperty(name);
            var propertySource = source.GetType().GetProperty(name);
            propertyTarget.SetValue(target, propertySource.GetValue(source, null), null);
        }
    }

    public class PortNameConverter : TypeConverter
    {
        private readonly Regex re = new Regex(@"[^a-zA-Z0-9_]");

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(SerialPort.GetPortNames());
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return false;
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return (sourceType == typeof(string));
        }

        public override object ConvertFrom(ITypeDescriptorContext context,
            CultureInfo culture, object value)
        {
            if (re.IsMatch(value.ToString())) throw Tools.Make("Invalid chars");
            return value;
        }

        public override object ConvertTo(ITypeDescriptorContext context,
            CultureInfo culture, object value, Type destinationType)
        {
            return value;
        }
    }

    public class BaudRateConverter : TypeConverter
    {
        public readonly static int[] BaudRates = new int[] {
            110,
            300,
            600,
            1200,
            2400,
            4800,
            9600,
            14400,
            19200,
            28800,
            38400,
            56000,
            57600,
            115200,
            128000,
            153600,
            230400,
            256000,
            460800,
            921600
        };

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(BaudRates);
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return false;
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return (sourceType == typeof(string));
        }

        public override object ConvertFrom(ITypeDescriptorContext context,
            CultureInfo culture, object value)
        {
            return int.Parse(value.ToString());
        }

        public override object ConvertTo(ITypeDescriptorContext context,
            CultureInfo culture, object value, Type destinationType)
        {
            return value.ToString();
        }
    }
}
