using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoteInputLib;
public class ParseInputNew
{
    public static readonly string[] LineDelimiters = new string[] { "\r\n", "\n", ";" };
    public const string Comment = @"//";

}
public enum ShowNoteAs
{
    /// <summary>
    /// Note with symbol
    /// </summary>
    Symbol,
    /// <summary>
    /// 0, 1, 2, .., 9, 10, 11
    /// </summary>
    Number,
    /// <summary>
    /// 0, 1, 2, .., 9, A, B
    /// </summary>
    NumberBase12,
    /// <summary>
    /// B♭Letter notation
    /// </summary>
    LetterNotation,
    /// <summary>
    /// B4♭ Scientific pitch notation
    /// </summary>
    NoteOctave,
}
