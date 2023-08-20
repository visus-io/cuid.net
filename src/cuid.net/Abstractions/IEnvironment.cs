namespace Visus.Cuid.Abstractions;

internal interface IEnvironment
{
	int CurrentManagedThreadId { get; }
	
	string MachineName { get; }
	
	int ProcessId { get; }
}