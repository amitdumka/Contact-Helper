using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Contact_Helper.Bases;
using Microsoft.EntityFrameworkCore;
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
        private ListView _contactListView;

        /// <summary>
        /// Contacts and ContactExt Model of Custom contact model
        /// </summary>
        [ObservableProperty]
        private ListView _contactListView2;

        /// <summary>
        /// VCards  using VCard model from VCF extension
        /// </summary>
        [ObservableProperty]
        private ListView _contactListView3;

        [ObservableProperty]
        private ObservableCollection<VCard> _vCards;

        [ObservableProperty]
        private ObservableCollection<AksContact> _contacts;

        [ObservableProperty]
        private ObservableCollection<ContactExt> _contactExts;

        [ObservableProperty]
        private string _fileName;

        [ObservableProperty]
        private Contact selectedContact;

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
            try
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
            catch (Exception ex)
            {
                Notify.NotifyLong(ex.Message);
            }
        }

        [RelayCommand]
        private async Task ToContacts()
        {
            try
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
            catch (Exception ex)
            {
                Notify.NotifyLong(ex.Message);
            }
        }

        [RelayCommand]
        private async Task ToContactCleaned()
        {
            try
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
            catch (Exception ex)
            {
                Notify.NotifyLong(ex.Message);
            }
        }

        [RelayCommand]
        private async Task BreakContacts()
        {
            try
            {
                if (Contacts == null && Contacts.Count <= 0)
                {
                    var x = _db.Contacts.ToList();
                    foreach (var item in x)
                    {
                        Contacts.Add(item);
                    }
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
                            Status = "ERROR",
                            TrueCallerName = "ERROR",
                        };
                        var aks = cont;
                        aks.Telephone = phone;
                        aks.TrueCallerName = "ERROR";
                        cleanA.Add(ac);
                        cleanC.Add(aks);
                    }
                }

                cleanC = cleanC.DistinctBy(c => c.Telephone).ToList();
                cleanA = cleanA.DistinctBy(c => c.Phone).ToList();
                _db.AContacts.AddRange(cleanA);
                _db.Contacts.RemoveRange(Contacts.ToList());
                _db.Contacts.AddRange(cleanC);
                var count = await _db.SaveChangesAsync();
                if (count > 0) Notify.NotifyVLong("Saved"); else Notify.NotifyVShort("Error");
            }
            catch (Exception ex)
            {
                Notify.NotifyLong(ex.Message);
            }
        }

        [RelayCommand]
        private async Task ReadVCFFile()
        {
            try
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
                //string contents = File.ReadAllText(FileName, Encoding.UTF8);
                //var vcards = (IEnumerable<VCard>)MixERP.Net.VCards.Deserializer.GetVCards(contents);
                //if (VCards == null) VCards = new ObservableCollection<VCard>();
                //else
                //    VCards.Clear();
                //foreach (var vcard in vcards)
                //{
                //    //var x = vcard.ToContactExt();
                //    VCards.Add(vcard);
                //}
                //Notify.NotifyShort($"Read {VCards.Count}  contacts");
                await ReadVCFFile(FileName, true, false);

                //  Notify.NotifyShort($"Read {count}  contacts");
                if (ContactListView3 != null)
                {
                    ContactListView3.ItemsSource = VCards;
                    ContactListView3.IsVisible = true;
                    ContactListView.IsVisible = false;
                    ContactListView2.IsVisible = false;
                }
            }
            catch (Exception ex)
            {
                Notify.NotifyLong(ex.Message);
            }
        }

        [RelayCommand]
        private async Task ReadVCFFileToContactExt()
        {
            try
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
                //IEnumerable<VCard> vcards = (IEnumerable<VCard>)MixERP.Net.VCards.Deserializer.Deserialize(FileName);
                // string contents = File.ReadAllText(filePath, Encoding.UTF8);
                // vcards = (IEnumerable<VCard>)MixERP.Net.VCards.Deserializer.GetVCards(contents);
                //if (VCards == null) VCards = new ObservableCollection<VCard>();
                //else
                //  VCards.Clear();
                //if (ContactExts == null) ContactExts = new ObservableCollection<ContactExt>();
                //ContactExts.Clear();
                //foreach (var vcard in vcards)
                //{
                //    var x = vcard.ToContactExt();
                //    VCards.Add(vcard);
                //    ContactExts.Add(x);
                //}
                ReadVCFFile(FileName, true, true);

                // Notify.NotifyShort($"Read {count}  contacts");
                if (ContactListView2 != null)
                {
                    ContactListView2.ItemsSource = ContactExts;
                    ContactListView2.IsVisible = true;
                    ContactListView.IsVisible = false;
                    ContactListView3.IsVisible = false;
                }
            }
            catch (Exception ex)
            {
                Notify.NotifyLong(ex.Message);
            }
        }

        private async Task<int> ReadVCFFile(string fileName, bool toVcard = true, bool toContactExt = false)
        {
            string readContents = await File.ReadAllTextAsync(fileName, Encoding.UTF8);
            int BeginIndexInString = readContents.IndexOf("BEGIN:VCARD");
            int count = 0, skip = 0;

            if (toVcard)
            {
                if (VCards == null) VCards = new ObservableCollection<VCard>();
                else
                    VCards.Clear();
            }
            if (toContactExt)
            {
                if (ContactExts == null) ContactExts = new ObservableCollection<ContactExt>();
                else
                    ContactExts.Clear();
            }
            try
            {
                while (BeginIndexInString != -1)
                {
                    int EndIndexInString = readContents.IndexOf("END:VCARD", BeginIndexInString);
                    string OneVcf = readContents.Substring(BeginIndexInString, EndIndexInString - BeginIndexInString);

                    try
                    {
                        VCard vcards = (VCard)MixERP.Net.VCards.Deserializer.GetVCard(OneVcf);
                        await Task.Run(
                       () =>
                       {
                           if (toVcard)
                               VCards.Add(vcards);
                           if (toContactExt)
                               ContactExts.Add(((VCard)vcards).ToContactExt());
                       });
                    }
                    catch (Exception ex)
                    {
                        skip++;
                        // Console.WriteLine(ex.ToString());
                        //Console.WriteLine(OneVcf);
                        //Notify.NotifyVLong(OneVcf);
                    }

                    BeginIndexInString = readContents.IndexOf("BEGIN:VCARD", EndIndexInString);
                    count++;
                    // if (count > 200) return count;
                }
                Notify.NotifyVLong($"Total Skipped Contact {skip} out of {count}");
                return count;
            }
            catch (Exception ex)
            {
                return -1;
            }
        }

        private List<VCard> ReadVCFFileToVCards(string fileName)
        {
            string readContents = File.ReadAllText(fileName, Encoding.UTF8);
            int BeginIndexInString = readContents.IndexOf("BEGIN:VCARD");

            var vCardList = new List<VCard>();

            while (BeginIndexInString != -1)
            {
                int EndIndexInString = readContents.IndexOf("END:VCARD", BeginIndexInString);

                string OneVcf = readContents.Substring(BeginIndexInString, EndIndexInString - BeginIndexInString);
                var vcards = (VCard)MixERP.Net.VCards.Deserializer.GetVCard(OneVcf);
                vCardList.Add((VCard)vcards);
                BeginIndexInString = readContents.IndexOf("BEGIN:VCARD", EndIndexInString);
            }
            return vCardList;
        }

        private List<ContactExt> ReadVCFFileToContactExt(string fileName)
        {
            string readContents = File.ReadAllText(fileName, Encoding.UTF8);
            int BeginIndexInString = readContents.IndexOf("BEGIN:VCARD");

            var vCardList = new List<ContactExt>();

            while (BeginIndexInString != -1)
            {
                int EndIndexInString = readContents.IndexOf("END:VCARD", BeginIndexInString);

                string OneVcf = readContents.Substring(BeginIndexInString, EndIndexInString - BeginIndexInString);
                var vcards = (VCard)MixERP.Net.VCards.Deserializer.GetVCard(OneVcf);
                vCardList.Add(((VCard)vcards).ToContactExt());
                BeginIndexInString = readContents.IndexOf("BEGIN:VCARD", EndIndexInString);
            }
            return vCardList;
        }

        [RelayCommand]
        private async Task DoubtFullContacts()
        {
            try
            {
                var x = await _db.Contacts.Where(c => c.TrueCallerName == "ERROR").ToListAsync();
                if (x != null && x.Count > 0)
                {
                    foreach (var contact in x)
                    {
                        Contacts.Add(contact);
                    }
                    Notify.NotifyVShort("Contacts are listed in view");
                }
                else
                {
                    Notify.NotifyVShort("No Contact found!");
                }
            }
            catch (Exception ex)
            {
                Notify.NotifyLong(ex.Message);
            }
        }

        [RelayCommand]
        private async Task SearchInTrueCaller()
        {
            try
            {
                var x = _db.AContacts.Where(c => c.Status.Contains("ERROR")).Take(50).ToList();
                int count = 0, Saved = 0;
                if (x != null)
                {
                    foreach (var contact in x)
                    {
                        var searchData = await APIServer.SearchNumberByName(contact.Phone, true, false, true);
                        if (searchData != null && searchData.Status.ToLower().Contains("error") == false)
                        {
                            if (searchData.Name.ToLower().Contains("unkown contact") == false)
                            {
                                contact.TrueCallerName = searchData.Name;
                                contact.Status = searchData.Status;
                                _db.AContacts.Update(contact);
                                Saved++;
                            }
                            else
                            {
                                count++;
                                if (count > 10) break;
                            }
                        }
                    }
                    var c = await _db.SaveChangesAsync() > 0;
                    if (c) Notify.NotifyVShort($"Searched {Saved} contacts");
                    else Notify.NotifyVShort($"Not able to searched contacts");
                }
            }
            catch (Exception ex)
            {
                Notify.NotifyLong(ex.Message);
            }
        }

        [RelayCommand]
        private async Task OnGetAllContact()
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

