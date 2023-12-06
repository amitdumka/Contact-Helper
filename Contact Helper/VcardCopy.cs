using MixERP.Net.VCards.Models;
using MixERP.Net.VCards.Types;
using Syncfusion.Pdf.Parsing;
using VCard = MixERP.Net.VCards.VCard;

namespace Contact_Helper
{
    internal static class VcardCopy
    {
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

        public static AksContact ToAKSContactClean(this ContactExt ce)
        {
            var x= new AksContact
            {
                Address = ce.Addresse.Replace("#", ",\n"),
                AnniversaryDate = ce.Anniversary,
                Birthdate = ce.BirthDay,
                Email = ce.Email,
                FirstName = ce.FirstName,
                LastName = ce.LastName,
                MiddleName = ce.MiddleName,
                Id = ce.Id,
                IsEmmbeded = ce.IsEmbeddedPhoto,
                Latitude = ce.Latitude,
                Longitude = ce.Longitude,
                NamePrefix = ce.Prefix,
                Organization = ce.Organization,
                OrganizationalUnit = ce.OrganizationalUnit,
                TrueCallerName = ce.TrueCallerName,
                NameSuffix = ce.Suffix,
                Note = ce.Note,
                PhotoContent = ce.ContentsPhoto,
                PhotoExt = ce.ExtensionPhoto,
                Title = ce.Title,
                Phone = ce.Telephone
            };

            var phones = x.Phone.Split(';').ToList();
            var clean= new List<string>();
            foreach (var item in phones)
            {
                clean.Add(CleanPhoneNumber(item));
                
            }
            clean=clean.Distinct().ToList();
            foreach (var item in clean)
            {
                x.Phone += $"{item};";
            }
            x.Note += "#Cleaned";
            return x;
        }

        public static AksContact ToAKSContact(this ContactExt ce)
        {
            return new AksContact
            {
                Address = ce.Addresse.Replace("#", ",\n"),
                AnniversaryDate = ce.Anniversary,
                Birthdate = ce.BirthDay,
                Email = ce.Email,
                FirstName = ce.FirstName,
                LastName = ce.LastName,
                MiddleName = ce.MiddleName,
                Id = ce.Id,
                IsEmmbeded = ce.IsEmbeddedPhoto,
                Latitude = ce.Latitude,
                Longitude = ce.Longitude,
                NamePrefix = ce.Prefix,
                Organization = ce.Organization,
                OrganizationalUnit = ce.OrganizationalUnit,
                TrueCallerName = ce.TrueCallerName,
                NameSuffix = ce.Suffix,
                Note = ce.Note,
                PhotoContent = ce.ContentsPhoto,
                PhotoExt = ce.ExtensionPhoto,
                Title = ce.Title,
                Phone = ce.Telephone
            };
        }

        public static AksContact ToAKSContact(this VCard card)
        {
            AksContact ceExt = new AksContact
            {
                AnniversaryDate = card.Anniversary,
                Birthdate = card.BirthDay,

                FirstName = card.FirstName,
                LastName = card.LastName,

                Title = card.Title,
                NameSuffix = card.Suffix,
                Latitude = card.Latitude,
                Longitude = card.Longitude,

                Note = card.Note,
                MiddleName = card.MiddleName,

                Organization = card.Organization,
                OrganizationalUnit = card.OrganizationalUnit,
                NamePrefix = card.Prefix,

                TrueCallerName = "",
                PhotoContent = card.Photo.Contents,
                PhotoExt = card.Photo.Extension,
                IsEmmbeded = card.Photo.IsEmbedded,

                Id = 0,
            };
            foreach (var item in card.Addresses)
            {
                ceExt.Address += $"{item.Label},\n{item.ExtendedAddress},\n{item.PoBox},\n{item.Street},\n{item.PostalCode},\n{item.Locality},\n{item.Region},\n{item.Country},\n\n" + ";";
            }
            foreach (var item in card.Telephones)
            {
                ceExt.Phone += item + ";";
            }
            foreach (var item in card.Emails)
            {
                ceExt.Email += item + ";";
            }
            return ceExt;
        }

