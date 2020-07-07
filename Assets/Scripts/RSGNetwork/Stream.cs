//Author: Jake Aquilina
//Creation Date: 27/06/20 11:00 PM
//Company: RealSoft Games
//Website: https://www.realsoftgames.com/

using System.Collections.Generic;

namespace RealSoftGames.Network
{
    [System.Serializable]
    public class Stream
    {
        public Stream(Stream stream)
        {
            foreach (var item in stream.data)
            {
                data.Push(item);
            }
        }

        private Stack<object> data;
        private bool isWriting;

        public bool IsWriting { get => isWriting; }

        public T RecieveNext<T>()
        {
            return (T)data.Pop();
        }

        public void SendNext(object obj)
        {
            data.Push(obj);
        }
    }
}