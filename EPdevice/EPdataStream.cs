using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Xml;

//using FileManager;

namespace EPdevice
{
	///  <summary>
	///  This class represents a read / write memory stream.
	///  </summary>
	sealed public class EPdataStream : Stream
	{
		private MemoryStream innerStream;
		private long readPosition;
		private long writePosition;
		private long overflowPosition;
		private Boolean processingData;

		///  <summary>
		///  Constructor.
		///  </summary>
		public EPdataStream(Boolean start = false)
		{
			innerStream = new MemoryStream();
			readPosition = 0;
			writePosition = 0;
			overflowPosition = 0;
			processingData = start;
		}

		///  <summary>
		///  Destructor.
		///  </summary>
		~EPdataStream()
		{
			innerStream.Dispose();
		}

		#region Stream
		/// <summary>
		/// Gets a value indicating whether the stream supports reading.
		/// </summary>
		public override bool CanRead { get { return true; } }

		/// <summary>
		/// Gets a value indicating whether the stream supports seeking.
		/// </summary>
		public override bool CanSeek { get { return false; } }

		/// <summary>
		/// Not supported.
		/// </summary>
		public override bool CanTimeout
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// Gets a value indicating whether the stream supports writing.
		/// </summary>     
		public override bool CanWrite { get { return true; } }

		/// <summary>
		/// Gets the length of the stream in bytes. Waits for the stream to be unlocked.
		/// </summary>
		public override long Length
		{
			get
			{
				Boolean available = false;
				long memLength;

				while (!available)
					Monitor.TryEnter(innerStream, ref available);

				memLength = innerStream.Length;
				Monitor.Exit(innerStream);

				return memLength;
			}
		}

		/// <summary>
		/// Not supported.
		/// </summary>
		public override long Position
		{
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}

		/// <summary>
		/// Not supported.
		/// </summary>
		public override int ReadTimeout
		{
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}

		/// <summary>
		/// Clears all buffers for this stream.
		/// </summary>
		public override void Flush()
		{
			Boolean available = false;

			while (!available)
				Monitor.TryEnter(innerStream, ref available);

			innerStream.Flush();
			Monitor.Exit(innerStream);
		}

		/// <summary>
		/// Not supported.
		/// </summary>
		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Not supported.
		/// </summary>
		public override void SetLength(long value)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Reads a block of bytes from the current stream and writes the data to a buffer.
		/// </summary>
		/// <param name="buffer"> Contains the values between offset and (offset + count - 1) replaced by the characters read from the stream. </param>
		/// <param name="offset"> Not supported. </param>
		/// <param name="count"> The maximum number of bytes to read. </param>
		/// <returns> Number of bytes read. </returns>
		public override Int32 Read(Byte[] buffer, Int32 offset, Int32 count)
		{
			return ReadStream(buffer, count);
		}

		/// <summary>
		/// Writes a block of bytes to the current stream using data read from a buffer.
		/// </summary>
		/// <param name="buffer"> The buffer to write data from. </param>
		/// <param name="offset"> The zero-based byte offset in buffer at which to begin copying bytes to the current stream. </param>
		/// <param name="count"> The maximum number of bytes to write. </param>
		public override void Write(Byte[] buffer, Int32 offset, Int32 count)
		{
			WriteStream(buffer, offset, count);
		}
		#endregion

		///  <summary>
		///  Writing process can mark this stream as finished.
		///  </summary>
		public Boolean ProcessingData
		{
			get { return processingData; }
			set { processingData = value; }
		}

		/// <summary>
		/// Read bytes from the current stream.
		/// </summary>
		private Int32 ReadStream(Byte[] buffer, Int32 count)
		{
			Boolean available = false;
			Int32 readBytes = 0;

			while (!available)
				Monitor.TryEnter(innerStream, ref available);

			// Pushing towards overflow.
			if (overflowPosition != 0)
			{
				if ((readPosition + count) >= overflowPosition)
				{
					innerStream.Position = readPosition;
					readBytes = innerStream.Read(buffer, 0, (Int32)(overflowPosition - readPosition));
					innerStream.Position = 0;
					overflowPosition = 0;
					readBytes += innerStream.Read(buffer, readBytes, count - readBytes);
					readPosition = innerStream.Position;
				}
				else
				{
					innerStream.Position = readPosition;
					readBytes = innerStream.Read(buffer, 0, count);
					readPosition = innerStream.Position;
				}
			}
			// No overflow occured yet.
			else
			{
				if ((readPosition + count) <= writePosition)
				{
					innerStream.Position = readPosition;
					readBytes = innerStream.Read(buffer, 0, count);
					readPosition = innerStream.Position;
				}
				else
				{
					readBytes = 0;
				}
			}

			Monitor.Exit(innerStream);

			return readBytes;
		}


		/// <summary>
		/// Write bytes to the current stream.
		/// </summary>
		private void WriteStream(Byte[] buffer, Int32 offset, Int32 count)
		{
			Boolean available = false;

			while (!available)
				Monitor.TryEnter(innerStream, ref available);

			innerStream.Position = writePosition;				// Set the current write position.
			try
			{
				innerStream.Write(buffer, offset, count);		// Write data.
				writePosition = innerStream.Position;			// Save the new write position.
			}
			catch (System.OutOfMemoryException)
			{
				overflowPosition = writePosition;				// Set overflow on old position.
				innerStream.Position = 0;							// Start writing at the beginning.
				innerStream.Write(buffer, offset, count);		// Write data.
			}
			finally
			{
				Monitor.Exit(innerStream);
			}
		}
	}
}
