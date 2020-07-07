//Author: Jake Aquilina
//Creation Date: 15/06/20 12:32 AM
//Company: RealSoft Games
//Website: https://www.realsoftgames.com/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RealSoftGames
{
    public static class StringExtensions
    {
        public static bool IsNullorWhitespace(this string text)
        {
            for (int i = 0; i < text.Length; i++)
            {
                if (!char.IsWhiteSpace(text[i]))
                {
                    return false;
                }
            }

            return true;
        }
    }
}