﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using ContactsManager = Microsoft.Maui.ApplicationModel.Communication.Contacts;

namespace Contact_Helper.Contacts
{
    public partial class ContactListPage : BasePage
    {
        public ContactListPage() { InitializeComponent(); }
    }

    public partial class BaseViewModel : ObservableObject
    {
        [ObservableProperty]
        public bool isBusy;

        public bool IsNotBusy => !IsBusy;

        internal event Func<string, Task> DoDisplayAlert;

        internal event Func<BaseViewModel, bool, Task> DoNavigate;

        public Task NavigateAsync(BaseViewModel vm, bool showModal = false)
        { return DoNavigate?.Invoke(vm, showModal) ?? Task.CompletedTask; }

        public void OnAppearing()
        {
            //throw new NotImplementedException();
        }

        public void OnDisappearing()
        {
            // throw new NotImplementedException();
        }
    }

    public class ContactDetailsViewModel : BaseViewModel
    {
        public ContactDetailsViewModel(Contact contact) { Contact = contact; }

        public Contact Contact { get; }
    }

    public partial class ContactsViewModel : BaseViewModel
    {
        public ContactsViewModel() { contactsList = new ObservableCollection<Contact>(); }
        [ObservableProperty]
        Contact selectedContact;
        static DBClass db = new DBClass();
        [ObservableProperty]
        public ObservableCollection<Contact> contactsList;

        public async IAsyncEnumerable<string> GetContactNames()
        {
            if (await Permissions.RequestAsync<Permissions.ContactsRead>() != PermissionStatus.Granted)
                yield return null;
            ;
            var contacts = await Microsoft.Maui.ApplicationModel.Communication.Contacts.Default.GetAllAsync();

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
                var contacts = await Microsoft.Maui.ApplicationModel.Communication.Contacts.Default.GetAllAsync();

                await Task.Run(
                    () =>
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

        [RelayCommand]
        async Task OnGetContact()
        {
            if (IsBusy)
                return;
            IsBusy = true;

            try
            {
                var contact = await ContactsManager.PickContactAsync();
                if (contact == null)
                    return;


                var details = new ContactDetailsViewModel(contact);
                await NavigateAsync(details);
            }
            catch (Exception ex)
            {
                Notify.NotifyVShort($"Error:{ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        async Task OnGetAllContact()
        {
            //if (await Permissions.RequestAsync<Permissions.ContactsRead>() != PermissionStatus.Granted)
            //  return;

            if (IsBusy)
                return;
            IsBusy = true;

            ContactsList.Clear();
            try
            {
                var contacts = await ContactsManager.GetAllAsync();

                await Task.Run(
                    () =>
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
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        async Task OnContactSelected()
        {
            if (SelectedContact == null)
                return;

            var details = new ContactDetailsViewModel(SelectedContact);

            SelectedContact = null;

            await NavigateAsync(details);
        }

        [RelayCommand]
        async Task FromDatabase()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                var x = (await db.GetContactsAsync());
                //x.Sort();
                Notify.NotifyVShort($"Count is {x.Count}");
                ContactsList.Clear();

                foreach (var item in x)
                {
                    ContactsList.Add(new Contact { FamilyName=item.FamilyName, GivenName=item.GivenName, 
                        Phones= new List<ContactPhone> { new ContactPhone { PhoneNumber=item.Phone } },
                        Emails= new List<ContactEmail> { new ContactEmail {EmailAddress=item.Email } },
                        Id=item.IDS, MiddleName=item.MiddleName, NamePrefix=item.NamePrefix, NameSuffix=item.NameSuffix });
                }
            }
            catch (Exception ex)
            {
                Notify.NotifyVLong($"Error:{ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }

        }
        [RelayCommand]
        async Task DuplicateContact()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                var x = await db.GetContactsAsync();
                var z = x.GroupBy(c => c.Phone).Where(x => x.Count() > 1).Select(x => new Contact
                {
                    GivenName = x.Select(c => c.GivenName).First(),
                    MiddleName = x.Select(c => c.MiddleName).First(),
                    Phones = new List<ContactPhone> { new ContactPhone { PhoneNumber = x.Key } },
                    NameSuffix = x.Select(c => c.NameSuffix).First(),
                    Emails = new List<ContactEmail> { new ContactEmail { EmailAddress= x.Select(c => c.Email).First() } },
                    NamePrefix = x.Select(c => c.NamePrefix).First(),
                    FamilyName = x.Select(c => c.FamilyName).First(),
                    Id = x.Select(c => c.IDS).First()
                }).ToList();
                ContactsList.Clear();
                foreach (var item in z)
                {
                    ContactsList.Add(item);
                }
            }
            catch (Exception ex)
            {
                Notify.NotifyVLong($"Error:{ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        async Task SaveToDataBase()
        {
            if (ContactsList.Count < 1)
            {
                await GetAllContacts();
            }

            int Count = 0,Saved=0;
            foreach (var contact in ContactsList)
            {
                // List<Contact> newcc = new List<Contact>();

                if (contact.Phones != null)
                {
                    int ctr = 0;
                    
                    foreach (var phone in contact.Phones)
                    {
                        var cc = new AContact
                        {
                            MiddleName = contact.MiddleName,
                            NamePrefix = contact.NamePrefix,
                            FamilyName = contact.FamilyName,
                            GivenName = contact.GivenName,
                            IDS = contact.Id,
                            NameSuffix = contact.NameSuffix,
                            Phone =  phone.PhoneNumber,
                            Email= contact.Emails!=null && contact.Emails.Any() ? contact.Emails[0].EmailAddress:""
                        };
                        // newcc.Add(cc);
                       Saved+=await db.SaveContactAsync(cc); Count++;

                    }
                }
            }
            Notify.NotifyVShort($"Save Contact {Count} from {ContactsList.Count}.");
        }
    }
}
