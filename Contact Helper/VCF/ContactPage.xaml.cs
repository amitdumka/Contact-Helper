using Contact_Helper.Bases;
using Contact_Helper.Contacts;

namespace Contact_Helper.VCF;

public partial class ContactPage : BasePage
{
	public ContactPage(AppContext context)
	{

        InitializeComponent();
		viewModel.SetDB(context);
		viewModel.ContactListView = this.CListView;
        viewModel.ContactListView2= this.CListView2;
        viewModel.ContactListView3 = this.CListView3;

    }
}