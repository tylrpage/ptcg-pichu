using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using tylrpage.Pichu.PTCGEngine.Core.Effects;
using tylrpage.Pichu.PTCGEngine.Core.Resolution;
using tylrpage.Pichu.PTCGEngine.Core.Value;

namespace tylrpage.Pichu.PTCGEngine.Core.Serialization
{
    public class EngineTypeBinder : ISerializationBinder
    {
        private readonly Dictionary<string, Type> _nameToType;

        public EngineTypeBinder()
        {
            List<Type> serializableTypes = new()
            {
                typeof(GameStep),
                typeof(CardEffect),
                typeof(NumberValue),
            };
            
            // Every concrete class from serializableTypes, keyed by class name
            _nameToType = serializableTypes.SelectMany(x => x.Assembly.GetTypes()
                .Where(y => x.IsAssignableFrom(y) && !y.IsAbstract))
                .ToDictionary(GetTypeName);
        }
        
        public Type BindToType(string assemblyName, string typeName)
        {
            if (!_nameToType.TryGetValue(typeName, out Type type))
            {
                throw new JsonSerializationException($"Type not allowed: {typeName}");
            }

            return type;
        }

        public void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            assemblyName = null;
            typeName = GetTypeName(serializedType);
        }

        private string GetTypeName(Type type)
        {
            return type.Name;
        }
    }
}