﻿namespace Exchange.RestServices
{
    using System;
    using Microsoft.OutlookServices;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json.Serialization;

    /// <summary>
    /// Custom deserializer for handling Json.
    /// </summary>
    internal class Deserializer
    {
        /// <summary>
        /// Singleton instance.
        /// </summary>
        private static readonly Deserializer instance = new Deserializer();

        /// <summary>
        /// Create new instance of <see cref="Deserializer"/>.
        /// </summary>
        private Deserializer()
        {
        }

        /// <summary>
        /// Singleton instance.
        /// </summary>
        internal static Deserializer Instance
        {
            get { return Deserializer.instance; }
        }

        /// <summary>
        /// Deserialize http web entityResponse. Doesn't perform any validation.
        /// </summary>
        /// <typeparam name="T">Type.</typeparam>
        /// <param name="httpWebResponse">Http web entityResponse.</param>
        /// <returns></returns>
        public T Deserialize<T>(IHttpWebResponse httpWebResponse, Type type = null)
        {
            return this.Deserialize<T>(httpWebResponse.Content, type);
        }

        /// <summary>
        /// Deserialize Json string.
        /// </summary>
        /// <typeparam name="T">Type.</typeparam>
        /// <param name="content">String content.</param>
        /// <returns></returns>
        public T Deserialize<T>(string content, Type type)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
                TypeNameHandling = TypeNameHandling.All,
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
            };

            settings.Converters.Add(new AttachmentConverter());
            if (null != type)
            {
                settings.Converters.Add(new ItemConverter(type));
            }

            return JsonConvert.DeserializeObject<T>(content, settings);
            
        }

        /// <summary>
        /// Attachment class resolver. Just making sure stack overflow is not encountered. 
        /// </summary>
        private class AttachmentClassResolver : DefaultContractResolver
        {
            /// <summary>
            /// Resolve contract converter. 
            /// </summary>
            /// <param name="objectType"></param>
            /// <returns></returns>
            protected override JsonConverter ResolveContractConverter(Type objectType)
            {
                if (typeof(Attachment).IsAssignableFrom(objectType) && !objectType.IsAbstract)
                {
                    return null;
                }

                return base.ResolveContractConverter(objectType);
            }
        }

        /// <summary>
        /// Outlook item class resolver
        /// </summary>
        private class ItemClassResolver : DefaultContractResolver
        {
            /// <summary>
            /// Resolve contract converter. 
            /// </summary>
            /// <param name="objectType"></param>
            /// <returns></returns>
            protected override JsonConverter ResolveContractConverter(Type objectType)
            {
                if (typeof(Item).IsAssignableFrom(objectType) && !objectType.IsAbstract)
                {
                    return null;
                }

                return base.ResolveContractConverter(objectType);
            }
        }

        /// <summary>
        /// Correctly deserialize specific type of the attachment since
        /// items are using abstract reference property.
        /// </summary>
        private class AttachmentConverter : JsonConverter
        {
            /// <summary>
            /// Conversion settings.
            /// </summary>
            private JsonSerializerSettings subClassConversionSettings = new JsonSerializerSettings()
            {
                ContractResolver = new AttachmentClassResolver(),
                NullValueHandling = NullValueHandling.Ignore
            };

            /// <summary>
            /// Write Json - Not implemented, however it won't throw as <see cref="CanWrite"/> returns false.
            /// </summary>
            /// <param name="writer">Writer.</param>
            /// <param name="value">Value.</param>
            /// <param name="serializer">Serializer.</param>
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// Reads Json and correctly binds Attachment type.
            /// </summary>
            /// <param name="reader">Reader.</param>
            /// <param name="objectType">Object type.</param>
            /// <param name="existingValue">Existing value.</param>
            /// <param name="serializer">Serializer.</param>
            /// <returns></returns>
            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                JToken jsonToken = JObject.ReadFrom(reader);
                string currentAttachmentType = jsonToken["@odata.type"].ToString();
                if (currentAttachmentType.Equals(FileAttachmentObjectSchema.ODataType.DefaultValue))
                {
                    return JsonConvert.DeserializeObject<FileAttachment>(jsonToken.ToString(), subClassConversionSettings);
                }

                if (currentAttachmentType.Equals(ItemAttachmentObjectSchema.ODataType.DefaultValue))
                {
                    // correct item attachment converter needs to be added.
                    JToken item = jsonToken["item"];
                    if (null != item)
                    {
                        string itemType = item["@odata.type"].ToString();
                        if (string.IsNullOrEmpty(itemType))
                        {
                            this.subClassConversionSettings.Converters.Add(
                                new ItemConverter(typeof(Message)));
                        }
                        else
                        {
                            if (itemType.Equals(MessageObjectSchema.ODataType.DefaultValue))
                            {
                                this.subClassConversionSettings.Converters.Add(
                                    new ItemConverter(typeof(Message)));
                            }

                            if (itemType.Equals(EventObjectSchema.ODataType.DefaultValue))
                            {
                                this.subClassConversionSettings.Converters.Add(
                                    new ItemConverter(typeof(Event)));
                            }
                        }
                    }
                    else
                    {
                        this.subClassConversionSettings.Converters.Add(
                            new ItemConverter(typeof(Message)));
                    }

                    return JsonConvert.DeserializeObject<ItemAttachment>(jsonToken.ToString(), subClassConversionSettings);
                }

                if (currentAttachmentType.Equals(ReferenceAttachmentObjectSchema.ODataType.DefaultValue))
                {
                    return JsonConvert.DeserializeObject<ReferenceAttachment>(jsonToken.ToString(), subClassConversionSettings);
                }

                throw new NotImplementedException(currentAttachmentType);
            }

            /// <summary>
            /// If object is typeof <see cref="Attachment"/> returns true.
            /// </summary>
            /// <param name="objectType"></param>
            /// <returns></returns>
            public override bool CanConvert(Type objectType)
            {
                return (objectType == typeof(Attachment));
            }

            /// <summary>
            /// CanWrite - false.
            /// </summary>
            public override bool CanWrite
            {
                get { return false; }
            }
        }

        /// <summary>
        /// Correctly deserialize specific type of the attachment since
        /// items are using abstract reference property.
        /// </summary>
        private class ItemConverter : JsonConverter
        {
            /// <summary>
            /// Type to convert.
            /// </summary>
            private Type type;

            /// <summary>
            /// Outlook item converter.
            /// </summary>
            /// <param name="type"></param>
            internal ItemConverter(Type type)
            {
                this.type = type;
                this.subClassConversionSettings.Converters.Add(new AttachmentConverter());
            }

            /// <summary>
            /// Conversion settings.
            /// </summary>
            private JsonSerializerSettings subClassConversionSettings = new JsonSerializerSettings()
            {
                ContractResolver = new ItemClassResolver(),
                NullValueHandling = NullValueHandling.Ignore,
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
            };

            /// <summary>
            /// Write Json - Not implemented, however it won't throw as <see cref="CanWrite"/> returns false.
            /// </summary>
            /// <param name="writer">Writer.</param>
            /// <param name="value">Value.</param>
            /// <param name="serializer">Serializer.</param>
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// Reads Json and correctly binds Outlook item type.
            /// </summary>
            /// <param name="reader">Reader.</param>
            /// <param name="objectType">Object type.</param>
            /// <param name="existingValue">Existing value.</param>
            /// <param name="serializer">Serializer.</param>
            /// <returns></returns>
            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                JToken jsonToken = JObject.ReadFrom(reader);

                if (this.type == typeof(Message))
                {
                    return JsonConvert.DeserializeObject<Message>(
                        jsonToken.ToString(), 
                        subClassConversionSettings);
                }

                if (this.type == typeof(Event))
                {
                    return JsonConvert.DeserializeObject<Event>(
                        jsonToken.ToString(), 
                        subClassConversionSettings);
                }

                if (this.type == typeof(Task))
                {
                    return JsonConvert.DeserializeObject<Task>(
                        jsonToken.ToString(), 
                        subClassConversionSettings);
                }

                if (this.type == typeof(Contact))
                {
                    return JsonConvert.DeserializeObject<Contact>(
                        jsonToken.ToString(), 
                        subClassConversionSettings);
                }

                throw new NotImplementedException($"Type not implemented: {this.type.FullName}");
            }

            /// <summary>
            /// If object is typeof <see cref="Attachment"/> returns true.
            /// </summary>
            /// <param name="objectType"></param>
            /// <returns></returns>
            public override bool CanConvert(Type objectType)
            {
                return (objectType == typeof(Item));
            }

            /// <summary>
            /// CanWrite - false.
            /// </summary>
            public override bool CanWrite
            {
                get { return false; }
            }
        }
    }
}