using Contact_Helper.Bases;

namespace Contact_Helper.Contacts
{
    public class ContactDetailsViewModel : BaseViewModel
    {
        public ContactDetailsViewModel(Contact contact) { Contact = contact; }

        public Contact Contact { get; }
    }
}
