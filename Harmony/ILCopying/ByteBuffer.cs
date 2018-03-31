using System;

namespace Harmony.ILCopying
{
	public class ByteBuffer
	{
		public byte[] buffer;
		public int position;

        public ByteBuffer(byte[] buffer) => this.buffer = buffer;

        public byte ReadByte()
		{
			CheckCanRead(1);
			return this.buffer[this.position++];
		}

		public byte[] ReadBytes(int length)
		{
			CheckCanRead(length);
            byte[] value = new byte[length];
			Buffer.BlockCopy(this.buffer, this.position, value, 0, length);
            this.position += length;
			return value;
		}

		public short ReadInt16()
		{
			CheckCanRead(2);
            short value = (short)(this.buffer[this.position]
				| (this.buffer[this.position + 1] << 8));
            this.position += 2;
			return value;
		}

		public int ReadInt32()
		{
			CheckCanRead(4);
			int value = this.buffer[this.position]
				| (this.buffer[this.position + 1] << 8)
				| (this.buffer[this.position + 2] << 16)
				| (this.buffer[this.position + 3] << 24);
            this.position += 4;
			return value;
		}

		public long ReadInt64()
		{
			CheckCanRead(8);
            uint low = (uint)(this.buffer[this.position]
				| (this.buffer[this.position + 1] << 8)
				| (this.buffer[this.position + 2] << 16)
				| (this.buffer[this.position + 3] << 24));

            uint high = (uint)(this.buffer[this.position + 4]
				| (this.buffer[this.position + 5] << 8)
				| (this.buffer[this.position + 6] << 16)
				| (this.buffer[this.position + 7] << 24));

			long value = (((long)high) << 32) | low;
            this.position += 8;
			return value;
		}

		public float ReadSingle()
		{
			if (!BitConverter.IsLittleEndian)
			{
                byte[] bytes = ReadBytes(4);
				Array.Reverse(bytes);
				return BitConverter.ToSingle(bytes, 0);
			}

			CheckCanRead(4);
            float value = BitConverter.ToSingle(this.buffer, this.position);
            this.position += 4;
			return value;
		}

		public double ReadDouble()
		{
			if (!BitConverter.IsLittleEndian)
			{
                byte[] bytes = ReadBytes(8);
				Array.Reverse(bytes);
				return BitConverter.ToDouble(bytes, 0);
			}

			CheckCanRead(8);
            double value = BitConverter.ToDouble(this.buffer, this.position);
            this.position += 8;
			return value;
		}

		void CheckCanRead(int count)
		{
			if (this.position + count > this.buffer.Length)
				throw new ArgumentOutOfRangeException();
		}
	}
}
