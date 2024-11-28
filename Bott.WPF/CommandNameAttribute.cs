namespace Bott.WPF;

[AttributeUsage(AttributeTargets.Method)]
internal class CommandNameAttribute(string commandName) : Attribute
{
	public string CommandName { get; private set; } = commandName;
}