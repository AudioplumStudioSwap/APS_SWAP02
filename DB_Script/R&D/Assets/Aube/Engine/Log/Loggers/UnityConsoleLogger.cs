namespace Aube
{
	//! @class UnityConsoleLogger
	//!
	//! @brief logger that writes in the Unity console
	public class UnityConsoleLogger : Logger
	{
#region Inherited Methods from Logger
		protected override void Append(string message)
		{
			switch(CurrentVerbosity)
			{
				case Log.Verbosity.Debug:
				case Log.Verbosity.Info:
				{
					UnityEngine.Debug.Log(message);
				}
				break;
				case Log.Verbosity.Warning:
				case Log.Verbosity.WarningPerf:
				{
					UnityEngine.Debug.LogWarning(message);
				}
				break;
				case Log.Verbosity.Error:
				case Log.Verbosity.Fatal:
				{
					UnityEngine.Debug.LogError(message);
				}
				break;
			}
		}
#endregion
	}
}