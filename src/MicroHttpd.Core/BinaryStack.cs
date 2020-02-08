using System;

namespace MicroHttpd.Core
{
	/// <summary>
	/// Utility to store binary data as a stack,
	/// allows fast push/pop a block of binary data 
	/// into/from the stack by utilizing Buffer.BlockCopy;
	/// 
	/// Used by the HttpBufferedReader to store read-ahead data.
	/// </summary>
	sealed class BinaryStack
	{
		// The data
		readonly byte[] _stackData;

		// The rollback buffer is a stack of bytes,
		// new bytes appended to the left, 
		// data read from left to right.
		//
		// This points to the top of the stack.
		// If the stack is full, this would be 0,
		// If the stack is empty, it is _stackData.Length.
		int _rollbackBufferTopIndex;

		/// <summary>
		/// The amount of data stored in this buffer, in butes
		/// </summary>
		public int Length
		{ get { return _stackData.Length - _rollbackBufferTopIndex; } }

		/// <summary>
		/// Constructing the stack with a fixed stack size,
		/// not worth implementing a dynamic sized stack, 
		/// a fixed size one is all we need.
		/// </summary>
		public BinaryStack(int stackSize)
		{
			RequireValidStackSize(stackSize);
			_stackData = new byte[stackSize];
			_rollbackBufferTopIndex = _stackData.Length;
		}

		/// <summary>
		/// Push data to the stack,
		/// Copy 'count' bytes from specified buffer into the stack,
		/// starting from 'offset'.
		/// </summary>
		public void Push(byte[] src, int offset, int count)
		{
			// Some validations
			Validation.RequireValidBuffer(src, offset, count);
			RequireEnoughSpaceToPush(count);

			// Move the head of the stack to the left 'count' bytes.
			// Notes: it is important to move the pointer first,
			// before doing BlockCopy()
			_rollbackBufferTopIndex -= count;

			// Write!
			Buffer.BlockCopy(
				src:		src, 
				srcOffset:	offset, 
				dst:		_stackData, 
				dstOffset:	_rollbackBufferTopIndex, 
				count:		count
				);
		}

		/// <summary>
		/// Pop data from the stack,
		/// Copy 'count' bytes to the specified buffer, starting from offset.
		/// </summary>
		public void Pop(byte[] dest, int offset, int count)
		{
			// Some validations
			Validation.RequireValidBuffer(dest, offset, count);
			RequireEnoughDataToPop(count);

			// Write to dest!
			Buffer.BlockCopy(
				src:		_stackData,
				srcOffset:	_rollbackBufferTopIndex,
				dst:		dest,
				dstOffset:	offset,
				count:		count
				);

			// Move the head of the stack to the right 'count' bytes.
			// Notes: it is important to move the pointer after the BlockCopy() above.
			_rollbackBufferTopIndex += count;
		}

		void RequireEnoughDataToPop(int bytesAskedToPop)
		{
			if(bytesAskedToPop > Length)
				throw new InvalidOperationException(
					"Not enough space in the stack to pop data"
					);
		}

		void RequireEnoughSpaceToPush(int bytesAskedToPush)
		{
			if((_rollbackBufferTopIndex - bytesAskedToPush) < 0)
				throw new InvalidOperationException(
					"Not enough data in the stack to pop"
					);
		}

		static void RequireValidStackSize(int stackSize)
		{
			if(stackSize <= 0 || stackSize >= (16 * 1024 * 1024))
				throw new ArgumentException(
					$"Invalid stack size: {stackSize}"
					);
		}
	}
}