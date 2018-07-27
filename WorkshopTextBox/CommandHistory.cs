using System.Collections;

namespace WorkshopTextBox
{
	internal class CommandHistory
	{
		private int currentPosition;

		private ArrayList commandHistory = new ArrayList();

		internal CommandHistory()
		{
			currentPosition = -1;
		}

		internal void Add(string command)
		{
			commandHistory.Add(command);
			currentPosition = commandHistory.Count;
		}

		internal bool HasPreviousCommand()
		{
			return currentPosition > 0;
		}

		internal bool HasNextCommand()
		{
			return currentPosition < commandHistory.Count - 1;
		}

		internal string GetPreviousCommand()
		{
			currentPosition--;
			return (string)commandHistory[currentPosition];
		}

		internal string GetNextCommand()
		{
			currentPosition++;
			return (string)commandHistory[currentPosition];
		}
	}
}
