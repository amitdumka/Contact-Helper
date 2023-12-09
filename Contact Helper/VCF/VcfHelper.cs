using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Contact_Helper.Bases;
using System.Collections.ObjectModel;
using System.Text;
using ContactsManager = Microsoft.Maui.ApplicationModel.Communication.Contacts;
using VCard = MixERP.Net.VCards.VCard;

namespace Contact_Helper.VCF
{


    internal partial class VcfHelper : BaseViewModel
    {
        /// <summary>
        /// ContactList  using Contact Model from MAUI API
        /// </summary>
        [ObservableProperty]
        ListView _contactListView;
        /// <summary>
        /// Contacts and ContactExt Model of Custom contact model
        /// </summary>
        [ObservableProperty]
        ListView _contactListView2;
        /// <summary>
        /// VCards  using VCard model from VCF extension
        /// </summary>
        [ObservableProperty]
        ListView _contactListView3;

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
            //_db=appContext;
           // OnGetAllContact();
        }
        public void SetDB(AppContext appContext)
        {
            _db = appContext;
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
                _db.ContactExts.Add(c);
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
                _db.Contacts.Add(c);
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
                _db.Contacts.Add(c);
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
                var phones = cont.Telephone.Split(';');

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
                    aks.Telephone = phone;
                    cleanA.Add(ac);
                    cleanC.Add(aks);
                }
            }

            cleanC = cleanC.DistinctBy(c => c.Telephone).ToList();
            cleanA = cleanA.DistinctBy(c => c.Phone).ToList();
            _db.AContacts.AddRange(cleanA);
            _db.Contacts.AddRange(cleanC);
            var count = await _db.SaveChangesAsync();
            if (count > 0) Notify.NotifyVLong("Saved"); else Notify.NotifyVShort("Error");

        }
        [RelayCommand]
        private async Task ReadVCFFile()
        {
            //if (!string.IsNullOrEmpty(filePath)) this.FileName = filePath;
            PickOptions options = new()
            {
                PickerTitle = "Please select a VCF file",

            };
            var result = await FilePicker.Default.PickAsync(options);
            if (result != null)
            {
                if (result.FileName.EndsWith("vcf", StringComparison.OrdinalIgnoreCase) || result.FileName.EndsWith("vcf", StringComparison.OrdinalIgnoreCase))
                {
                    FileName = result.FullPath;
                }
                else
                {
                    Notify.NotifyVShort($"{result.FileName} is not valid, select only Vcf File");
                    return;
                }
            }
            else
            {
                Notify.NotifyVShort($"Operation Cancled!");
                return;
            }

            // IEnumerable<VCard> vcards = (IEnumerable<VCard>)MixERP.Net.VCards.Deserializer.Deserialize(FileName);
            string contents = File.ReadAllText(FileName, Encoding.UTF8);
            var vcards = (IEnumerable<VCard>)MixERP.Net.VCards.Deserializer.GetVCards(contents);
            if (VCards == null) VCards = new ObservableCollection<VCard>();
            else
                VCards.Clear();
            foreach (var vcard in vcards)
            {
                //var x = vcard.ToContactExt();
                VCards.Add(vcard);
            }
            Notify.NotifyShort($"Read {VCards.Count}  contacts");
            if (ContactListView3 != null)
            {
                ContactListView3.ItemsSource = VCards;
                ContactListView3.IsVisible = true;
                ContactListView.IsVisible = false;
                ContactListView2.IsVisible = false;
            }
        }
        [RelayCommand]
        private async Task ReadVCFFileToContactExt()
        {
            PickOptions options = new()
            {
                PickerTitle = "Please select a excel file",

            };
            var result = await FilePicker.Default.PickAsync(options);
            if (result != null)
            {
                if (result.FileName.EndsWith("vcf", StringComparison.OrdinalIgnoreCase) || result.FileName.EndsWith("vcf", StringComparison.OrdinalIgnoreCase))
                {
                    FileName = result.FullPath;
                }
                else
                {
                    Notify.NotifyVShort($"{result.FileName} is not valid, select only Vcf File");
                    return;
                }
            }
            else
            {
                Notify.NotifyVShort($"Operation Cancled!");
                return;
            }
            IEnumerable<VCard> vcards = (IEnumerable<VCard>)MixERP.Net.VCards.Deserializer.Deserialize(FileName);
            // string contents = File.ReadAllText(filePath, Encoding.UTF8);
            // vcards = (IEnumerable<VCard>)MixERP.Net.VCards.Deserializer.GetVCards(contents);
            if (VCards == null) VCards = new ObservableCollection<VCard>();
            else
                VCards.Clear();
            if(ContactExts == null) ContactExts= new ObservableCollection<ContactExt> ();
            ContactExts.Clear();
            foreach (var vcard in vcards)
            {
                var x = vcard.ToContactExt();
                VCards.Add(vcard);
                ContactExts.Add(x);
            }
            Notify.NotifyShort($"Read {VCards.Count}  contacts");
            if (ContactListView2 != null)
            {
                ContactListView2.ItemsSource = ContactExts;
                ContactListView2.IsVisible = true;
                ContactListView.IsVisible = false;
                ContactListView3.IsVisible = false;
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
                if (ContactListView != null)
                {
                    //ContactListView.ItemsSource = ContactListView;
                    ContactListView.IsVisible = true;
                    ContactListView2.IsVisible = false;
                    ContactListView3.IsVisible = false;
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
    }
}