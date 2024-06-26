﻿using HotelManager;
using Sigma.Core.DataStorage;
using System.Text.Json.Serialization;
using System.Text.Json;
using Sigma.Core.RemoteHotelEntry;
using System.Globalization;
using System.Transactions;

namespace Sigma.Core.RemoteHotelEntry
{
    public class DocumentsSet : HashSet<DocumentEntity>{}

    [JsonConverter(typeof(SystemTextJsonPolymorphism.DocumentConverter))]
    public class DocumentEntity: Entity
    {
        public static string DocumentTypeToString(DocumentType documentType)
        {
            switch(documentType)
            {
                case DocumentType.Sell:
                    return "Реализация";
                case DocumentType.Buy:
                    return "Покупка";
                case DocumentType.ReturnFromClient:
                    return "Возврат";
                case DocumentType.ReturnToClient:
                    return "Покупка товаров и услуг";
                case DocumentType.MoneyGet:
                    return "Прих. ордер";
                case DocumentType.MoneyPut:
                    return "Расх. ордер";
                case DocumentType.MoneyReturnToClient:
                    return "Возв. денег";
                case DocumentType.MoneyReturnFromClient:
                    return "Возв. денег от продавца";
                case DocumentType.MoneyTransfer:
                    return "Сдача кассы";
                case DocumentType.Unknown:
                    return "Handled Unknown";
            }

            return "Unknown";
        }

        public void Fill(OrganizationEntity organization, DateTime? date, float money, string? comment, UserEntity? user, bool isActive, bool isDeleted)
        {
            Organization = organization;
            Date = date;
            Money = money;
            Comment = comment;
            User = user;
            IsActive = isActive;
            IsDeleted = isDeleted;
            Name = generateName();
        }

        private string generateName()
        { 
            return DocumentTypeToString(Type) + " " + stripTime;
        }

        public void SetParentDocumentID(string? parentDocumentID)
        {
            if (parentDocumentID != ParentID)
            {
                ParentID = parentDocumentID;
            }
        }

        public void SetChildDocuments(DocumentsDictionary newChildDocuments)
        {
            if (!newChildDocuments.Equals(Children))
            {
                foreach(KeyValuePair<string, DocumentEntity> oldChildPair in Children)
                {
                    if (!newChildDocuments.ContainsKey(oldChildPair.Key))
                    {
                        if (oldChildPair.Value.ParentID == this.Id)
                        {
                            oldChildPair.Value.SetParentDocumentID(null);
                        }
                    }
                }

                foreach (KeyValuePair<string, DocumentEntity> newChildPair in newChildDocuments)
                {
                    if (newChildPair.Value.ParentID != this.Id)
                    {
                        newChildPair.Value.SetParentDocumentID(this.Id);
                    }
                }

                Children = newChildDocuments;
            }
        }

