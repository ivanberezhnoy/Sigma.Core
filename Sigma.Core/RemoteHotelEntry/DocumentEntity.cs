﻿using HotelManager;
using Sigma.Core.DataStorage;
using System.Text.Json.Serialization;
using System.Text.Json;
using Sigma.Core.RemoteHotelEntry;

namespace Sigma.Core.RemoteHotelEntry
{
    public class DocumentsSet : HashSet<DocumentEntity>{}

    [JsonConverter(typeof(SystemTextJsonPolymorphism.DocumentConverter))]
    public class DocumentEntity
    {
        public static string DocumentTypeToString(DocumentType documentType)
        {
            switch(documentType)
            {
                case DocumentType.Sell:
                    return "Реализация товаров и услуг";
                case DocumentType.Buy:
                    return "Поступление товаров и услуг";
                case DocumentType.ReturnFromClient:
                    return "Возврат товаров от покупателя";
                case DocumentType.ReturnToClient:
                    return "Поступление товаров и услуг";
                case DocumentType.MoneyGet:
                    return "Приходный кассовый ордер";
                case DocumentType.MoneyPut:
                    return "Расходный кассовый ордер";
                case DocumentType.MoneyReturnToClient:
                    return "Возврат денег покупателю";
                case DocumentType.MoneyReturnFromClient:
                    return "Возврат денег от продавца";
                case DocumentType.MoneyTransfer:
                    return "Сдача кассы";
            }

            return "Unknown";
        }

        public void Fill(OrganizationEntity organization, DateTime? date, string? comment, UserEntity? user, bool isActive)
        {
            Organization = organization;
            Date = date;
            Comment = comment;
            User = user;
            IsActive = isActive;
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

        public DocumentEntity(string id, OrganizationEntity organization, DateTime? date, string? comment, UserEntity? user, DocumentType documentType, bool isActive)
        {
            Id = id;
            Children = new DocumentsDictionary();
            Type = documentType;

            Organization = organization;
            Date = date;
            Comment = comment;
            User = user;
            IsActive = isActive;

            //Fill(organization, client, date, comment, user, isActive);
        }

        public string Id { get; set; }

        private string stripId 
        {
            get
            {
                int indexOfDots = Id.IndexOf(':');

                return Id.Substring(0, indexOfDots > 0 ? indexOfDots - 1 : Id.Length - 1);
            }
        }

        private string stripTime
        {
            get
            {
                string dateString = Date != null ? (Date!.ToString() ?? "") : "";
                int indexOfDots = dateString.LastIndexOf(':');

                return Id.Substring(0, indexOfDots > 0 ? indexOfDots - 1 : dateString.Length - 1);
            }
        }


        public string Name 
        {
            get
            { 
                return DocumentTypeToString(Type) + " " + stripId + " " + Date.ToString();
            }
        }
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