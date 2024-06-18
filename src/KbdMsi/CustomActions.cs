using System;
using WixToolset.Dtf.WindowsInstaller;

namespace KbdMsi
{
	public class CustomActions
	{
		[CustomAction("CA01")]
		public static ActionResult RegisterKeyboard(Session session)
			=> TryCatchProcessProductCode(
				session, (_, productCode) =>
				{
					var customActionData = session["CustomActionData"];
					var array = customActionData.Split('|');
					var filePath = array[0];
					var lcid = array[1];

					session.Log("Registering keyboard layout:");
					session.Log("Layout File Path = {0}", filePath);
					session.Log("LCID = {0}", lcid);

					var klid = KeyboardLayoutUtils.RegisterKeyboard(
						productCode,
						filePath,
						lcid
					);

					session.Log("KLID = {0}", klid);

					return ActionResult.Success;
				});

		[CustomAction("CA02")]
		public static ActionResult UnregisterKeyboard(Session session)
			=> TryCatchProcessProductCode(
				session, (_, productCode) =>
				{
					KeyboardLayoutUtils.UnregisterKeyboard(productCode);
					return ActionResult.Success;
				});

		[CustomAction("CA03")]
		public static ActionResult AddKeyboardToLangBar(Session session)
			=> TryCatchProcessProductCode(
				session, (_, productCode) =>
				{
					KeyboardLayoutUtils.AddKeyboardToLangBar(productCode);
					return ActionResult.Success;
				});

		private static ActionResult TryCatchProcessProductCode(Session session, Func<Session, Guid, ActionResult> processor)
		{
			try
			{
				if (!Guid.TryParse(session["ProductCode"], out var productCode))
				{
					session.Log("Error: invalid MSI product code");
					return ActionResult.Failure;
				}

				return processor(session, productCode);
			}
			catch (Exception e)
			{
				session.Log($"Error: {e.Message}");
				session.Log(e.StackTrace);
				return ActionResult.Failure;
			}
		}
	}
}