        public virtual bool filterMatch(EntityFilterDocument filter, out bool completeMismatch)
        {
            completeMismatch = true;
            if (filter.DocumentType != null)
            {
                if (this.Type != filter.DocumentType)
                {
                    return false;
                }
            }

            if (filter.BookingIDs.Count > 0)
            {
                if (!ProductDocumentEntity.IsClientDocument(Type))
                {
                    return false;
                }

                ClientDocumentEntity clientDocument = (ClientDocumentEntity)this;
                if (clientDocument.EasyMSBookingId == null)
                {
                    return false;
                }

                bool accept = false;

                foreach (string bookingId in filter.BookingIDs)
                {
                    if (!clientDocument.EasyMSBookingId.Contains(bookingId))
                    {
                        accept = true;
                        break;
                    }
                }

                if (!accept)
                {
                    return false;
                }
            }

            if (filter.CreatorID != null)
            {
                if (this.User?.Id != filter.CreatorID)
                {
                    return false;
                }
            }

            if (filter.StartDate != null)
            {
                if (this.Date == null || this.Date < filter.StartDate)
                {
                    return false;
                }
            }

            if (filter.EndDate != null)
            {
                if (this.Date > filter.EndDate)
                {
                    return false;
                }
            }

            if (base.filterMatch(filter, out completeMismatch))
            {
                return true;
            }
            else if (completeMismatch)
            {
                return false;
            }

            if (filter.StringFilter != null && filter.StringFilter.Trim().Length > 0)
            {
                if (filter.CreatorID == null && this.User != null)
                {
                    if (filter.Culture.CompareInfo.IndexOf(this.User.Name, filter.StringFilter, CompareOptions.IgnoreCase) >= 0)
                    {
                        return true;
                    }
                }

                if (this.Date != null)
                {
                    if (this.Date.ToString()!.Contains(filter.StringFilter, StringComparison.CurrentCulture))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public DocumentEntity(string id, OrganizationEntity organization, DateTime? date, float money, string? comment, UserEntity? user, DocumentType documentType, bool isActive, bool isDeleted) : base(id, "", isDeleted)
        {
            Children = new DocumentsDictionary();
            Type = documentType;

            Fill(organization, date, money, comment, user, isActive, isDeleted);
        }

        private string stripId 
        {
            get
            {
                int indexOfDots = Id.IndexOf(':');
                string result = Id.Substring(0, indexOfDots > 0 ? indexOfDots : Id.Length);
                int trimEnd = 0;
                while (result[trimEnd] == '0')
                {
                    ++trimEnd;
                }

                return trimEnd > 0 ? result.Substring(trimEnd) : result;
            }
        }

        private string stripTime
        {
            get
            {
                string dateString = Date != null ? (Date!.ToString() ?? "") : "";
                int indexOfDots = dateString.LastIndexOf(':');

                return dateString.Substring(0, indexOfDots > 0 ? indexOfDots : dateString.Length);
            }
        }
        public float Money { get; set; }

        public OrganizationEntity Organization { get; set; }
        
        public DateTime? Date { get; set; }

        public string? Comment { get; set; }

        public UserEntity? User { get; set; }

        public DocumentType Type { get; set; }

        public bool IsActive { get; set; }

        public DocumentsDictionary Children { get; set; }

        public String? ParentID { get; set; }
    }
}

namespace SystemTextJsonPolymorphism
{
    internal class DocumentConverter: JsonConverter<DocumentEntity>
    {
        public override bool CanConvert(Type typeToConvert) => typeof(DocumentEntity).IsAssignableFrom(typeToConvert);

        public override DocumentEntity? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            using (var jsonDocument = JsonDocument.ParseValue(ref reader))
            {
                if (!jsonDocument.RootElement.TryGetProperty("type", out var typeProperty))
                {
                    throw new JsonException();
                }

                var jsonDoc = jsonDocument.RootElement.GetRawText();

                HotelManager.DocumentType documentType = (HotelManager.DocumentType)typeProperty.GetUInt32();

                if (ProductDocumentEntity.IsProductDocument(documentType))
                {
                    return (ProductDocumentEntity?)JsonSerializer.Deserialize(jsonDoc, typeof(ProductDocumentEntity));
                }
                else if (ProductDocumentEntity.IsMoneyStoreDocument(documentType))
                {
                    return (MoneyStoreDocumentEntity?)JsonSerializer.Deserialize(jsonDoc, typeof(MoneyStoreDocumentEntity));
                }
                else if (documentType == DocumentType.MoneyTransfer)
                {
                    return (MoneyTransferDocumentEntity?)JsonSerializer.Deserialize(jsonDoc, typeof(MoneyTransferDocumentEntity));
                }
            }

            return null;
        }
        public override void Write(
        Utf8JsonWriter writer, DocumentEntity documentEntity, JsonSerializerOptions options)
        {

            JsonSerializerOptions newOptions  = new JsonSerializerOptions(options);
            newOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                //JsonSerializerDefaults.Web
            if (documentEntity is ProductDocumentEntity productDocument)
            {
                JsonSerializer.Serialize(writer, productDocument, newOptions);
            }
            else if (documentEntity is MoneyStoreDocumentEntity moneyDocument)
            {
                JsonSerializer.Serialize(writer, moneyDocument, newOptions);
            }
            else if (documentEntity is MoneyTransferDocumentEntity moneyTransfer)
            {
                JsonSerializer.Serialize(writer, moneyTransfer, newOptions);
            }
        }
    }
}