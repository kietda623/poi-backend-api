using CommunityToolkit.Maui.Views;

namespace AppUser.Pages.Popups;

public partial class TourPlanResultPopup : Popup
{
	public TourPlanResultPopup(string result)
	{
		InitializeComponent();
		BindingContext = result;
	}

	private void OnCloseClicked(object sender, EventArgs e)
	{
		Close();
	}
}
