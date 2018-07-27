using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace WorkshopTextBox
{
	public class WorkshopTextBox : TextBox
	{
		private string prompt = "WorkshopTool>";

		private CommandHistory commandHistory;

		private bool bIsBlockingInput;

		public string Command;

		private IContainer components;

		public event EventHandler CommandEntered;

		public WorkshopTextBox()
		{
			InitializeComponent();
			commandHistory = new CommandHistory();
			bIsBlockingInput = false;
		}

		protected override void WndProc(ref Message m)
		{
			switch (m.Msg)
			{
			case 771:
				return;
			case 12:
			case 768:
			case 770:
				if (!IsCaretPositionWriteable())
				{
					MoveCaretToEndOfText();
				}
				break;
			}
			base.WndProc(ref m);
		}

		private void TextBox_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (char.IsControl(e.KeyChar) && e.KeyChar != '\r' && e.KeyChar != '\b' && e.KeyChar != '\u0003' && e.KeyChar != '\u0016' && e.KeyChar != '\u0018')
			{
				e.Handled = true;
			}
			else if (e.KeyChar != '\u0003')
			{
				if (bIsBlockingInput)
				{
					e.Handled = true;
				}
				else if (e.KeyChar == '\r')
				{
					if (this.CommandEntered != null)
					{
						Command = GetCommand();
						if (Command != "")
						{
							commandHistory.Add(Command);
						}
						PrintNewLine();
						bIsBlockingInput = true;
						this.CommandEntered(this, new EventArgs());
					}
					e.Handled = true;
				}
				else if (e.KeyChar == '\b' && IsCaretRightAfterPrompt())
				{
					e.Handled = true;
				}
			}
		}

		private void TextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (!IsCaretPositionWriteable() && !e.Control && e.KeyCode != Keys.Return)
			{
				MoveCaretToEndOfText();
			}
			if (e.KeyCode == Keys.Left && IsCaretRightAfterPrompt())
			{
				e.Handled = true;
			}
			else if (e.KeyCode == Keys.Up)
			{
				if (commandHistory.HasPreviousCommand())
				{
					ReplaceTextAtPrompt(commandHistory.GetPreviousCommand());
				}
				e.Handled = true;
			}
			else if (e.KeyCode == Keys.Down)
			{
				if (commandHistory.HasNextCommand())
				{
					ReplaceTextAtPrompt(commandHistory.GetNextCommand());
				}
				e.Handled = true;
			}
		}

		private string GetCurrentLine()
		{
			if (base.Lines.Length > 0)
			{
				return (string)base.Lines.GetValue(base.Lines.GetLength(0) - 1);
			}
			return "";
		}

		private int GetCaretColumnPosition()
		{
			string currentLine = GetCurrentLine();
			int caretPosition = base.SelectionStart;
			return caretPosition - TextLength + currentLine.Length;
		}

		private bool IsCaretOnCurrentLine()
		{
			return TextLength - base.SelectionStart <= GetCurrentLine().Length;
		}

		private bool IsCaretRightAfterPrompt()
		{
			if (IsCaretOnCurrentLine())
			{
				return GetCaretColumnPosition() == prompt.Length;
			}
			return false;
		}

		private bool IsCaretPositionWriteable()
		{
			if (IsCaretOnCurrentLine())
			{
				return GetCaretColumnPosition() >= prompt.Length;
			}
			return false;
		}

		private void MoveCaretToEndOfText()
		{
			base.SelectionStart = TextLength;
			ScrollToCaret();
		}

		private void ReplaceTextAtPrompt(string text)
		{
			string currentLine = GetCurrentLine();
			int charactersAfterPrompt = currentLine.Length - prompt.Length;
			if (charactersAfterPrompt == 0)
			{
				AppendText(text);
			}
			else
			{
				Select(TextLength - charactersAfterPrompt, charactersAfterPrompt);
				SelectedText = text;
			}
		}

		private string GetCommand()
		{
			string currentLine = GetCurrentLine();
			if (currentLine.Length >= prompt.Length)
			{
				return GetCurrentLine().Substring(prompt.Length);
			}
			return "";
		}

		public void PrintNewLine()
		{
			AppendText(Environment.NewLine);
		}

		public void PrintLine(string text)
		{
			AppendText(text);
			PrintNewLine();
		}

		public void PrintLineAtLineStart(string text)
		{
			int current_line = GetLineFromCharIndex(base.SelectionStart);
			base.SelectionStart = GetFirstCharIndexFromLine(current_line);
			SelectionLength = TextLength - base.SelectionStart;
			SelectedText = text;
			MoveCaretToEndOfText();
		}

		public void PrintPrompt()
		{
			string currentText = Text;
			if (currentText.Length != 0 && currentText[currentText.Length - 1] != '\n')
			{
				PrintNewLine();
			}
			AppendText(prompt);
			Focus();
			bIsBlockingInput = false;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			SuspendLayout();
			BackColor = System.Drawing.Color.White;
			Font = new System.Drawing.Font("Courier New", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
			ForeColor = System.Drawing.Color.Black;
			base.Name = "WorkshopTextBox";
			base.Size = new System.Drawing.Size(1024, 768);
			base.WordWrap = true;
			base.AcceptsReturn = true;
			MaxLength = 0;
			base.KeyDown += new System.Windows.Forms.KeyEventHandler(TextBox_KeyDown);
			base.KeyPress += new System.Windows.Forms.KeyPressEventHandler(TextBox_KeyPress);
			ResumeLayout(false);
		}
	}
}
