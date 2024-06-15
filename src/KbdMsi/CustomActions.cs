using WixToolset.Dtf.WindowsInstaller;

namespace KbdMsi
{
	public class CustomActions
	{
		[CustomAction("SampleCA")]
		public static ActionResult SampleCustomAction2(Session session)
		{
			using (Record msgRec = new Record(0))
			{
				msgRec[0] = "Hello from SampleCA!";
				session.Message(InstallMessage.Info, msgRec);
				session.Message(InstallMessage.User, msgRec);
			}
			session.Log("Testing summary info...");
			SummaryInfo summInfo = session.Database.SummaryInfo;
			session.Log("MSI PackageCode = {0}", summInfo.RevisionNumber);
			session.Log("MSI ModifyDate = {0}", summInfo.LastSaveTime);
			return ActionResult.UserExit;
		}
	}
}