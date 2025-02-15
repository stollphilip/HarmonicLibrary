using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using HarmonicAnalysisCommonLib;
using HarmonicAnalysisCommonLib.Quarantine;
using ScoreLibrary.Parameter;
using TextMetricLib;

namespace ScoreLibrary.Base
{
    /// <summary>
    /// represents the progression on a Staff
    /// </summary>
    /// <remarks>responsible for adding Chords, updating layout and location of Chords</remarks>
    public class StaffProgressionStructBase
    {
        protected ProgressionParam staffParameter;
        /// <summary>
        /// List of Chords in the Staff
        /// </summary>
        /// <remarks> A ChordStructBase corresponds to a column of notes, and may not correspond to a chord per se.</remarks>
        public readonly List<ChordStructBase> ChordStructs = new List<ChordStructBase>();
        /// <summary>
        /// Location of the Staff on the Grid with respect to its parent Progression
        /// </summary>
        public LocationStruct Location { get; set; }
        protected double fontScaleFactor = Zoom.fontScaleFactor;
        protected double margin = Zoom.margin;

        public virtual StaffTypeEnum StaffType => StaffTypeEnum.None;
        protected readonly List<(string text, string accidental, int row)> AccidentalToAlter = new List<(string text, string accidental, int row)> {
            ("FLAT", "♭", -1),
            ("NATURAL", "♮", 0),
            ("SHARP", "♯",1),
        };

        public StaffProgressionStructBase(ProgressionParam staffParameter)
        {
            this.staffParameter = staffParameter;
        }
        public StaffProgressionStructBase()
        {

        }

        #region public interface
        /// <summary>
        ///  add Chord
        /// </summary>
        public void Add(ChordStructBase chordStruct)
        {
            Debug.Assert(!ChordStructs.Contains(chordStruct));
            ChordStructs.Add(chordStruct);
        }
        /// <summary>
        /// Return next available ElementStruct location after performing collision avoidance
        /// </summary>
        /// <remarks>Progression would have to be involved in determining the next available location</remarks>
        public virtual LocationStruct NextAvailableLocation(LocationStruct location)
        {
            return location;
        }
        public virtual void SetGrid(int row, int column, int rowSpan, int columnSpan)
        {
            foreach (var chord in ChordStructs)
            {
                chord.SetGrid(Location.Row + row, Location.Column + column, Location.RowSpan, Location.ColumnSpan);
            }
        }
        public int ChordCount()
        {
            return ChordStructs.Count;
        }

        #endregion

        public IEnumerable<ChordStructBase> ChordStructIterator()
        {
            foreach (var chord in ChordStructs)
            {
                yield return chord;
            }
        }

        /// <summary>
        /// Add Chord and add notes to the Chord
        /// </summary>
        /// <param name="chord"></param>
        public virtual void AddChord(ColumnData columnData)
        {

        }
        public virtual bool ShowStaff => true;
        
        public LocationStruct GetChordLocation(int indexChord)
        {
            return new LocationStruct(ChordStructs[indexChord].Location);
        }
        public void SetChordLocation(int indexChord, LocationStruct location)
        {
             ChordStructs[indexChord].Location = location;
        }
        public virtual void AddStaffLines()
        {

        }
        public TextMetric GetTextMetric()
        {
            var textMetric = staffParameter.ShowNoteAs == ShowNoteAs.Symbol
                ? TextMetric.Data.SingleOrDefault(x => x.Text == "𝅗")
                : TextMetric.Data.SingleOrDefault(x => x.Text == "0");
            return textMetric;
        }
        public TextMetric GetTextMetric(ShowNoteAs showNoteAs)
        {
            var textMetric = showNoteAs == ShowNoteAs.Symbol
                ? TextMetric.Data.SingleOrDefault(x => x.Text == "𝅗")
                : TextMetric.Data.SingleOrDefault(x => x.Text == "0");
            return textMetric;
        }
        public TextMetric GetTextMetric(string text)
        {
            var textMetric = TextMetric.Data.SingleOrDefault(x => x.Text == text);
            return textMetric;
        }
        public void DebugWrite()
        {
            Debug.WriteLine($"Staff {StaffType} {Location}");
            foreach (var chord in ChordStructs)
                chord.DebugWrite();
        }
    }
}
