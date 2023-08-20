namespace Visus.Cuid;

using System.Diagnostics.CodeAnalysis;
using Abstractions;

[ExcludeFromCodeCoverage]
internal sealed class Environment : IEnvironment
{
	public int CurrentManagedThreadId => System.Environment.CurrentManagedThreadId;
	
	public string MachineName
	{
		get
		{
			try
			{
				return System.Environment.MachineName;
			}
			catch ( InvalidOperationException )
			{
				return string.Empty;
			}
		}
	}
	
	public int ProcessId => System.Environment.ProcessId;
}