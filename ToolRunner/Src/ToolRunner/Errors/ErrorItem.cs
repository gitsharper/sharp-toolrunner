

namespace CustomToolBase {

	/////////////////////////////////////////////////////////////////////////////

	public class ErrorItem : ErrorItemBase {

		public bool ParseSuccess { get; set; }
		public int ErrorNumber { get; set; }

		/////////////////////////////////////////////////////////////////////////////

		public ErrorItem( bool parseSuccess = true, int errorNumber = -1)
		{
			ParseSuccess = parseSuccess;
			ErrorNumber = errorNumber;
		}
	}

}