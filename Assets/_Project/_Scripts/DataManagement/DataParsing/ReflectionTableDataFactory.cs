using Shield_Shot.DataManagement.InventorySystem;
using System;
using System.IO;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;


namespace Shield_Shot.DataManagement.DataParsing
{
    public class ReflectionTableDataFactory<T> : ITableDataFactory<T> where T : ScriptableObject
    {

        // relection : CSV파일의 문자열을 실제 필드 변수로 연결
        public T Create(string[] header, string[] row)
        {
            T data = ScriptableObject.CreateInstance<T>();

            for (int i=0; i < header.Length; i++)
            {
                if (i>= row.Length)

                {
                    break;
                }
                string fieldName = header[i].Trim();
                string rawValue = row[i].Trim();

                //대소문자등 모두 찾기
                FieldInfo field = typeof(T).GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);

                if (field ==null)
                {
                    Debug.Log($"[{typeof(TagHandle).Name}] 필드를 찾을 수 없음 : {fieldName}");
                    continue;
                }

                object convertedValue = ConvertValue(field.FieldType, rawValue);
                field.SetValue(data, convertedValue);
            }
            return data;
        }

        //string 문자열을 실제 각 타입으로 변환
        private object ConvertValue(Type type, string value)
        {
            if(string.IsNullOrEmpty(value))
            {
                if (type.IsValueType)
                    return Activator.CreateInstance(type);

                return null;
            }

            if (type == typeof(string))
                return value;

            if (type == typeof(int))
                return int.Parse(value);

            if (type == typeof(float))
                return float.Parse(value);

            if (type == typeof(bool))
                return bool.Parse(value);

            if (type.IsEnum)
                return Enum.Parse(type, value);

            if (type == typeof(Sprite))
                return Resources.Load<Sprite>(value);

            if (type == typeof(GameObject))
                return Resources.Load<GameObject>(value);

            if (typeof(UnityEngine.Object).IsAssignableFrom(type))
                return Resources.Load(value, type);

            Debug.LogWarning($"지원하지 않는 타입: {type.Name}");
            return null;
        }
    
    }
}