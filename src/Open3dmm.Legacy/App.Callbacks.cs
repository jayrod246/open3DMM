using static Open3dmm.KnownMessageValues;
using static Open3dmm.MessageFlags;

namespace Open3dmm
{
	partial class App
	{
		protected override MessageCallbackCollection GetCallbacks() => Events<App>.Callbacks;

		static App()
		{
			Events<App>.Init(
				// APP::FUN_0040b330       
				new((int)Mvu_ShowConfigDialog, Self | Broadcast, null),
				// APP::GoToStudio        
				new((int)App_GoToStudio, Self | Broadcast, (x, m) => (x as App).GoToStudio(m)),
				// APP::FUN_0040ab10       
				new((int)App_GoToBuilding, Self | Broadcast, null),
				// APP::CreateTheatre      
				new((int)App_CreateTheatre, Self | Broadcast, null),
				// APP::ReleaseTheatre     
				new((int)App_DeleteTheatre, Self | Broadcast, null),
				// APP::FUN_0040ada0       
				new((int)App_ShowOpenFileDialog, Self | Broadcast, null),
				// APP::FUN_0040ad80       
				new((int)App_DiscardMovie, Self | Broadcast, null),
				// APP::FUN_0040c960       
				new((int)App_EnableDisableHotkeys, Self | Broadcast, null),
				// APP::FUN_0040c970       
				new((int)App_SwapAcceleratorTable, Self | Broadcast, null),
				// APP::FUN_0040c980       
				new((int)App_StartSplotMachine, Self | Broadcast, null),
				// APP::FUN_0040abc0       
				new((int)App_ShowErrorMessage, Self | Broadcast, null),
				// APP::FUN_0040a430       
				new((int)App_ShowModalWindow, Self | Broadcast, null)
			);
		}
	}
}
