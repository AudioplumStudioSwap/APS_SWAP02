using System;

namespace Aube
{
	//!	@class	Assertion
	//!
	//!	@brief	Static class that provides methods to check for assertion statements
	public static class Assertion
	{
#if UNITY_EDITOR
		public delegate void DisplayAssertionDialog(string a_message, System.Diagnostics.StackTrace a_stackTrace);
		public static DisplayAssertionDialog displayDialogCallback = null;
#endif // UNITY_EDITOR

		//!	@brief	asserts if the condition is false
		//!
		//!	@param	a_condition		the condition to check
		public static void Check(bool a_condition)
		{
			if(!a_condition)
			{
				ProceedAssertion("ASSERTION!");
			}
		}

		//!	@brief	asserts if the condition is false, giving an information message
		//!
		//!	@param	a_condition		the condition to check
		//!	@param	a_info			the information message
		public static void Check(bool a_condition, string a_info)
		{
			if(!a_condition)
			{
				ProceedAssertion("ASSERTION! : " + a_info);
			}
		}

		//!	@brief	trigger an assertion because this code should not be executed
		public static void UnreachableCode()
		{
			ProceedAssertion("ASSERTION! : Unreachable Code!");
		}

#region Private
		private static void ProceedAssertion(string a_info)
		{
			Log.Fatal(a_info);

#if UNITY_EDITOR
			System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(true);
			if(displayDialogCallback != null)
			{
				displayDialogCallback(a_info, stackTrace);
			}
#endif // UNITY_EDITOR

			throw new SystemException(a_info);
		}
#endregion
	}
} // namespace Aube