using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Contact_Helper.Bases;
using Contact_Helper.Contacts;
using System.Collections.ObjectModel;
using ContactsManager = Microsoft.Maui.ApplicationModel.Communication.Contacts;
using VCard = MixERP.Net.VCards.VCard;

namespace Contact_Helper.VCF
{


    internal partial class VcfHelper : BaseViewModel
    {
        [ObservableProperty]
        ListView _contactListView;

        [ObservableProperty]
        private ObservableCollection<VCard> _vCards;

        [ObservableProperty]
        private ObservableCollection<AksContact> _contacts;

        [ObservableProperty]
        private ObservableCollection<ContactExt> _contactExts;
        [ObservableProperty]
        private string _fileName;

        [ObservableProperty]
        Contact selectedContact;

        [ObservableProperty]
        public ObservableCollection<Contact> contactsList;

        public VcfHelper()
        {
            //OnGetAllContact();
        }

        public static AppContext _db;//= new AppContext();

        private static string CleanPhoneNumber(string phoneNumber)
        {
            phoneNumber = phoneNumber.Trim();
            phoneNumber = phoneNumber.TrimStart('0');
            phoneNumber = phoneNumber.TrimStart('.');
            phoneNumber = phoneNumber.TrimStart(',');
            phoneNumber = phoneNumber.Replace(" ", "");
            phoneNumber = phoneNumber.Replace("-", "");
            phoneNumber = phoneNumber.Replace("++", "+");

            if (phoneNumber.StartsWith("091") && phoneNumber.Length > 11)
            {
                phoneNumber = phoneNumber.Replace("091", "+91");
            }
            if (phoneNumber.StartsWith("91") && phoneNumber.Length > 11)
            {
                phoneNumber = "+" + phoneNumber;
            }
            if (phoneNumber.Length > 10 && phoneNumber.StartsWith("0"))
            {
                phoneNumber = phoneNumber.Remove(1, 1);
            }

            if (phoneNumber.Length > 10 && phoneNumber.StartsWith("00"))
            {
                phoneNumber = phoneNumber.Remove(1, 2);
                phoneNumber = "+" + phoneNumber;
            }

            if (phoneNumber.Length == 10)
            {
                phoneNumber = string.Concat("+91", phoneNumber);
            }
            return phoneNumber;
        }

        [RelayCommand]
        private async Task ToContactExt()
        {
            foreach (var cont in VCards)
            {
                var c = cont.ToContactExt();
                ContactExts.Add(c);
                //_db.ContactExts.Add(c);
            }

            bool flag = await _db.SaveChangesAsync() > 0;
            if (flag) Notify.NotifyLong("Added and Saved contact ext"); else Notify.NotifyLong("Fail to add and Save contact ext");
        }
        [RelayCommand]
        private async Task ToContacts()
        {
            foreach (var cont in ContactExts)
            {
                var c = cont.ToAKSContact();
                Contacts.Add(c);
                //_db.Contacts.Add(c);
            }

            bool flag = await _db.SaveChangesAsync() > 0;
            if (flag) Notify.NotifyLong("Added and Saved contacts"); else Notify.NotifyLong("Fail to add and Save contacts");
        }
        [RelayCommand]
        private async Task ToContactCleaned()
        {
            foreach (var cont in ContactExts)
            {
                var c = cont.ToAKSContactClean();
                Contacts.Add(c);
                c.Id = 0;
                //_db.Contacts.Add(c);
            }

            bool flag = await _db.SaveChangesAsync() > 0;
            if (flag) Notify.NotifyLong("Added and Saved contacts which is cleaned"); else Notify.NotifyLong("Fail to add and Save contacts Cleaned");
        }
        [RelayCommand]
        async Task BreakContacts()
        {
            if (Contacts == null && Contacts.Count <= 0)
            {
                //var x = _db.Contacts.ToList();
                //foreach (var item in x)
                //{
                //    Contacts.Add(item);
                //}


            }

            List<AksContact> cleanC = new List<AksContact>();
            List<AContact> cleanA = new List<AContact>();
            foreach (var cont in Contacts)
            {
                var phones = cont.Phone.Split(';');

                foreach (var ph in phones)
                {
                    var phone = CleanPhoneNumber(ph);

                    AContact ac = new AContact
                    {
                        Email = cont.Email,
                        GivenName = cont.FirstName,
                        FamilyName = cont.LastName,
                        MiddleName = cont.MiddleName,
                        NamePrefix = cont.NamePrefix,
                        NameSuffix = cont.NameSuffix,
                        Phone = phone,
                        Status = "#",
                        TrueCallerName = "",
                    };
                    var aks = cont;
                    aks.Phone = phone;
                    cleanA.Add(ac);
                    cleanC.Add(aks);
                }
            }

            cleanC = cleanC.DistinctBy(c => c.Phone).ToList();
            cleanA = cleanA.DistinctBy(c => c.Phone).ToList();
            _db.AContacts.AddRange(cleanA);
           // _db.Contacts.AddRange(cleanC);
            var count = await _db.SaveChangesAsync();
            if (count > 0) Notify.NotifyVLong("Saved"); else Notify.NotifyVShort("Error");

        }
        [RelayCommand]
        private async Task ReadVCFFile(string filePath)
        {
            if (!string.IsNullOrEmpty(filePath)) this.FileName = filePath;
            IEnumerable<VCard> vcards = (IEnumerable<VCard>)MixERP.Net.VCards.Deserializer.Deserialize(FileName);
            // string contents = File.ReadAllText(filePath, Encoding.UTF8);
            // vcards = (IEnumerable<VCard>)MixERP.Net.VCards.Deserializer.GetVCards(contents);

            foreach (var vcard in vcards)
            {
                //var x = vcard.ToContactExt();
                VCards.Add(vcard);
            }
            Notify.NotifyShort($"Read {VCards.Count}  contacts");
        }
        [RelayCommand]
        private async Task ReadVCFFileToContactExt(string filePath)
        {
            if (!string.IsNullOrEmpty(filePath)) this.FileName = filePath;
            IEnumerable<VCard> vcards = (IEnumerable<VCard>)MixERP.Net.VCards.Deserializer.Deserialize(FileName);
            // string contents = File.ReadAllText(filePath, Encoding.UTF8);
            // vcards = (IEnumerable<VCard>)MixERP.Net.VCards.Deserializer.GetVCards(contents);

            foreach (var vcard in vcards)
            {
                var x = vcard.ToContactExt();
                VCards.Add(vcard);
                ContactExts.Add(x);
            }
            Notify.NotifyShort($"Read {VCards.Count}  contacts");
        }

        [RelayCommand]
        async Task OnGetAllContact()
        {
            //if (await Permissions.RequestAsync<Permissions.ContactsRead>() != PermissionStatus.Granted)
            //  return;

            if (IsBusy)
                return;
            IsBusy = true;
            if (ContactsList == null) ContactsList = new ObservableCollection<Contact>();
            else
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
    }
}