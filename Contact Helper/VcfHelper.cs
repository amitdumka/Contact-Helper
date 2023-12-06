using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MixERP.Net.VCards.Serializer;
using Network;
using System.Collections.ObjectModel;

using VCard = MixERP.Net.VCards.VCard;

namespace Contact_Helper
{
    internal partial class VcfHelper : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<VCard> _vCards;
        [ObservableProperty]
        private ObservableCollection<AksContact> _contacts;
        [ObservableProperty]
        private ObservableCollection<ContactExt> _contactExts;
        [ObservableProperty]
        private string _fileName;
        public static AppContext _db = new AppContext();
        private string CleanPhoneNumber(string phoneNumber)
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
            _db.Contacts.AddRange(cleanC);
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

    }
    internal partial class VcfHelper2 : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<VCard> _vCards;

        [RelayCommand]
        private async Task BreakContacts()
        {
            foreach (var vcard in VCards)
            {
                List<string> phones = vcard.Telephones.Select(c => c.Number).ToList();
                List<string> emails = vcard.Emails.Select(c => c.EmailAddress).ToList();
                // Create Contact for each Phone and check for Duplicate here and clean also  and join emails, seperate by ;
                string emailid = "";
                foreach (var email in emails) emailid += email + ";";

                foreach (var ph in phones)
                {
                    var phone = CleanPhoneNumber(ph);

                    AContact ac = new AContact
                    {
                        Email = emailid,
                        GivenName = vcard.FirstName,
                        FamilyName = vcard.LastName,
                        MiddleName = vcard.MiddleName,
                        NamePrefix = vcard.Prefix,
                        NameSuffix = vcard.Suffix,
                        Phone = phone,
                        Status = "#",
                        TrueCallerName = "",
                    };
                }
            }
        }

        [RelayCommand]
        private async Task CleanContact()
        { }

        private string CleanPhoneNumber(string phoneNumber)
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
        private async Task DBToVCard()
        { }

        [RelayCommand]
        private async Task DeleteVCFFile(string filePath)
        { }

        [RelayCommand]
        private async Task FindDuplicate()
        {
        }

        [RelayCommand]
        private async Task ReadVCFFile(string filePath)
        {
            IEnumerable<VCard> vcards = (IEnumerable<VCard>)MixERP.Net.VCards.Deserializer.Deserialize(filePath);
            // string contents = File.ReadAllText(filePath, Encoding.UTF8);
            // vcards = (IEnumerable<VCard>)MixERP.Net.VCards.Deserializer.GetVCards(contents);

            foreach (var vcard in vcards)
            {
                var x = vcard.ToContactExt();
                VCards.Add(vcard);
            }
            Notify.NotifyShort($"Read {VCards.Count}  contacts");
        }

        [RelayCommand]
        private async Task RemoveDupilcate()
        { }

        [RelayCommand]
        private async Task SaveToDB()
        {
        }

        [RelayCommand]
        private async Task SearchInTrueCaller()
        { }

        [RelayCommand]
        private async Task WriteVCFFile(string filePath)
        {
            //var vcard = new VCard
            //{
            //    Version = VCardVersion.V4,
            //    FormattedName = "John Doe",
            //    FirstName = "John",
            //    LastName = "Doe",
            //    Classification = ClassificationType.Confidential,
            //    Categories = new[] { "Friend", "Fella", "Amsterdam" },

            //};

            var cards = VCards.ToList();

            string serialized = cards.Serialize();
            // string serialized = vcard.Serialize();
            // string path = Path.Combine(@"C:\", "JohnDoe.vcf");
            File.WriteAllText(filePath, serialized);
        }
    }
}