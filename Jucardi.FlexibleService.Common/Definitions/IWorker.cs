// Filename: IWorker.cs
// Author:   Jucardi
// Date:     3/23/2014 11:12:12 AM
//
// The use of this code is subject to Open Source License Agreement.

namespace Jucardi.FlexibleService.Common
{
	public interface IWorker
	{
		/// <summary>
		/// Starts the execution of the worker.
		/// </summary>
		void Start();

		/// <summary>
		/// Stops the execution of the worker.
		/// </summary>
		void Stop();

		/// <summary>
		/// Gets a value indicating whether this instance is running.
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance is running; otherwise, <c>false</c>.
		/// </value>
		bool IsRunning { get; }

		/// <summary>
		/// Gets or sets the worker name.
		/// </summary>
		/// <value>
		/// The name.
		/// </value>
		string Name { get; }
	}
}
