using WixToolset.Dtf.WindowsInstaller;

namespace KbdMsi
{
	public class CustomActions
	{
		[CustomAction("CA01")]
		public static ActionResult RegisterKeyboard(Session session)
		{
			var customActionData = session["CustomActionData"];
			var array = customActionData.Split('|');
			var filePath = array[0];
			var lcid = array[1];

			session.Log("Registering keyboard layout.");
			session.Log("DLL = {0}", filePath);
			session.Log("LCIDValue = {0}", lcid);

			return ActionResult.Success;
		}

		[CustomAction("CA02")]
		public static ActionResult UnregisterKeyboard(Session session)
		{
			// TODO: how do we know which keyboard layout needs to be unregistered?
			return ActionResult.Success;
		}
	}
}