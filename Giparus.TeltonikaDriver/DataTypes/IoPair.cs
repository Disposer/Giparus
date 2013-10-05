using System;
using System.Linq;

namespace Giparus.TeltonikaDriver.DataTypes
{
    public enum IoType:byte { OneByte=1, TwoBytes=2, FourBytes=4, EightBytes=8 };

    public class IoPair
    {
        private readonly byte[] _value;

        public byte Id { get; private set; }
        public IoType Type { get; private set; }

        public byte[] GetRawValue()
        {
            return _value.ToArray(); //Note:clone value
        }

        internal IoPair(byte id, IoType type, byte[] value)
        {
            this.Id = id;
            this.Type = type;
            _value = value;

            if((int)type!=value.Length)
                throw new InvalidOperationException("value length is not match with Io type");
        }

        internal IoPair(byte id, byte[] value)
        {
            this.Id = id;
            if(!Enum.IsDefined(typeof(IoType), (byte)value.Length))
                throw new InvalidOperationException("value length is not match with Io type");

            this.Type = (IoType)value.Length;
            _value = value;
        }
    }
}
