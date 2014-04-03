// Filename: TestWorker.cs
// Author:   Jucardi
// Date:     4/2/2014 6:18:02 PM
//
// Confidential Information. Not for disclosure or distribution prior written consent.
// This software contains code, techniques and know-how which is confidential and 
// proprietary of the author.
//
// Use of this software is subject to the terms of an end user license agreement.

using System;
using Jucardi.FlexibleService.Common.BaseWorkers;

namespace Jucardi.SampleWorker
{
	internal class TestWorker : TimerWorker
	{
		#region Fields

		private string _name = "Test worker";

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the worker name.
		/// </summary>
		/// <value>
		/// The name.
		/// </value>
		public override string Name
		{
			get { return _name; }
			set { _name = value; }
		}

		#endregion

		#region Events Handlers

		/// <summary>
		/// Called when then timer with the specified interval ticks.
		/// </summary>
		protected override void OnIntervalTick()
		{
			Console.WriteLine("Worker tick ({0}ms)", Interval);
		}

		#endregion

		#region Methods

		#endregion
	}
}
