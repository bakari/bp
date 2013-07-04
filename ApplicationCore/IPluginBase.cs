using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

namespace ApplicationCore
{
	/// <summary>
	/// Plugin base class
	/// </summary>
	public interface IPluginBase
	{
		void Load(CompositionContainer container);
	}
}
