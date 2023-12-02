﻿using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MixERP.Net.VCards;
using MixERP.Net.VCards.Serializer;
using System.Collections.ObjectModel;

namespace Contact_Helper
{

    class AutoMapperHelper
    {
        public IMapper Mapper { get; set; }
        public MapperConfiguration Configuration { get; set; }

        public AutoMapperHelper()
        {
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<VCard, ContactExt>();
                cfg.CreateMap<ContactExt, VCard>();
            });
            // only during development, validate your mappings; remove it before release
#if DEBUG
            configuration.AssertConfigurationIsValid();
#endif
            this.Configuration = configuration; 

            // use DI (http://docs.automapper.org/en/latest/Dependency-injection.html) or create the mapper yourself
            var mapper = configuration.CreateMapper();
            this.Mapper = mapper;
        }
    }



    internal partial class VcfHelper : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<VCard> _vCards;

        [RelayCommand]
        private async Task ReadVCFFile(string filePath)
        {
            IEnumerable<VCard> vcards = (IEnumerable<VCard>)MixERP.Net.VCards.Deserializer.Deserialize(filePath);
            // string contents = File.ReadAllText(filePath, Encoding.UTF8);
            // vcards = (IEnumerable<VCard>)MixERP.Net.VCards.Deserializer.GetVCards(contents);

            foreach (var vcard in vcards)
            {
                VCards.Add(vcard);
            }
            Notify.NotifyShort($"Read {VCards.Count}  contacts");
        }

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

        [RelayCommand]
        private async Task DeleteVCFFile(string filePath)
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
        private async Task SaveToDB()
        {
        }

        [RelayCommand]
        private async Task CleanContact()
        { }

        [RelayCommand]
        private async Task DBToVCard()
        { }

        [RelayCommand]
        private async Task FindDuplicate()
        {
        }

        [RelayCommand]
        private async Task RemoveDupilcate()
        { }

        [RelayCommand]
        private async Task SearchInTrueCaller()
        { }
    }
}