//private void SliptVcard1() {
//    DialogResult result = folderBrowserDialog1.ShowDialog();
//    if (result == DialogResult.OK)
//    {
//        string folderName = folderBrowserDialog1.SelectedPath;
//        string readContents;
//        using (StreamReader streamReader = new StreamReader(fileName))
//            readContents = streamReader.ReadToEnd();
//        int BeginIndexInString = readContents.IndexOf("BEGIN:VCARD");
//        while (BeginIndexInString != -1)
//        {
//            int EndIndexInString = readContents.IndexOf("END:VCARD", BeginIndexInString);
//            string OneVcf = readContents.Substring(BeginIndexInString, EndIndexInString - BeginIndexInString);
//            int b = OneVcf.IndexOf("FN:");
//            int a = OneVcf.LastIndexOf("N:", b);
//            string VcfName = OneVcf.Substring(a, b - a);
//            VcfName = Regex.Replace(VcfName, @"N:|;|:|\r|\n", "");
//            File.WriteAllText(folderName + "\\" + VcfName + ".vcf", OneVcf, Encoding.Default);
//            BeginIndexInString = readContents.IndexOf("BEGIN:VCARD", EndIndexInString);
//        }
//        MessageBox.Show("Done!");

//    }
//private void SliptVcard2() {
//    String source = this.tbSource.Text;
//    String target = this.tbTarget.Text;

//    if (!File.Exists(source)) return;
//    if (!Directory.Exists(target)) return;

//    String[] lines = File.ReadAllLines(source);
//    String contents = String.Empty;
//    String name = String.Empty;
//    Boolean isBegin = false;
//    Boolean isEnd = false;
//    Int32 iFiles = 0;
//    this.progress.Maximum = lines.Length;
//    foreach (String line in lines)
//    {
//        if (line == "BEGIN:VCARD") isBegin = true;
//        if (line == "END:VCARD") isEnd = true;
//        ////if (line.StartsWith("FN:")) name = line.Substring(3) + ".vcf";
//        if (isBegin) contents += System.Environment.NewLine + line;
//        if (isEnd)
//        {
//            iFiles++;

//            name = String.Format($"vcard_{iFiles:D3}.vcf");
//            File.WriteAllText(Path.Combine(target, name), contents);
//            isBegin = false;
//            isEnd = false;
//            contents = String.Empty;
//        }
//        this.progress.PerformStep();
//    }

//    MessageBox.Show("Your VCard was split into " + iFiles.ToString() + " files.", "Success", MessageBoxButtons.OK);
//}  