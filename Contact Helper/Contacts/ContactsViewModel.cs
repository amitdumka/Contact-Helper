using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Contact_Helper.Bases;
using System.Collections.ObjectModel;
using ContactsManager = Microsoft.Maui.ApplicationModel.Communication.Contacts;

namespace Contact_Helper.Contacts
{
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
                    ContactsList.Add(new Contact
                    {
                        FamilyName = item.FamilyName,
                        GivenName = item.GivenName,
                        Phones = new List<ContactPhone> { new ContactPhone { PhoneNumber = item.Phone } },
                        Emails = new List<ContactEmail> { new ContactEmail { EmailAddress = item.Email } },
                        Id = item.IDS,
                        MiddleName = item.MiddleName,
                        NamePrefix = item.NamePrefix,
                        NameSuffix = item.NameSuffix
                    });
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
        async Task RemoveDuplicate()
        {
            int count = 0;
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                var x = await db.GetContactsAsync();
                var z = x.GroupBy(c => c.Phone).Where(x => x.Count() > 1).Select(x => new AContact
                {
                    GivenName = x.Select(c => c.GivenName).First(),
                    MiddleName = x.Select(c => c.MiddleName).First(),
                    Phone =x.Key,
                    NameSuffix = x.Select(c => c.NameSuffix).First(),
                    Email = x.Select(c => c.Email).First(),
                    NamePrefix = x.Select(c => c.NamePrefix).First(),
                    FamilyName = x.Select(c => c.FamilyName).First(),
                    IDS = x.Select(c => c.IDS).First(),
                    Id = x.Select(c => c.Id).First()
                }).ToList();

                foreach (var c in z)
                {
                    var ccs = x.Where(c => c.Phone == c.Phone).ToList();
                    int ctr = 0;
                    foreach (var item in ccs)
                    {
                        if (ctr > 0)
                        {
                           count+= await db.DeleteContactAsync(item);
                        }
                        ctr++;
                    }
                }
                Notify.NotifyVLong($"Remove total {count} contacts");

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
                    Emails = new List<ContactEmail> { new ContactEmail { EmailAddress = x.Select(c => c.Email).First() } },
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

            int Count = 0, Saved = 0;
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
                            Phone = phone.PhoneNumber,
                            Email = contact.Emails != null && contact.Emails.Any() ? contact.Emails[0].EmailAddress : ""
                            ,
                            Id = ++ctr + Count,
                        };
                        // newcc.Add(cc);
                        Saved += await db.SaveContactAsync(cc); Count++;

                    }
                }
            }
            Notify.NotifyVShort($"Save Contact {Count} from {ContactsList.Count}.");
        }

        [RelayCommand]
        async Task TrueCallerUpdate()
        {
            var fromDb = await db.GetContactsAsync();
            var filter = fromDb.Where(c => c.Status != "OK").ToList();
            int count = 0, Save = 0, ok = 0;
            foreach (var contact in filter)
            {
                if (contact.Phone.Length >= 10)
                {
                    var name = await TCallerAPI.SearchNumberByName(contact.Phone, true);
                    if (name != null && name.Status == "Ok")
                    {
                        contact.TrueCallerName = name.Name;
                        contact.Status = "OK";
                        Notify.NotifyShort(name.Name);
                        Save += await db.UpdateContactAsync(contact);
                        ok++;
                        ContactsList.Add(new Contact
                        {
                            FamilyName = contact.FamilyName,
                            GivenName = contact.GivenName,
                            Phones = new List<ContactPhone> { new ContactPhone { PhoneNumber = contact.Phone } },
                            Emails = new List<ContactEmail> { new ContactEmail { EmailAddress = contact.Email } },
                            Id = contact.IDS,
                            MiddleName = contact.TrueCallerName,
                            NamePrefix = contact.NamePrefix,
                            NameSuffix = contact.NameSuffix
                        });
                        await Task.Delay(5000);

                    }
                    else
                    {
                        count++;
                        if (count > 20) return;
                    }
                }
            }
            if (Save != ok) Notify.NotifyVLong("Error number not matched");
            else Notify.NotifyShort("Update as much we can Total update are " + Save);
            return;
        }

        [RelayCommand]
        async Task ContactCleaner()
        {
            var contacts = await db.GetContactsAsync();
            ContactsList.Clear();

            if (contacts != null && contacts.Count > 0)
            {
                foreach (var contact in contacts)
                {
                    if (contact.Phone.Length < 9)
                    {
                        contact.Status = "REMOVE";
                        contact.NameSuffix = "Remove";
                    }
                    else
                    {
                        contact.Phone = contact.Phone.Trim();
                        contact.Phone = contact.Phone.TrimStart('0');
                        contact.Phone = contact.Phone.TrimStart('.');
                        contact.Phone = contact.Phone.TrimStart(',');
                        contact.Phone = contact.Phone.Replace(" ", "");
                        contact.Phone = contact.Phone.Replace("-", "");
                        contact.Phone = contact.Phone.Replace("++", "+");


                        if (contact.Phone.StartsWith("091") && contact.Phone.Length > 11)
                        {
                            contact.Phone = contact.Phone.Replace("091", "+91");
                        }
                        if (contact.Phone.StartsWith("91") && contact.Phone.Length > 11)
                        {
                            contact.Phone = "+" + contact.Phone;
                        }
                        if (contact.Phone.Length > 10 && contact.Phone.StartsWith("0"))
                        { 
                            contact.Phone = contact.Phone.Remove(1, 1); 
                        }

                        if (contact.Phone.Length > 10 && contact.Phone.StartsWith("00"))
                        {
                            contact.Phone = contact.Phone.Remove(1, 2);
                            contact.Phone = "+" + contact.Phone;
                        }


                        if (contact.Phone.Length == 10)
                        {
                            contact.Phone = string.Concat("+91", contact.Phone);
                        }
                    }

                    await db.UpdateContactAsync(contact);
                    ContactsList.Add(new Contact
                    {
                        FamilyName = contact.FamilyName,
                        GivenName = contact.GivenName,
                        Phones = new List<ContactPhone> { new ContactPhone { PhoneNumber = contact.Phone } },
                        Emails = new List<ContactEmail> { new ContactEmail { EmailAddress = contact.Email } },
                        Id = contact.IDS,
                        MiddleName = contact.TrueCallerName,
                        NamePrefix = contact.NamePrefix,
                        NameSuffix = contact.NameSuffix
                    });

                }
            }
        }

    }
}