        public static ContactExt ToContactExt(this VCard card)
        {
            ContactExt ceExt = new ContactExt
            {
                Anniversary = card.Anniversary,
                BirthDay = card.BirthDay,
                Categories = card.Categories.ToString(),
                FirstName = card.FirstName,
                LastName = card.LastName,
                FormattedName = card.FormattedName,
                Key = card.Key,
                UniqueIdentifier = card.UniqueIdentifier,
                Kind = card.Kind,
                Title = card.Title,
                Suffix = card.Suffix,
                Latitude = card.Latitude,
                Longitude = card.Longitude,
                Mailer = card.Mailer,
                LastRevision = card.LastRevision,
                Note = card.Note,
                MiddleName = card.MiddleName,
                NickName = card.NickName,
                Organization = card.Organization,
                OrganizationalUnit = card.OrganizationalUnit,
                Prefix = card.Prefix,
                Status = false,
                Role = card.Role,
                Sound = card.Sound,
                TrueCallerName = "",
                SourceUri = card.Source.OriginalString,
                UrlUri = card.Url.OriginalString,
                ContentsLogo = card.Logo.Contents,
                ContentsPhoto = card.Photo.Contents,
                ExtensionLogo = card.Logo.Extension,
                IsEmbeddedLogo = card.Logo.IsEmbedded,
                ExtensionPhoto = card.Photo.Extension,
                IsEmbeddedPhoto = card.Photo.IsEmbedded,
                Id = 0,
                SortString = card.SortString,
                DeliveryAddress = card.DeliveryAddress.Address.Trim(),
            };
            foreach (var item in card.Relations)
            {
                ceExt.Relations += item.RelationUri.OriginalString + "#" + item.Type.ToString() + "#" + item.Preference.ToString() + ";";
            }
            foreach (var item in card.Addresses)
            {
                ceExt.Addresse += $"{item.Label}#{item.ExtendedAddress}#{item.PoBox}#{item.Street}#{item.PostalCode}#{item.Locality}#{item.Region}#{item.Country}" + ";";
            }
            foreach (var item in card.Telephones)
            {
                ceExt.Telephone += item + ";";
            }
            foreach (var item in card.Emails)
            {
                ceExt.Email += item + ";";
            }
            return ceExt;
        }

        public static VCard ToVCard(this ContactExt ce)
        {
            VCard card = new VCard
            {
                Anniversary = ce.Anniversary,
                BirthDay = ce.BirthDay,
                FirstName = ce.FirstName,
                LastName = ce.LastName,
                MiddleName = ce.MiddleName,
                FormattedName = ce.FormattedName,
                DeliveryAddress = new MixERP.Net.VCards.Types.DeliveryAddress { Address = ce.DeliveryAddress },
                Key = ce.Key,
                Kind = ce.Kind,
                LastRevision = ce.LastRevision,
                Logo = new MixERP.Net.VCards.Models.Photo(ce.IsEmbeddedLogo, ce.ExtensionLogo, ce.ContentsLogo),
                Photo = new MixERP.Net.VCards.Models.Photo(ce.IsEmbeddedPhoto, ce.ExtensionPhoto, ce.ContentsPhoto),
                Latitude = ce.Latitude,
                SortString = ce.SortString,
                Role = ce.Role,
                OrganizationalUnit = ce.OrganizationalUnit,
                Longitude = ce.Longitude,
                Sound = ce.Sound,
                Organization = ce.Organization,
                Note = ce.Note,
                NickName = ce.NickName,
                Version = MixERP.Net.VCards.Types.VCardVersion.V2_1,
                Title = ce.Title,
                Suffix = ce.Suffix,
                Mailer = ce.Mailer,
                UniqueIdentifier = ce.UniqueIdentifier,
                Prefix = ce.Prefix,
            };

            var tels = ce.Telephone.Split(';');
            var rels = ce.Relations.Split(";");
            var adds = ce.Addresse.Split(";");
            var emails = ce.Email.Split(";");

            var Telephones = new List<Telephone>();
            var Addresses = new List<Address>();
            var Emails = new List<MixERP.Net.VCards.Models.Email>();
            var Relations = new List<Relation>();

            foreach (var item in tels)
            {
                Telephones.Add(new Telephone { Number = item, Type = TelephoneType.Cell });
            }
            foreach (var item in emails)
            {
                Emails.Add(new MixERP.Net.VCards.Models.Email { EmailAddress = item, Type = EmailType.Smtp });
            }
            foreach (var item in rels)
            {
                var rel = item.Split("#");
                Relations.Add(new Relation { RelationUri = new Uri(rel[0]), Preference = int.Parse(rel[2].Trim()), Type = (RelationType)Enum.Parse(typeof(RelationType), rel[1]) });
            }
            foreach (var item in adds)
            {
                var add = item.Split("#");
                Addresses.Add(new Address { Label = add[0], ExtendedAddress = add[1], PoBox = add[2], Street = add[3], PostalCode = add[4], Locality = add[5], Region = add[6], Country = add[7] });
            }
            card.Telephones = Telephones;
            card.Emails = Emails; card.Relations = Relations; card.Addresses = Addresses;

            return card;
        }
    }
}