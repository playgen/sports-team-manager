/// <summary>
/// Connecting class between GameManager in logic and the Learning Pill UI
/// </summary>
public class LearningPill {
	/// <summary>
	/// Return the learning pill text for the provided key
	/// </summary>
	public string GetHelpText(string key)
	{
		return GameManagement.GameManager.EventController.GetHelpText(key);
	}
}
