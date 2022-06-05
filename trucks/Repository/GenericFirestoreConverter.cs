using System.Reflection;
using Google.Cloud.Firestore;
using System.Collections;

namespace Trucks
{
    /// <summary>
    /// Implements a generic custom converter to handle default POCO serialization
    /// without explicit attribute decoration.
    /// <see ref="https://cloud.google.com/dotnet/docs/reference/Google.Cloud.Firestore/latest/datamodel#custom-converters">Custom Converters</see>
    /// </summary>
    public class GenericFirestoreConverter<T> 
            : IFirestoreConverter<T> where T : class, new()
    {
        private readonly PropertyInfo[] _properties;
        private readonly string _idProperty;
        // Expected attribute name for the required unique identifier.
        private const string FirestoreId = "id";

        /// <summary>
        /// Create a converter by specifing the unique identifier property name
        /// of the type to be converted.
        /// </summary>
        public GenericFirestoreConverter(string idProperty) : this()
        {                       
            if(GetProperty(idProperty) == null)
                throw new ArgumentException(
                    $"idProperty must be the name of a public property of type {typeof(T)}");
            
            _idProperty = idProperty;
        }

        public GenericFirestoreConverter()
        {
            _properties = GetProperties();
        }

        public object ToFirestore(T value) 
        {
            var map = new Dictionary<string, object>();
            foreach(var p in _properties)
            {
                // Firestore expects an id property as unique identifier.
                if (p.Name == _idProperty)
                {
                    map.Add(FirestoreId, p.GetValue(value));
                }

                // Deal with instances where DateTime is empty.
                if (p.PropertyType == typeof(DateTime))
                {
                    var dateValue = (DateTime)p.GetValue(value);
                    map.Add(p.Name, 
                        DateTime.SpecifyKind(dateValue, DateTimeKind.Utc)
                    );
                }
                else
                {
                    map.Add(p.Name, p.GetValue(value));
                }
            }
            return map;
        } 

        public T FromFirestore(object value)
        {
            T item = new T();

            if (value is IDictionary<string, object> map)
            {                
                foreach(var pair in map)
                {
                    PropertyInfo property = null;

                    if (pair.Key == FirestoreId)
                        property = GetProperty(_idProperty);
                    else
                        property = GetProperty(pair.Key);
                    
                    if (property != null)
                        TrySetValue(item, property, pair.Value);
                }
            }

            return item;
        }

        private PropertyInfo[] GetProperties()
        {
            var properties = typeof(T).GetProperties(BindingFlags.Public|BindingFlags.Instance);
            // Only use properties that can be written to and read.
            return properties.Where(p => p.CanRead == true && p.CanWrite == true)
                .ToArray();
        }

        private PropertyInfo GetProperty(string name)
        {  
            if (_properties?.Count() > 0)
            {
                return _properties.Where(p => p.Name == name).FirstOrDefault();
            }
            else
            {
                return null;
            }
        }

        private void TrySetValue(T item, PropertyInfo property, object value)
        {
            try
            {
                if (property.PropertyType == typeof(DateTime))
                {
                    Timestamp obj = (Timestamp)value;
                    property.SetValue(item, obj.ToDateTime());
                }
                else if (property.PropertyType == typeof(Int32))
                {
                    property.SetValue(item, (Int32)(Int64)value);
                }
               else if (value is IEnumerable<object> && property.PropertyType.IsGenericType)
               {
                   IList list = CreateGenericList(property, value);
                   property.SetValue(item, list);
               }                
                else
                {
                    property.SetValue(item, value);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error in TrySetValue {property.Name}: {e.Message}");
            }
        }

        private IList CreateGenericList(PropertyInfo property, object value)
        {
            var values = (IEnumerable<object>)value;

            try 
            {
                var converter = FirestoreListDeserializer.GetConverter(property);
                var list = converter.CreateList();

                foreach (var item in values) 
                {
                    var result = converter.Convert(item);
                    list.Add(result);
                }

                return list;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error serializaing {property.Name}: {e.Message}");
                return null;
            }          
        }
    }
}