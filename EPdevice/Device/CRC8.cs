using System;

namespace EPdevice
{
	// This enum is used to indicate what kind of checksum  will be calculated.
	public enum CRC8_POLY
	{
		CRC8 = 0xD5,
		CRC8_CCITT = 0x07,            // Use this one for the ADS1298 data.
		CRC8_DALLAS_MAXIM = 0x31,
		CRC8_SAE_J1850 = 0x1D,
		CRC_8_WCDMA = 0x9B,
	};

	public class CRC8
	{
		private Byte[] table = new Byte[256];

		public Byte init_val = 0;

		///  <summary>
		///  Constructor.
		///  </summary>
		///
		///  <param name="polynomial"> . </param>
		///  
		///  <returns> . </returns> 
		public CRC8(CRC8_POLY polynomial)
		{
			this.table = this.GenerateTable(polynomial);
		}

		///  <summary>
		///  .
		///  </summary>
		///
		///  <param name="val"> . </param>
		///  
		///  <returns> . </returns>
		public Byte CheckSum(params Byte[] val)
		{
			if (val == null)
				throw new ArgumentNullException("val");

			Byte c = init_val;

			foreach (Byte b in val)
				c = table[c ^ b];

			return c;
		}

		///  <summary>
		///  Get/set method for the table.
		///  </summary>
		public Byte[] Table
		{
			get { return this.table; }
			set { this.table = value; }
		}

		///  <summary>
		///  .
		///  </summary>
		///
		///  <param name="polynomial"> . </param>
		///  
		///  <returns> . </returns> 
		public Byte[] GenerateTable(CRC8_POLY polynomial)
		{
			Byte[] csTable = new Byte[256];

			for (Int32 i = 0; i < 256; ++i)
			{
				Int32 curr = i;

				for (Int32 j = 0; j < 8; ++j)
				{
					if ((curr & 0x80) != 0)
						curr = (curr << 1) ^ (Int32)polynomial;
					else
						curr <<= 1;
				}
				csTable[i] = (Byte)curr;
			}
			return csTable;
		}
	}
}
