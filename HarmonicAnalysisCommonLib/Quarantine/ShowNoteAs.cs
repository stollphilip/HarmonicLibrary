using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HarmonicAnalysisCommonLib.Quarantine;
public enum ShowNoteAs
{
    /// <summary>
    /// Note with symbol
    /// </summary>
    Symbol,
    /// <summary>
    /// 0, 1, 2, .., 9, 10, 11
    /// </summary>
    NumberBase10,
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
    Scientific,
}

