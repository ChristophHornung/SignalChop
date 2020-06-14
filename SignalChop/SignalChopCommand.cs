using System.Threading.Tasks;

namespace Crosberg.SignalChop
{
	internal abstract class SignalChopCommand
	{
		public abstract string Name { get; }
		public abstract void DisplayHelp();
		public abstract Task Execute(SignalChopper chopper, string[] args);
	}
}