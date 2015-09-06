

namespace CustomToolBase {

	/////////////////////////////////////////////////////////////////////////////

	public class ErrorItemBase {

		public string FileName { get; set; }
		public string ErrorText { get; set; }
		public int Line { get; set; }
		public int Column { get; set; }

		/////////////////////////////////////////////////////////////////////////////

		public ErrorItemBase( string fileName = "", string errorText = "", int line = -1, int col = -1 )
		{
			FileName = string.IsNullOrEmpty(fileName) ? string.Empty : fileName;
			ErrorText = string.IsNullOrWhiteSpace(errorText) ? string.Empty : errorText;
			Line = line;
			Column = col;
		}
	}

}