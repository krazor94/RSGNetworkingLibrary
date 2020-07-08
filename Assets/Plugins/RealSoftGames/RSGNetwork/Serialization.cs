//Author: Jake Aquilina
//Creation Date: 15/06/20 12:31 AM
//Company: RealSoft Games
//Website: https://www.realsoftgames.com/

using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace RealSoftGames.Network
{
    public static class Serialization
    {
        public static T Deserialize<T>(this byte[] bytes)
        {
            //return JsonConvert.DeserializeObject<T>(Encoding.Default.GetString(bytes));
            BinaryFormatter bf = new BinaryFormatter();
            System.IO.Stream stream = new MemoryStream(bytes);
            object obj = (object)bf.Deserialize(stream);

            return (T)Convert.ChangeType(obj, obj.GetType());
        }

        public static byte[] Serialize<T>(this T[] objects)
        {
            //return Encoding.Default.GetBytes(JsonConvert.SerializeObject(objects));
            return objects.ToBytes();
        }

        public static byte[] Serialize<T>(this T obj)
        {
            //return Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(obj));
            return obj.ToBytes();
        }

        public static byte[] ToBytes<T>(this T[] objects)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, objects);
                return ms.ToArray();
            }
        }

        public static byte[] ToBytes<T>(this T obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }
    }
}