
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Communication = Microsoft.Maui.ApplicationModel.Communication;

namespace Contact_Helper.Contacts
{
    
    internal class ContactManager
    {
        public ObservableCollection<Contact> ContactsList { get; set; }
        public bool IsBusy { get; set; }

        public async IAsyncEnumerable<string> GetContactNames()
        {
            if (await Permissions.RequestAsync<Permissions.ContactsRead>() != PermissionStatus.Granted)
                yield return null; ;
            var contacts = await Communication.Contacts.Default.GetAllAsync();

            // No contacts
            if (contacts == null)
                yield break;

            foreach (var contact in contacts)
                yield return contact.DisplayName;
        }

        public async Task<int> GetAllContacts()
        {
            if (await Permissions.RequestAsync<Permissions.ContactsRead>() != PermissionStatus.Granted)
                return -999;

            if (IsBusy)
                return -111;
            IsBusy = true;

            ContactsList.Clear();
            try
            {
                var contacts = await Communication.Contacts.Default.GetAllAsync();

                await Task.Run(() =>
                {
                    foreach (var contact in contacts)
                    {
                        MainThread.BeginInvokeOnMainThread(() => ContactsList.Add(contact));
                    }
                });
            }
            catch (Exception ex)
            {
                Notify.NotifyVLong($"Error:{ex.Message}");
                return -1;
            }
            finally
            {
                IsBusy = false;

            }
            return ContactsList.Count;
        }
    }
}
